using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Utils.Events;

namespace CertificateOfOrigins.BL;

// The two reminder schedulers. Nothing on the REST surface reaches these — they exist for the Planar jobs
// CertificateOfOrigins.Planar.ReminderForImporterScheduler and .AuthenticationRequestReminder, which are the
// migrated form of the legacy BaseScheduleTask classes of the same names.
//
// Both follow the legacy split: a "which rows are due" read, then a "raise the event per row" write. Keeping that
// split means the job stays a thin shell and the decision logic stays testable in the BL, exactly as before.
public partial class AuthenticationRequestBl
{
    // Legacy AuthenticationRequestBL.GetImportAuthenticationRequestsForReminderForImporterScheduler.
    //
    // The legacy SP did the "no OPEN reminder task" check itself, with an OUTER APPLY onto Infrastructure.Tasks_Task
    // filtered on `TaskStatusID != 2` — anything but Closed. Tasks belongs to another service now, so the SP returns
    // every row past the reminder window and the filter happens here.
    //
    // 🛑 IsTaskExist does NOT mean "an open task exists". Infrastructure.usp_Tasks_IsTaskExist has every status
    // filter commented out and returns IsTaskInProgress as a COMPUTED column (TaskStatusID IN (1,4)), so it reports
    // CLOSED tasks too. Counting rows here — which is what this method did until 2026-09-07 — suppressed the
    // reminder for any request that had ever had one, because the event below opens with CloseOld and therefore
    // leaves exactly one closed task behind. Net effect: the reminder fired once per request and never again.
    //
    // Filtering on IsTaskInProgress restores the intent. It is not exact parity: legacy also treated Canceled(3)
    // and Suspended(5) as blocking, and IsTaskInProgress covers only Open(1)/InProgress(4).
    // TODO(confirm): exact parity needs the raw TaskStatusID (or a status filter) from the Tasks service; until
    // then a Canceled or Suspended reminder task will not block a new reminder, where legacy would have.
    public async Task<List<ReminderForImporterSchedulerDto>> GetImportAuthenticationRequestsForReminderForImporterScheduler()
    {
        var days = await parametersUtil.Get<int>(CertificateOfOriginsConsts.DaysForReminderForImporterSchedulerParameter);
        var candidates = await DataLayer.GetImportAuthenticationRequestsForReminderForImporterScheduler(days);
        if (candidates.Count == 0)
        {
            return candidates;
        }

        var tasksProxy = Resolve<ITasksProxy>();
        var due = new List<ReminderForImporterSchedulerDto>();
        foreach (var candidate in candidates)
        {
            // isTaskInProgress: true is the contract-correct request, but the SP ignores it today (its status
            // branches are commented out), so the result is filtered again here. Belt and braces on purpose:
            // drop either half and the reminder breaks in one direction or the other.
            var reminderTasks = await tasksProxy.IsTaskExist(
                candidate.DocumentId,
                (int)EEntityType.ImportAuthenticationRequest,
                [(int)ETaskType.SendReminderForImporter],
                isTaskInProgress: true);
            if (reminderTasks?.Any(task => task.IsTaskInProgress) == true)
            {
                continue;
            }

            due.Add(candidate);
        }

        return due;
    }

    // Legacy AuthenticationRequestBL.RaiseEventForReminderForImporterScheduler — opens the SendReminderForImporter
    // task for one request. CloseOld means a previous identical task is closed rather than duplicated.
    public async Task<bool> RaiseEventForReminderForImporterScheduler(ReminderForImporterSchedulerDto request)
    {
        var eventUtil = Resolve<IEventUtil>();
        var reminderEvent = eventUtil.CreatBuilder()
            .WithEventType((int)EEventType.NewReminderForImporterCreated)
            .WithEntityId(request.DocumentId)
            .WithEntityType((int)EEntityType.ImportAuthenticationRequest)
            .WithTitle(request.DocumentId.ToString())
            .WithOrganizationUnitId(request.OrganizationUnitId)
            .WithOrganizationUnitTypeId(CustomsCloud.InfrastructureCore.Interfaces.Shared.OrganizationUnitTypes.ClaliMakor)
            .AddRelatedEntity(request.AuthenticationFileId, (int)EEntityType.AuthenticationRequestFile)
            .WithTaskArguments(t => t.WithOpenTaskBehaviour(OpenTaskBehaviour.CloseOld))
            .Build();
        await eventUtil.RaiseEvent(reminderEvent);
        return true;
    }

    // Legacy BaseAuthenticationRequestBL.CallProcedureForReminderScheduler. The legacy materializer was a plain
    // 1:1 read with an "empty means null" quirk; an empty list is returned here instead, and the job treats both
    // the same way.
    //
    // The six offsets were platform global params (UDF ids 1600/1148/1941/1149/1667/1668). A service-owned SP may
    // not call that UDF, so they are read here and passed down. All six were already seeded as service parameters
    // with no consumer, and four of them carry exactly the values the legacy SP documented for their UDF ids.
    // TODO(confirm): the id→parameter pairing for 1600↔...Request3 and 1148↔...Request1 was inferred from those
    // matching values, not from a mapping document — worth one look at the parameters table before go-live.
    public async Task<List<AuthenticationRequestsForSchedulerDto>> GetAuthenticationRequestsForScheduler()
    {
        var firstReminder = await parametersUtil.Get<int>(CertificateOfOriginsConsts.SchedulerFirstReminderParameter);
        var secondReminder = await parametersUtil.Get<int>(CertificateOfOriginsConsts.SchedulerSecondReminderParameter);
        var finalDecision = await parametersUtil.Get<int>(CertificateOfOriginsConsts.SchedulerFinalDecisionParameter);
        var finalDecisionForCustomsHouse = await parametersUtil.Get<int>(CertificateOfOriginsConsts.SchedulerFinalDecisionForCustomsHouseParameter);
        var exportFirstReminder = await parametersUtil.Get<int>(CertificateOfOriginsConsts.SchedulerExportFirstReminderParameter);
        var exportSecondReminder = await parametersUtil.Get<int>(CertificateOfOriginsConsts.SchedulerExportSecondReminderParameter);

        var result = await DataLayer.GetAuthenticationRequestsForScheduler(
            firstReminder,
            secondReminder,
            finalDecision,
            finalDecisionForCustomsHouse,
            exportFirstReminder,
            exportSecondReminder);
        return result;
    }

    // Legacy AuthenticationRequestBL.RaiseSchedulerEvents — the reminder ladder. Returns how many tasks were
    // actually opened (rows whose task already existed are skipped), which is what the job reports as effected rows.
    public async Task<int> RaiseSchedulerEvents(List<AuthenticationRequestsForSchedulerDto> requests)
    {
        var count = 0;
        foreach (var request in requests)
        {
            var rung = ResolveReminderRung(request);
            if (rung is null)
            {
                continue;
            }

            count += await OpenReminderTask(request, rung.Value.EventType, rung.Value.TaskType);
        }

        return count;
    }

    // The legacy switch, lifted out so the branching is readable on its own. A delivery method outside the three
    // the legacy handled produces no event at all — that silence is deliberate in the original and kept here.
    private static (EEventType EventType, ETaskType TaskType)? ResolveReminderRung(AuthenticationRequestsForSchedulerDto request)
    {
        switch (request.DeliveryMethodId)
        {
            case (int)EDeliveryMethod.PostedMailing:
            case (int)EDeliveryMethod.SentByEmailRequest:
                if (request.IsImport && request.SendThreeMonthsReminder)
                {
                    return (EEventType.ReminderNotice3Months, ETaskType.VendorReminderNotice3Months);
                }

                if (!request.IsImport)
                {
                    return (EEventType.ExportReminderNotice6Months, ETaskType.ExportReminderNotice6Months);
                }

                return request.IsVendor
                    ? (EEventType.ImporterReminderNotice6Months, ETaskType.SendImporterMsgFromVendorReference)
                    : (EEventType.ReminderNotice6Months, ETaskType.ReminderNotice6Months);

            case (int)EDeliveryMethod.FirstRemindSent:
                if (!request.IsImport)
                {
                    return (EEventType.ExportReminderNotice10Months, ETaskType.ExportReminderNotice10Months);
                }

                return request.IsVendor
                    ? (EEventType.FinalDecisionInTheFile, ETaskType.FinalDecisionInCase)
                    : (EEventType.ReminderNotice10Months, ETaskType.ReminderNotice10Months);

            default:
                return null;
        }
    }

    // Legacy OpenTask. The entity type follows IsImport, because the SP's Id column means an authentication-file id
    // on import rows and an export-request id on export rows.
    private async Task<int> OpenReminderTask(AuthenticationRequestsForSchedulerDto request, EEventType eventType, ETaskType taskType)
    {
        var entityType = request.IsImport
            ? EEntityType.AuthenticationRequestFile
            : EEntityType.ExportDocumentAuthenticationRequest;

        var tasksProxy = Resolve<ITasksProxy>();
        var existing = await tasksProxy.IsTaskExist(request.Id, (int)entityType, [(int)taskType]);
        if (existing is { Count: > 0 })
        {
            return 0;
        }

        var eventUtil = Resolve<IEventUtil>();
        var reminderEvent = eventUtil.CreatBuilder()
            .WithEventType((int)eventType)
            .WithEntityId(request.Id)
            .WithEntityType((int)entityType)
            .WithTitle(request.Id.ToString())
            .WithOrganizationUnitId(request.OrganizationUnitId)
            .WithTaskArguments(t => t.WithOpenTaskBehaviour(OpenTaskBehaviour.CloseOld))
            .Build();
        await eventUtil.RaiseEvent(reminderEvent);
        return 1;
    }
}

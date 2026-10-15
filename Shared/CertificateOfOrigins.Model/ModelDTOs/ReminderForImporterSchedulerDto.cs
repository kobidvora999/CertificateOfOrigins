namespace CertificateOfOrigins.Model.ModelDTOs;

// One import-authentication request due an importer reminder (legacy ReminderForImporterSchedulerDTO), returned by
// dbo.GetImportAuthenticationRequestsForReminderForImporterScheduler and consumed by the ReminderForImporterScheduler
// Planar job.
//
// DocumentTypeName and DocumentTitle come from Infrastructure.Docs_Document / Shared.Docs_enum_DocumentType, which
// this service does not own — the migrated SP drops those JOINs, so both arrive NULL. Nothing in the job reads them
// (the event is keyed on DocumentID / AuthenticationFileID / OrganizationUnitID), so they are carried for parity
// only and are not enriched.
public class ReminderForImporterSchedulerDto
{
    public int DocumentId { get; set; }

    public int AuthenticationFileId { get; set; }

    public int LeadDocumentId { get; set; }

    public int DocumentTypeId { get; set; }

    public string? DocumentTypeName { get; set; }

    public string? DocumentTitle { get; set; }

    public int OrganizationUnitId { get; set; }
}

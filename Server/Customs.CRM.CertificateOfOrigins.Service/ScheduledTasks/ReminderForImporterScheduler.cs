using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CRM.CertificateOfOrigins.BL;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using Microsoft.Practices.Unity;

namespace Customs.CRM.CertificateOfOrigins.Service.ScheduledTasks
{
    public class ReminderForImporterScheduler : BaseScheduleTask
    {
        public override int ExecuteTask()
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var bl = new AuthenticationRequestBL(uow);
                var authenticationRequestsForSchedulerDTO = bl.GetImportAuthenticationRequestsForReminderForImporterScheduler();

                foreach (var dto in authenticationRequestsForSchedulerDTO)
                {
                    bl.RaiseEventForReminderForImporterScheduler(dto);
                }
                
                return authenticationRequestsForSchedulerDTO.Count;
            }
        }
    }
}

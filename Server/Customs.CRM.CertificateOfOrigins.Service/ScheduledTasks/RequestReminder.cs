using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CRM.CertificateOfOrigins.BL;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.Environment.Const;
using Microsoft.Practices.Unity;

namespace Customs.CRM.CertificateOfOrigins.Service.ScheduledTasks
{
    /// <summary>
    /// RequestReminder
    /// </summary>
    public class AuthenticationRequestReminder : BaseScheduleTask
    {
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public override int ExecuteTask()
        {
            using (var uow = Container.Resolve<IUnitOfWork>(CRMConsts.CertificateOfOriginsUnitOfWork))
            {
                var baseBl = new BaseAuthenticationRequestBL(uow);
                var authenticationRequestsForSchedulerDTO = baseBl.CallProcedureForReminderScheduler();
                if (authenticationRequestsForSchedulerDTO.IsNullOrEmpty())
                    return 0;

                var bl = new AuthenticationRequestBL(uow);
                var count = bl.RaiseSchedulerEvents(authenticationRequestsForSchedulerDTO);

                return count;
            }


        }
    }
}

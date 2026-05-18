using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.CRM.CertificateOfOrigins.InternalCommon;
using Customs.CRM.CertificateOfOrigins.InternalCommon.Common;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using Microsoft.Data.Extensions;

namespace Customs.CRM.CertificateOfOrigins.BL
{
  public  class BaseAuthenticationRequestBL : BaseBL
    {
      public BaseAuthenticationRequestBL(IUnitOfWork uow) : base(uow)
      {
      }

      public List<AuthenticationRequestsForSchedulerDTO> CallProcedureForReminderScheduler()
      {
          var spFilter = new ResultSetFilter<List<AuthenticationRequestsForSchedulerDTO>>
          {
              StoredProcedureName = CertificateOfOriginsConsts.GetAuthenticationRequestsForScheduler,
              MaterializeFunction = MaterializeGetFileForReminderScheduler,

          };
          var importAuthenticationFile = _uow.Repository.ExecuteResultSetFunction(spFilter);
          return importAuthenticationFile;
      }

      private List<AuthenticationRequestsForSchedulerDTO> MaterializeGetFileForReminderScheduler(DbDataReader reader)
      {
          var importAuthenticationFile =
              reader.Materialize<AuthenticationRequestsForSchedulerDTO>().ToList();
          if (importAuthenticationFile.IsNullOrEmpty()) return null;
          return importAuthenticationFile;
      }

    }
}

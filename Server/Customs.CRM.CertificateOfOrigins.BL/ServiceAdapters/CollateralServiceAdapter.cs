using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.FinanceInfr.Collateral.ExternalCommon.Common;
using Customs.FinanceInfr.Collateral.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Microsoft.Practices.Unity;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public class CollateralServiceAdapter : BaseServiceAdapter<ICollateralExternalProxy>, ICollateralServiceAdapter

    {
        public List<CollateralRequestDetails> ChangeTempCollateralRequest(List<ChangeTempCollateralRequestDTO> changeTempCollateralRequestDTO)
        {
            ArgumentValidator.AssertNotNull(changeTempCollateralRequestDTO, "List<ChangeTempCollateralRequestDTO>");
            if (changeTempCollateralRequestDTO.Count > 0)
                return ExternalProxy.ChangeTempCollateralRequest(changeTempCollateralRequestDTO, true);
            return null;
        }

        public List<CollateralRequestDTO> GetCollateralRequest(EEntityType? entityType, int? entityID, int? collateralRequestID)
        {
            var collateralExternalProxy = Container.Resolve<ICollateralExternalProxy>();
            return collateralExternalProxy.GetCollateralRequest(entityType, entityID, collateralRequestID);
        }

        public List<int> GetCollateralRequestIDsByRelatedEntityDTO(RelatedEntityDTO relatedEntityDTO)
        {
            var collateralExternalProxy = Container.Resolve<ICollateralExternalProxy>();
            return collateralExternalProxy.GetCollateralRequestIDsByRelatedEntityDTO(relatedEntityDTO);
        }

       public bool GrantAllCollateralRequests(List<GrantCollateralRequestDTO> grantCollateralRequestsDTO)
       {
           var collateralExternalProxy = Container.Resolve<ICollateralExternalProxy>();
           return collateralExternalProxy.GrantAllCollateralRequests(grantCollateralRequestsDTO);
       }

       public void DebitCreditCollateralRequest(DebitCreditFilter debitCreditFilter)

       {
           var collateralExternalProxy = Container.Resolve<ICollateralExternalProxy>();
          collateralExternalProxy.DebitCreditCollateralRequest(debitCreditFilter);
       }


    }
}

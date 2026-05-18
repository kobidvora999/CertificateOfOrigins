using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.FinanceInfr.Collateral.ExternalCommon.Common;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public interface  ICollateralServiceAdapter
    {

        /// <summary>
        /// Changes the temp collateral request.  
        /// </summary>
        /// <param name="changeTempCollateralRequestDTO">The collateral requests DTO.</param>
        /// <returns></returns>
        List<CollateralRequestDetails> ChangeTempCollateralRequest(List<ChangeTempCollateralRequestDTO> changeTempCollateralRequestDTO);

        /// <summary>
        /// Gets the collateral request.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="entityID">The entity ID.</param>
        /// <param name="collateralRequestID"></param>
        /// <returns></returns>
        List<CollateralRequestDTO> GetCollateralRequest(EEntityType? entityType, int? entityID, int? collateralRequestID);

        List<int> GetCollateralRequestIDsByRelatedEntityDTO(RelatedEntityDTO relatedEntityDTO);

        bool GrantAllCollateralRequests(List<GrantCollateralRequestDTO> grantCollateralRequestsDTO);

        void DebitCreditCollateralRequest(DebitCreditFilter debitCreditFilter);
    }
}

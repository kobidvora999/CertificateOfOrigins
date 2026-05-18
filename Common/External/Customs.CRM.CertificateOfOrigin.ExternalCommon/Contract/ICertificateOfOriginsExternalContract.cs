using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Customs.CRM.CertificateOfOrigins.ExternalCommon.Common;
using MalamTeam.Infrastructure.GeneralServices.CommonBase;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;

namespace Customs.CRM.CertificateOfOrigins.ExternalCommon.Contract
{
    /// <summary>
    /// CertificateOfOrigins external contract
    /// </summary>
    [ServiceContract]
    [InfContractExtension(ESubSystem.CRM)]
    public interface ICertificateOfOriginsExternalContract : IServiceContract
    {
        #region Sync Services
        /// <summary>
        ///
        /// </summary>
        /// <param name="filter">The filter.</param>
		[OperationContract]
		[InfOperationExtension(IsCached = true, IsSyncOperation = true)]
		[FaultContract(typeof(InfExceptionData))]
		bool TempSync(int i);
        #endregion Sync Services

        /// <summary>
        /// Converts the specified connected entity.
        /// </summary>
        /// <param name="connectedEntity">The connected entity.</param>
        /// <returns></returns>
        [OperationContract]
        [InfOperationExtension(IsSyncOperation = true)]
        [FaultContract(typeof(InfExceptionData))]
        VirtualEntity Convert(ConnectedEntity connectedEntity);

        /// <summary>
        /// Handles the authentication request delivery sent.
        /// </summary>
        /// <param name="raiseEventArgs">The <see cref="RaiseEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        [OperationContract]
        [InfOperationExtension(IsSyncOperation = true)]
        [FaultContract(typeof(InfExceptionData))]
        bool HandleAuthenticationRequestDeliverySent(RaiseEventArgs raiseEventArgs);

       
        [OperationContract(IsOneWay = true)]
        void UpdateCetrificateOfOrigins(UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO);

        [OperationContract]
        [InfOperationExtension(IsSyncOperation = true)]
        [FaultContract(typeof(InfExceptionData))]
        int? GetCertificateOfOriginID(string certificateNumber);

        [OperationContract]
        [InfOperationExtension(IsSyncOperation = true)]
        [FaultContract(typeof(InfExceptionData))]
        List<GoodsItemCerificateDTO> GetGoodsItemCerificateDTO(List<GoodsItemCerificateDTO> goodsItemCerificateDTOs);

        [OperationContract]
        [InfOperationExtension(IsSyncOperation = true)]
        [FaultContract(typeof(InfExceptionData))]
        bool SaveCertificateOfOriginAttachments(SaveCertificateAttachmentsArgsDTO saveCertificateAttachmentsArgsDTO);
    }
}

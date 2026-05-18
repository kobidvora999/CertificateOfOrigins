//------------------------------------------------------------------------------
// <inf-auto-generated>
//     *version: 10.0
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </inf-auto-generated>
//------------------------------------------------------------------------------

using System;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Communication;
namespace Customs.CRM.CertificateOfOrigins.ExternalProxy
{
	/// <summary>
	/// Generated proxy interface
	/// </summary>
	public partial interface ICertificateOfOriginsExternalProxy
	{
		/// <summary>
		/// TempSync auto generated wrapper
		/// </summary>
		System.Boolean TempSync(System.Int32 i);
		/// <summary>
		/// Convert auto generated wrapper
		/// </summary>
		MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities.VirtualEntity Convert(MalamTeam.Infrastructure.GeneralServices.EAISchema.ConnectedEntity connectedEntity);
		/// <summary>
		/// HandleAuthenticationRequestDeliverySent auto generated wrapper
		/// </summary>
		System.Boolean HandleAuthenticationRequestDeliverySent(MalamTeam.Infrastructure.GeneralServices.CommonBase.RaiseEventArgs raiseEventArgs);
		/// <summary>
		/// UpdateCetrificateOfOrigins auto generated wrapper
		/// </summary>
		string UpdateCetrificateOfOrigins(Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.UpdateCetificateOfOriginsDTO updateCetificateOfOriginsDTO);
		/// <summary>
		/// GetCertificateOfOriginID auto generated wrapper
		/// </summary>
		System.Nullable<System.Int32> GetCertificateOfOriginID(System.String certificateNumber);
		/// <summary>
		/// GetGoodsItemCerificateDTO auto generated wrapper
		/// </summary>
		System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO> GetGoodsItemCerificateDTO(System.Collections.Generic.List<Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.GoodsItemCerificateDTO> goodsItemCerificateDTOs);
		/// <summary>
		/// SaveCertificateOfOriginAttachments auto generated wrapper
		/// </summary>
		System.Boolean SaveCertificateOfOriginAttachments(Customs.CRM.CertificateOfOrigins.ExternalCommon.Common.SaveCertificateAttachmentsArgsDTO saveCertificateAttachmentsArgsDTO);
	}
}

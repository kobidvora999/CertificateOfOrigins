using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Customs.Inf.MMI.Common.CAL;
using Customs.Shared.Entities;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Interfaces;

namespace Customs.CRM.CertificateOfOrigins.ExternalCommon.Common
{
    public class InvoiceDetailDTO
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Title
        {
            get { return Invoicenumber + " - " + VendorName; }
        }
        [DataMember]
        public DateTime? IssuingDate { get; set; }
        [DataMember]
        public string Invoicenumber { get; set; }

        [DataMember]
        public int SequenceNumber { get; set; }

        [DataMember]
        public int? VendorID { get; set; }

        [DataMember]
        public string VendorName
        {
            get { return GetVendorName(); }
        }

        [DataMember]
        public int? InvoiceCountryID { get; set; }

        [DataMember]
        public decimal? GoodsItemTaxDifferences { get; set; }

        [DataMember]
        public List<GoodsItemDetailDTO> GoodsItemDetailDTOList { get; set; }

        public string GetVendorName()
        {
            if (!VendorID.HasValue) return null;
            var systemTablesUtil = CALFacade.Instance.ClientUnityContainer.Resolve(typeof(ISystemTablesUtilBase), null) as ISystemTablesUtilBase;
            if (systemTablesUtil == null) return null;
            return systemTablesUtil.GetCodeById<VendorLOV>(VendorID.Value).Name;
        }

    }

    public class GoodsItemDetailDTO
    {
        [DataMember]
        public int SequenceNumber { get; set; }

        [DataMember]
        public int GoodsItemID { get; set; }

        [DataMember]
        public int? OriginCountryID { get; set; }

        [DataMember]
        public int CustomsItemID { get; set; }

        [DataMember]
        public decimal GoodsItemStatisticalQuantity { get; set; }

        [DataMember]
        public string GoodsItemDescription { get; set; }
    }
}
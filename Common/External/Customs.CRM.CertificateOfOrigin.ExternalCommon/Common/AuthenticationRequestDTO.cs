using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MalamTeam.Infrastructure.GeneralServices.CommonBase;

namespace Customs.CRM.CertificateOfOrigins.ExternalCommon.Common
{
    /// <summary>
    /// AuthenticationRequestDTO
    /// </summary>
    public class AuthenticationRequestDTO : BaseEntity
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationRequestDTO"/> class.
        /// </summary>
        public AuthenticationRequestDTO()
        {
            Init();
        }
        #endregion Ctor

        #region Properties
        [DataMember]
        public DateTime? LeadDocumentSubmitDate { get; set; }

        [DataMember]
        public int OrganizationUnitID { get; set; }

        [DataMember]
        public int LeadDocumentID { get; set; }

        [DataMember]
        public int DocumentaryInspectionID { get; set; }

        [DataMember]
        public int? ImporterID { get; set; }

        [DataMember]
        public int AuthenticationRequestInitiatorUserID { get; set; }

        [DataMember]
        public List<InvoiceDetailDTO> InvoiceDetailDTOList { get; set; }
               
        #endregion Properties

        #region Private Method

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Init()
        {
            LeadDocumentID = GetHashCode();
        }

        #endregion Private Method
    }

    
}

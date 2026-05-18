using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customs.CRM.CertificateOfOrigins.InternalCommon.Common
{
    /// <summary>
    /// AttachmentDocumentDTO
    /// </summary>
   public class AttachmentDocumentDTO
    {
        /// <summary>
        /// Gets or sets the document identifier.
        /// </summary>
        /// <value>
        /// The document identifier.
        /// </value>
       public int DocumentID { get; set; }

       /// <summary>
       /// Gets or sets the document title.
       /// </summary>
       /// <value>
       /// The document title.
       /// </value>
       public string DocumentTitle { get; set; }
    }

 
}

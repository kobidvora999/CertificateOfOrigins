using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customs.CRM.CertificateOfOrigins.InternalCommon.Common
{
    /// <summary>
    /// AuthenticationRequestsForSchedulerDTO
    /// </summary>
   public class AuthenticationRequestsForSchedulerDTO : IEnumerable
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
       public int ID { get; set; }
       /// <summary>
       /// Gets or sets the delivery method identifier.
       /// </summary>
       /// <value>
       /// The delivery method identifier.
       /// </value>
       public int DeliveryMethodID { get; set; }
       /// <summary>
       /// Gets or sets a value indicating whether [is import].
       /// </summary>
       /// <value>
       ///   <c>true</c> if [is import]; otherwise, <c>false</c>.
       /// </value>
       public bool IsImport { get; set; }
       /// <summary>
       /// Gets or sets a value indicating whether [is import].
       /// </summary>
       /// <value>
       ///   <c>true</c> if [is import]; otherwise, <c>false</c>.
       /// </value>
       public bool SendThreeMonthsReminder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [Is Vendor].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [Is Vendor]; otherwise, <c>false</c>.
        /// </value>
        public bool IsVendor { get; set; }

        public int OrganizationUnitID { get; set; }
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}

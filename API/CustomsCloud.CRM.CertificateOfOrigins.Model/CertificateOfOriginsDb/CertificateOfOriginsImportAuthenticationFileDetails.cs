using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginsDb;

// CRM.CertificateOfOrigins_ImportAuthenticationFileDetails (EDMX: key = ID identity). Onboarded for the delivery
// writers (#22-24) which advance the file's status/delivery-method. Writes go through Context + ExecuteUpdateAsync
// (targeted columns), so no row is loaded; the full column set is declared for completeness.
[Table("CertificateOfOrigins_ImportAuthenticationFileDetails", Schema = "CRM")]
public partial class CertificateOfOriginsImportAuthenticationFileDetails
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("State")]
    public int State { get; set; }

    // SQL rowversion — DB-generated; excluded from INSERT/UPDATE (the file INSERT in CreateNewAuthenticationFile
    // would otherwise fail trying to write this column).
    [Column("TimeStamp")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public byte[]? TimeStamp { get; set; }

    [Column("CreateDate")]
    public DateTime CreateDate { get; set; }

    [Column("CreateUserID")]
    public int CreateUserId { get; set; }

    [Column("UpdateDate")]
    public DateTime UpdateDate { get; set; }

    [Column("UpdateUserID")]
    public int UpdateUserId { get; set; }

    [Column("AuthenticationFileStatusID")]
    public int AuthenticationFileStatusId { get; set; }

    [Column("Notes")]
    public string? Notes { get; set; }

    [Column("PostalAdress")]
    public string? PostalAdress { get; set; }

    [Column("DeliveryMethodID")]
    public int DeliveryMethodId { get; set; }

    [Column("EmailAdress")]
    public string? EmailAdress { get; set; }

    [Column("ReminderMethodID")]
    public int ReminderMethodId { get; set; }

    [Column("RequestCountryID")]
    public int RequestCountryId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("UserNameIssuingLetter")]
    public string? UserNameIssuingLetter { get; set; }

    [Column("LastDelivery")]
    public DateTimeOffset? LastDelivery { get; set; }

    [Column("ImporterContactingReasonID")]
    public int? ImporterContactingReasonId { get; set; }

    [Column("FirstProvideContactDate")]
    public DateTimeOffset? FirstProvideContactDate { get; set; }
}

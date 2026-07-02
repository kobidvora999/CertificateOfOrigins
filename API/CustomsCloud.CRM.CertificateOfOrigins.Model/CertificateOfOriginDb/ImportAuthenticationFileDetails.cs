using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CustomsCloud.CRM.CertificateOfOrigins.Model.CertificateOfOriginDb;

[Table("CertificateOfOrigins_ImportAuthenticationFileDetails", Schema = "CRM")]
public class ImportAuthenticationFileDetails
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int State { get; set; }

    [Timestamp]
    public byte[] TimeStamp { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreateDate { get; set; }

    [Column("CreateUserID")]
    public int CreateUserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdateDate { get; set; }

    [Column("UpdateUserID")]
    public int UpdateUserId { get; set; }

    [Column("AuthenticationFileStatusID")]
    public int AuthenticationFileStatusId { get; set; }

    [StringLength(512)]
    public string? Notes { get; set; }

    [StringLength(255)]
    public string PostalAdress { get; set; } = null!;

    [Column("DeliveryMethodID")]
    public int DeliveryMethodId { get; set; }

    [StringLength(255)]
    public string? EmailAdress { get; set; }

    [Column("ReminderMethodID")]
    public int ReminderMethodId { get; set; }

    [Column("RequestCountryID")]
    public int RequestCountryId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(255)]
    public string UserNameIssuingLetter { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? LastDelivery { get; set; }

    [Column("ImporterContactingReasonID")]
    public int? ImporterContactingReasonId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FirstProvideContactDate { get; set; }
}

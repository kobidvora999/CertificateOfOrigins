namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Result of CreateNewAuthenticationFile — the newly created import authentication-request file (the legacy WCF
// echoed back the created file entity). CustomerIDList is intentionally omitted (transient/unused, developer
// decision 2026-07-30). OrganizationUnitId + CustomerId mirror the legacy transient (non-column) fields.
public class CreateNewAuthenticationFileResultDto
{
    public int Id { get; set; }

    public int AuthenticationFileStatusId { get; set; }

    public int OrganizationUnitId { get; set; }

    public int RequestCountryId { get; set; }

    public int CustomerId { get; set; }

    public int DeliveryMethodId { get; set; }

    public int ReminderMethodId { get; set; }

    public string? EmailAdress { get; set; }

    public DateTimeOffset CreateDate { get; set; }
}

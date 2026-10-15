namespace CertificateOfOrigins.Model.ModelDTOs;

// One authentication-request file due a reminder (legacy AuthenticationRequestsForSchedulerDTO), returned by
// dbo.GetAuthenticationRequestsForScheduler and consumed by the AuthenticationRequestReminder Planar job.
//
// The three flags below are what pick the event/task pair, so they carry the whole branching logic of the job:
// DeliveryMethodId chooses the rung of the ladder, and IsImport/IsVendor/SendThreeMonthsReminder choose the variant
// within it. Id therefore means different things per row — an ImportAuthenticationFileDetails id when IsImport,
// an ExportDocumentAuthenticationRequest id otherwise — which is also why the event's entity type is chosen per row.
public class AuthenticationRequestsForSchedulerDto
{
    public int Id { get; set; }

    public int DeliveryMethodId { get; set; }

    public bool IsImport { get; set; }

    public bool SendThreeMonthsReminder { get; set; }

    public bool IsVendor { get; set; }

    public int OrganizationUnitId { get; set; }
}

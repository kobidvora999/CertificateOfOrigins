namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Legacy MessageDestinationDTO — one recipient of a message (Message-Management service). Only UserId is populated
// by this service (the message goes to the current user); the org-unit fields are part of the contract.
public class MessageDestinationDto
{
    public int? UserId { get; set; }

    public int? OrganizationUnitId { get; set; }

    public int? OrganizationUnitTypeId { get; set; }
}

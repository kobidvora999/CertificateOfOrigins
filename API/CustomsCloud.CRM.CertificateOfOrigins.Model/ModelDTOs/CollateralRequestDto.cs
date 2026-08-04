namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Collateral request returned by the Collateral microservice (legacy CollateralRequestDTO). Enum-typed fields are
// carried as their numeric codes (the wire values). TODO(blocking): confirm the Collateral service's response
// contract + route before ROLLOUT.
public class CollateralRequestDto
{
    public int CollateralType { get; set; }

    public decimal AmountToGrant { get; set; }

    public int CustomerId { get; set; }

    public int? CustomerActivityTypeId { get; set; }

    public int CollateralRequestId { get; set; }

    public int InitiatorUserId { get; set; }

    public bool IsAutoDebit { get; set; }

    public bool IsImmediateGrant { get; set; }

    public int OrganizationUnitType { get; set; }

    public int OrganizationUnitId { get; set; }

    public VirtualEntityDto? RelatedEntity { get; set; }

    public int RelatedEntityType { get; set; }

    public DateTime RequestedValidity { get; set; }

    public DateTime CollateralRequestExpiryDate { get; set; }

    public int? GoodsItemId { get; set; }

    public int? InvoiceId { get; set; }

    public int? LeadDocumentId { get; set; }

    public int? LeadingFileId { get; set; }

    public int? AssignedUserId { get; set; }

    public int? StateId { get; set; }

    public int CollateralRequestStatus { get; set; }

    public string? CollateralRequestStatusName { get; set; }

    public bool IsAutoCollateral { get; set; }

    public string? EntityExternalId { get; set; }

    public bool IsCollateraNotCoverd { get; set; }

    public bool IsCollateraDebitCommitted { get; set; }
}

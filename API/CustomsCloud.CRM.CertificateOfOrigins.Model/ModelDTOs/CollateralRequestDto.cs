namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Mirrors Customs.FinanceInfr.Collateral CollateralRequestDTO.
// Enum-typed fields are kept as int (over the wire they are ints) — the original
// enum is noted in a comment next to each field.
public class CollateralRequestDto
{
    public int CollateralType { get; set; } // ECollateralType
    public decimal AmountToGrant { get; set; }
    public int CustomerId { get; set; }
    public int? CustomerActivityTypeId { get; set; }
    public int? UpdateCustomerId { get; set; }
    public int CollateralRequestId { get; set; }
    public int InitiatorUserId { get; set; }
    public bool IsAutoDebit { get; set; }
    public bool IsImmediateGrant { get; set; }
    public bool IsThirdPartyGuaranteeAllowed { get; set; }
    public int OrganizationUnitType { get; set; } // EOrganizationUnitType
    public int OrganizationUnitId { get; set; }
    public VirtualEntityDto? RelatedEntity { get; set; }
    public int RelatedEntityType { get; set; } // EEntityType
    public DateTime RequestedValidity { get; set; }
    public DateTime CollateralRequestExpiryDate { get; set; }
    public int CollateralRelatedEntityProcessType { get; set; } // ECollateralRelatedEntityProcessType
    public int? GoodsItemId { get; set; }
    public int? InvoiceId { get; set; }
    public int? LeadDocumentId { get; set; }
    public int? LeadDocumentType { get; set; } // EEntityType?
    public int? LeadingFileId { get; set; }
    public int? LeadingFileType { get; set; } // EEntityType?
    public int? AssignedUserId { get; set; }
    public int? StateId { get; set; }
    public int? SpecializationId { get; set; }
    public List<CollateralConditioningDto> CollateralConditions { get; set; } = new();
    public int CollateralRequestStatus { get; set; } // ECollateralRequestStatus
    public string? CollateralRequestStatusName { get; set; }
    public int? CancelCollateralRequestCreateUserId { get; set; }
    public int? CancelCollateralRequestCustomerId { get; set; }
    public int? CancelCollateralRequestUserId { get; set; }
    public bool IsAutoCollateral { get; set; } = true;
    public string? EntityExternalId { get; set; }
    public int? AmendmentStatus { get; set; } // ECollateralRequestAmendmentStatus?
    public DateTime? AmendmentValidityDate { get; set; }
    public int? CollateralOriginTypeId { get; set; }
    public int? TradeLeviId { get; set; }
    public bool IsAllowedAcceptingGuaranteeWithLowerValidity { get; set; } = true;
    public bool IsCollateraNotCoverd { get; set; }
    public bool IsCollateraDebitCommitted { get; set; }
}

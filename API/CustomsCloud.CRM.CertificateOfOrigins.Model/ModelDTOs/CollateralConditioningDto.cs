namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Mirrors Customs.FinanceInfr.Collateral CollateralConditioningDTO.
public class CollateralConditioningDto
{
    public int Id { get; set; }
    public int CollateralRequestId { get; set; }
    public int ReturnConditionType { get; set; } // EReturnCondition
    public decimal ConditionValue { get; set; }
    public decimal? AmendmentAmount { get; set; }
    public int? Status { get; set; }
}

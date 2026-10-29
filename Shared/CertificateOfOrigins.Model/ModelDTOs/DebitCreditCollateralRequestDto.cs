namespace CertificateOfOrigins.Model.ModelDTOs;

// Legacy DebitCreditFilter (Customs.FinanceInfr.Collateral.ExternalCommon.Common) — debits/credits one collateral
// request. Raised when an authentication file lands on WrongAuthenticationAnswer: the answer was rejected, so the
// guarantee is collected rather than released.
//
// The legacy caller only ever set CollateralRequestId (one call per collateral id on the file). The other two
// members are carried so the wire shape matches the Collateral service's contract.
public class DebitCreditCollateralRequestDto
{
    public int? CollateralRequestId { get; set; }

    public int? LeadDocumentId { get; set; }

    public bool IsCreateAutomaticAnswerForCollateralRequest { get; set; }
}

using CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;
using FluentValidation;

namespace CustomsCloud.CRM.CertificateOfOrigins.BL.Validations;

// The service had no validation layer at all, so a malformed body reached the database and the resulting FK
// violation escaped as an unhandled HTTP 500 (POST /CertificateOfOrigins with "{}" — found by the
// internal-workload Negative collection, 2026-08-25).
//
// The rules below are deliberately limited to what the DB itself requires: every column checked here is
// NOT NULL on CRM.CertificateOfOrigins_CertificateOfOrigin, or is a code the row cannot be written without.
// They are NOT a business-rule rewrite — the message-validation engine (GetPC_MSG2280_2281) keeps owning the
// in-band business exceptions. Verified against the existing internal-workload payloads before being added, so
// valid traffic is unaffected.
public class SaveCertificateOfOriginRequestValidator : AbstractValidator<SaveCertificateOfOriginRequestDto>
{
    public SaveCertificateOfOriginRequestValidator()
    {
        RuleFor(x => x.TypeId)
            .GreaterThan(0)
            .WithMessage("TypeId is required (CRM.CertificateOfOrigins_CertificateOfOrigin.TypeID is NOT NULL).");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required (Title is NOT NULL).");

        RuleFor(x => x.CertificateNumber)
            .NotEmpty()
            .WithMessage("CertificateNumber is required (CertificateNumber is NOT NULL).");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .WithMessage("CustomerId is required (CustomerID is NOT NULL).");

        RuleFor(x => x.OrganizationUnitId)
            .GreaterThan(0)
            .WithMessage("OrganizationUnitId is required (OrganizationUnitID is NOT NULL).");

        // IsInEnum() only applies to enum-typed properties; these columns are plain ints.
        RuleFor(x => x.CertificateOfOriginStatusId)
            .Must(v => Enum.IsDefined(typeof(ECertificateOfOriginStatus), v))
            .WithMessage("CertificateOfOriginStatusId must be a defined ECertificateOfOriginStatus value.");

        RuleFor(x => x.RequestReasonCode)
            .Must(v => Enum.IsDefined(typeof(ERequestReason), v))
            .WithMessage("RequestReasonCode must be a defined ERequestReason value.");
    }
}

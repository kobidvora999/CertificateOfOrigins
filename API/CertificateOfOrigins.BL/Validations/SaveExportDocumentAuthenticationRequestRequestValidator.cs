using CertificateOfOrigins.Model.ModelDTOs;
using FluentValidation;

namespace CertificateOfOrigins.BL.Validations;

// ExportDocumentAuthenticationRequest carries a [Timestamp] row version, so EF adds `WHERE TimeStamp = @original` to the
// parent UPDATE. The legacy save was last-writer-wins (its EDMX has no ConcurrencyMode="Fixed"), so a client could update
// without a TimeStamp; here such a body matched no row and came back as an unexplained 409 (parity finding H-2).
//
// The optimistic concurrency stays - it is the platform convention and a stale TimeStamp is still a 409 - but a missing one
// is a malformed request, answered as such: a 400 that names the field.
public class SaveExportDocumentAuthenticationRequestRequestValidator : AbstractValidator<SaveExportDocumentAuthenticationRequestRequestDto>
{
    public SaveExportDocumentAuthenticationRequestRequestValidator()
    {
        RuleFor(x => x.TimeStamp)
            .NotEmpty()
            .When(x => x.Id > 0)
            .WithMessage("TimeStamp is required to update an existing request: send back the value returned when it was read (optimistic concurrency).");
    }
}

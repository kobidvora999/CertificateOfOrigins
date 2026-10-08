using CertificateOfOrigins.Model.ModelDTOs;
using FluentValidation;

namespace CertificateOfOrigins.BL.Validations;

// ExportDocumentAuthenticationRequest carries a [Timestamp] row version, so EF adds `WHERE TimeStamp = @original` to the
// parent UPDATE. The legacy save was last-writer-wins (its EDMX has no ConcurrencyMode="Fixed"), so a client could update
// without a TimeStamp; here such a body matched no row and came back as an unexplained 409 (parity finding H-2).
//
// The optimistic concurrency stays - it is the platform convention and a stale TimeStamp is still a 409 - but a missing one
// is a malformed request, answered as such: a 400 that names the field.
//
// Decided (parity H-2, 2026-10-08) - the client contract: send back the TimeStamp of the LAST response (every Save returns
// the entity with its new row version, as the legacy WPF screen kept the returned entity; the screen saves twice in a row
// after the send-letter dialog), and treat a 409 as "changed by someone else, reload".
public class SaveExportDocumentAuthenticationRequestRequestValidator : AbstractValidator<SaveExportDocumentAuthenticationRequestRequestDto>
{
    public SaveExportDocumentAuthenticationRequestRequestValidator()
    {
        RuleFor(x => x.TimeStamp)
            .NotEmpty()
            .When(x => x.Id > 0)
            .WithMessage("TimeStamp is required to update an existing request: send back the value returned when it was read (optimistic concurrency).");

        // Legacy SaveExportDocumentAuthenticationRequest ended with `entity.StatusID.Value`, so a request without a status
        // failed every time - but only after it was saved and its status events raised. A status is a precondition: it is
        // rejected up front, before any write, event or message (parity finding H-6; stricter than legacy, which saved first).
        RuleFor(x => x.StatusId)
            .NotNull()
            .WithMessage("StatusId is required.");
    }
}

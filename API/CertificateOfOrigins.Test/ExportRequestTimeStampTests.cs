using CertificateOfOrigins.BL.Validations;
using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.Test;

// Parity finding H-2: the export request keeps its optimistic concurrency, but an update that omits the row version is a
// malformed request (400 naming the field), not an unexplained 409 from a UPDATE that matched no row.
[TestFixture]
public class ExportRequestTimeStampTests
{
    private readonly SaveExportDocumentAuthenticationRequestRequestValidator _validator = new();

    [Test]
    public void UpdatingWithoutATimeStampIsRejectedWithAClearMessage()
    {
        var result = _validator.Validate(new SaveExportDocumentAuthenticationRequestRequestDto { Id = 5, TimeStamp = null });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors[0].PropertyName, Is.EqualTo(nameof(SaveExportDocumentAuthenticationRequestRequestDto.TimeStamp)));
            Assert.That(result.Errors[0].ErrorMessage, Does.Contain("TimeStamp is required"));
        });
    }

    [Test]
    public void AnEmptyTimeStampIsAlsoRejected()
    {
        var result = _validator.Validate(new SaveExportDocumentAuthenticationRequestRequestDto { Id = 5, TimeStamp = [] });

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void UpdatingWithTheRowVersionPasses()
    {
        var result = _validator.Validate(new SaveExportDocumentAuthenticationRequestRequestDto { Id = 5, TimeStamp = [0, 0, 0, 0, 0, 0, 0, 1] });

        Assert.That(result.IsValid, Is.True);
    }

    // A create carries no row version: the database assigns it.
    [Test]
    public void CreatingWithoutATimeStampPasses()
    {
        var result = _validator.Validate(new SaveExportDocumentAuthenticationRequestRequestDto { Id = 0, TimeStamp = null });

        Assert.That(result.IsValid, Is.True);
    }
}

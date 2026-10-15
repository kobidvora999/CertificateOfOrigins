using CertificateOfOrigins.BL.Validations;
using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.Test;

// Parity finding H-6: legacy's save ended with `entity.StatusID.Value`, so a request without a status always failed, but
// only after it had been saved and its status events raised. The status is now a precondition: a 400 naming the field,
// before anything is written, raised or sent.
[TestFixture]
public class ExportRequestStatusRequiredTests
{
    private readonly SaveExportDocumentAuthenticationRequestRequestValidator _validator = new();

    [TestCase(0)]
    [TestCase(5)]
    public void ASaveWithoutAStatusIsRejectedNamingTheField(int id)
    {
        var result = _validator.Validate(new SaveExportDocumentAuthenticationRequestRequestDto { Id = id, StatusId = null, TimeStamp = [0, 0, 0, 0, 0, 0, 0, 1] });

        Assert.Multiple(() =>
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Select(e => e.PropertyName), Is.EqualTo(new[] { nameof(SaveExportDocumentAuthenticationRequestRequestDto.StatusId) }));
            Assert.That(result.Errors[0].ErrorMessage, Does.Contain("StatusId is required"));
        });
    }
}

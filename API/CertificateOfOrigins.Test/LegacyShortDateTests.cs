using CertificateOfOrigins.BL;
using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.Test;

// The legacy WCF host was he-IL on Windows (NLS): ToShortDateString() printed 25/09/2025. In the container the current culture
// is not he-IL, and even he-IL under ICU prints 25.9.2025. A printed or displayed short date must not depend on either.
[TestFixture]
public class LegacyShortDateTests
{
    [TestCase("en-US")]
    [TestCase("he-IL")]
    [TestCase("")]
    public void AStoredDateDisplaysDayFirstWithSlashesWhateverTheCurrentCulture(string currentCulture)
    {
        var previous = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(currentCulture);

            Assert.Multiple(() =>
            {
                Assert.That(CertificateOfOriginsBl.FormatStoredDate("2025-09-25T00:00:00.0000000"), Is.EqualTo("25/09/2025"), "ISO value stored by the message path");
                Assert.That(CertificateOfOriginsBl.FormatStoredDate("05/09/2025"), Is.EqualTo("05/09/2025"), "legacy-stored dd/MM/yyyy value");
            });
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previous;
        }
    }
}

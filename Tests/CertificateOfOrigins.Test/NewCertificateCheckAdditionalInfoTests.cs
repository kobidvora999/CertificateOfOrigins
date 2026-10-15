using System.Reflection;
using CertificateOfOrigins.BL;

namespace CertificateOfOrigins.Test;

// B-L9 (CR 194221, legacy RaiseTaskNewCertificateOfOriginCheck as updated 2026-10-11): the AdditionalInfo of the
// new-certificate-check task is the request-reason name, followed by ". " and UIMessage 6541 when the export declaration
// carries other live certificates. One number, two numbers, or ". לדוגמה:" + the first and the last.
[TestFixture]
public class NewCertificateCheckAdditionalInfoTests
{
    private const string Reason = "הוספת תעודה חדשה";
    private const string Message = "שים לב: להצהרת היצוא קיימות תעודות מקור נוספות ";

    [Test]
    public void NoOtherCertificatesGivesTheReasonAlone()
    {
        var result = Format(Reason, []);

        Assert.That(result, Is.EqualTo(Reason));
    }

    [Test]
    public void OneOtherCertificateIsNamed()
    {
        var result = Format(Reason, ["IL0000000002"]);

        Assert.That(result, Is.EqualTo(Reason + ". " + Message + "IL0000000002"));
    }

    [Test]
    public void TwoOtherCertificatesAreBothNamed()
    {
        var result = Format(Reason, ["IL0000000002", "IL0000000003"]);

        Assert.That(result, Is.EqualTo(Reason + ". " + Message + "IL0000000002, IL0000000003"));
    }

    [Test]
    public void MoreThanTwoGiveTheFirstAndTheLastAsAnExample()
    {
        var result = Format(Reason, ["IL0000000002", "IL0000000003", "IL0000000004"]);

        Assert.That(result, Is.EqualTo(Reason + ". " + Message + ". לדוגמה:IL0000000002, IL0000000004"));
    }

    // Legacy concatenated a null reason name as "", so the message still follows ". ".
    [Test]
    public void AnUnnamedReasonStillCarriesTheMessage()
    {
        var result = Format(string.Empty, ["IL0000000002"]);

        Assert.That(result, Is.EqualTo(". " + Message + "IL0000000002"));
    }

    private static string Format(string reasonName, List<string> certificateNumbers)
    {
        var result = (string)typeof(CertificateOfOriginsBl)
            .GetMethod("FormatNewCertificateCheckAdditionalInfo", BindingFlags.NonPublic | BindingFlags.Static)!
            .Invoke(null, [reasonName, certificateNumbers])!;
        return result;
    }
}

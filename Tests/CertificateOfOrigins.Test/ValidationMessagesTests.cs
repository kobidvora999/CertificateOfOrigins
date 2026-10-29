using System.Reflection;
using CertificateOfOrigins.BL;
using CustomsCloud.InfrastructureCore.Interfaces.WebApi;

namespace CertificateOfOrigins.Test;

// ValidationMessages reads every text from the embedded ErrorMessagesResources.resx at runtime: a missing or misnamed entry
// would surface only as an empty message in a response. The texts are the legacy UIMessage texts (EMessages1.cs).
[TestFixture]
public class ValidationMessagesTests
{
    private static IEnumerable<ValidationMessage> AllMessages()
    {
        return typeof(ValidationMessages)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => property.PropertyType == typeof(ValidationMessage))
            .Select(property => (ValidationMessage)property.GetValue(null)!);
    }

    [Test]
    public void EveryMessageHasItsNameAndALoadedText()
    {
        var messages = AllMessages().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(messages, Has.Count.EqualTo(76));
            Assert.That(messages.Where(message => string.IsNullOrWhiteSpace(message.DisplayMessage)).Select(message => message.Name), Is.Empty,
                "every entry resolves from the resx");
        });
    }

    [TestCase(nameof(ValidationMessages.CertificateTypeNotExist), "סוג תעודת מקור לא קיים במערכת")]
    [TestCase(nameof(ValidationMessages.InvalidIdentificationNumber), "הלקוח לא קיים במערכת")]
    [TestCase(nameof(ValidationMessages.NoCustomHouseForThisCountry), "לא הוגדר בית מכס למדינה זו. יש להגדיר כתובת מתאימה")]
    [TestCase(nameof(ValidationMessages.CustomsItemMismatch), "פרט המכס {0} בחשבונית {1} בתעודה אינו קיים או חסר בחשבונית {1} בהצהרת היצוא")]
    public void TextIsTheLegacyUiMessage(string name, string expected)
    {
        var message = AllMessages().Single(candidate => candidate.Name == name);

        Assert.That(message.DisplayMessage, Is.EqualTo(expected));
    }

    [Test]
    public void WarningsAreTheUiMessagesOfTypeTwo()
    {
        var warnings = AllMessages().Where(message => message.Level == ValidationMessageLevel.Warning).Select(message => message.Name);

        Assert.That(warnings, Is.EquivalentTo(new[]
        {
            nameof(ValidationMessages.DestinationGroupDiscrepancy),
            nameof(ValidationMessages.ImportReplacementNoTradeAgreement),
            nameof(ValidationMessages.CustomsItemInDeclarationNotInCertificate),
        }));
    }
}

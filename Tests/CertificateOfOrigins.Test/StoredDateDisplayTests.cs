using CertificateOfOrigins.BL;

namespace CertificateOfOrigins.Test;

// A stored date detail value is displayed as a short date. Legacy parsed it under the WCF host's he-IL culture (day
// first): legacy-stored values are dd/MM/yyyy. An invariant parse of such a value fails above day 12 (the display became
// 01/01/0001) and flips day and month at or below 12.
[TestFixture]
public class StoredDateDisplayTests
{
    [TestCase("27/09/2026 00:00:00", 2026, 9, 27)]
    [TestCase("05/09/2026 00:00:00", 2026, 9, 5)]
    [TestCase("2026-09-27T00:00:00.0000000", 2026, 9, 27)]
    [TestCase("2026-09-05T00:00:00.0000000", 2026, 9, 5)]
    public void ALegacyOrIsoStoredValueIsDisplayedAsTheSameDate(string stored, int year, int month, int day)
    {
        var display = CertificateOfOriginsBl.FormatStoredDate(stored);

        Assert.That(display, Is.EqualTo(new DateTime(year, month, day).ToShortDateString()));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("not a date")]
    public void AnUnparseableValueIsDisplayedAsTheDefaultDate(string? stored)
    {
        var display = CertificateOfOriginsBl.FormatStoredDate(stored);

        Assert.That(display, Is.EqualTo(default(DateTime).ToShortDateString()));
    }
}

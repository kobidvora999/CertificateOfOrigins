using System.Globalization;

namespace CertificateOfOrigins.BL;

// The culture of the legacy WCF host: he-IL on Windows (NLS), where ToShortDateString() gave "dd/MM/yyyy" and DateTime.TryParse
// read day first. The container has no such culture by default: its current culture is invariant or en-US (month first), and
// even he-IL is not the same under ICU, which .NET uses on every OS since .NET 5 - its short date is "d.M.yyyy" ("25.9.2025").
// So a date the legacy printed as 25/09/2025 came out as 09/25/2025 or 25.9.2025. Every parse of a legacy-stored or
// client-typed date and every short-date display goes through here.
internal static class LegacyHostCulture
{
    internal static readonly CultureInfo Culture = Create();

    // Legacy DateTime.ToShortDateString() on the host: "dd/MM/yyyy".
    internal static string ToShortDate(DateTime date)
    {
        var result = date.ToString("d", Culture);
        return result;
    }

    private static CultureInfo Create()
    {
        CultureInfo baseCulture;
        try
        {
            baseCulture = CultureInfo.GetCultureInfo("he-IL");
        }
        catch (CultureNotFoundException)
        {
            // A container without ICU / with invariant globalization has no he-IL.
            baseCulture = CultureInfo.InvariantCulture;
        }

        var culture = (CultureInfo)baseCulture.Clone();
        culture.DateTimeFormat.DateSeparator = "/";
        culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
        var result = CultureInfo.ReadOnly(culture);
        return result;
    }
}

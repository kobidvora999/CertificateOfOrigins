using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.Test;

// Legacy validated the message-2280 request reason against CRM.CertificateOfOrigins_enum_RequestReasonCode
// (CheckRequestReasonCodeEnum → SystemTablesUtil.GetCodeById) and read the reason name from it; the migration checks
// ERequestReason instead. The enum carried 6-9, which were deleted from the table as not relevant, so those reasons were
// accepted here and rejected by legacy (found 2026-10-11). This pins the enum to the seed script, both ways.
[TestFixture]
public class RequestReasonEnumMatchesSeedTests
{
    private const string SeedScript = "API_20260715 - seed data.sql";
    private const string ReasonTableInsert = "INSERT INTO CRM.CertificateOfOrigins_enum_RequestReasonCode";

    // A seed row: (ID, N'Name', State, ...
    private static readonly Regex Row = new(@"^\((\d+),\s*N'((?:[^']|'')*)',\s*(\d+),", RegexOptions.Compiled);

    [Test]
    public void EveryReasonRowHasAnEnumMemberWithItsName()
    {
        var rows = ReadReasonRows();
        Assert.That(rows, Is.Not.Empty, "no request-reason rows were found in the seed script");

        Assert.Multiple(() =>
        {
            foreach (var (id, name) in rows)
            {
                Assert.That(Enum.IsDefined(typeof(ERequestReason), id), Is.True, $"reason {id} ({name}) has no ERequestReason member");
                if (Enum.IsDefined(typeof(ERequestReason), id))
                {
                    var member = typeof(ERequestReason).GetMember(((ERequestReason)id).ToString()).Single();
                    Assert.That(member.GetCustomAttribute<DisplayAttribute>()?.Name, Is.EqualTo(name), $"reason {id}: [Display(Name)] differs from the table Name");
                }
            }
        });
    }

    [Test]
    public void TheEnumHasNoReasonTheTableLacks()
    {
        var ids = ReadReasonRows().Select(row => row.Id).ToHashSet();

        var extra = Enum.GetValues<ERequestReason>().Select(reason => (int)reason).Where(id => !ids.Contains(id));

        Assert.That(extra, Is.Empty, "a reason that is not in the table must be rejected as RequestReasonNotExist, as legacy");
    }

    private static List<(int Id, string Name)> ReadReasonRows()
    {
        var lines = File.ReadAllLines(FindSeedScript());
        var rows = new List<(int Id, string Name)>();
        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].StartsWith(ReasonTableInsert, StringComparison.Ordinal))
            {
                continue;
            }

            // The values row follows the INSERT, after any blank lines.
            var next = lines.Skip(i + 1).FirstOrDefault(line => line.Trim().Length > 0) ?? string.Empty;
            var match = Row.Match(next.Trim());
            if (match.Success)
            {
                rows.Add((int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), match.Groups[2].Value.Replace("''", "'")));
            }
        }

        return rows;
    }

    // The Scripts folder lives under the WebApi project: walk up from the test assembly to the repo root.
    private static string FindSeedScript()
    {
        var relative = Path.Combine("API", "CertificateOfOrigins.WebApi", "Scripts", SeedScript);
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, relative)))
        {
            dir = dir.Parent;
        }

        Assert.That(dir, Is.Not.Null, $"could not locate {relative} walking up from {AppContext.BaseDirectory}");
        return Path.Combine(dir!.FullName, relative);
    }
}

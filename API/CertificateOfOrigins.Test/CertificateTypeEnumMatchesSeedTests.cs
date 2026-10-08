using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.Test;

// Parity finding X-2: the certificate-type name comes from ECertificateOfOriginType's [Display(Name)], a recorded copy of the
// Name column of CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode (legacy read the table). A copy drifts: types 10
// and 11 were added to the table after the enum was generated and printed as "10" / "11". This pins the enum to the seed
// script, so a type added to the table without its enum member fails here.
[TestFixture]
public class CertificateTypeEnumMatchesSeedTests
{
    private const string SeedScript = "API_20260715 - seed data.sql";
    private const string TypeTableInsert = "INSERT INTO CRM.CertificateOfOrigins_enum_CertificateOfOriginTypeCode";

    // A seed row: (ID, N'Name', State, ...
    private static readonly Regex Row = new(@"^\((\d+),\s*N'((?:[^']|'')*)',\s*(\d+),", RegexOptions.Compiled);

    [Test]
    public void EveryTypeRowHasAnEnumMemberWithItsName()
    {
        var rows = ReadTypeRows();
        Assert.That(rows, Is.Not.Empty, "no certificate-type rows were found in the seed script");

        Assert.Multiple(() =>
        {
            foreach (var (id, name) in rows)
            {
                Assert.That(Enum.IsDefined(typeof(ECertificateOfOriginType), id), Is.True, $"type {id} ({name}) has no ECertificateOfOriginType member");
                if (Enum.IsDefined(typeof(ECertificateOfOriginType), id))
                {
                    var member = typeof(ECertificateOfOriginType).GetMember(((ECertificateOfOriginType)id).ToString()).Single();
                    Assert.That(member.GetCustomAttribute<DisplayAttribute>()?.Name, Is.EqualTo(name), $"type {id}: [Display(Name)] differs from the table Name");
                }
            }
        });
    }

    [Test]
    public void TheEnumHasNoTypeTheTableLacks()
    {
        var ids = ReadTypeRows().Select(row => row.Id).ToHashSet();

        var extra = Enum.GetValues<ECertificateOfOriginType>().Select(type => (int)type).Where(id => !ids.Contains(id));

        Assert.That(extra, Is.Empty);
    }

    private static List<(int Id, string Name)> ReadTypeRows()
    {
        var lines = File.ReadAllLines(FindSeedScript());
        var rows = new List<(int Id, string Name)>();
        for (var i = 0; i < lines.Length; i++)
        {
            if (!lines[i].StartsWith(TypeTableInsert, StringComparison.Ordinal))
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

using System.Text.RegularExpressions;

namespace CertificateOfOrigins.Test;

// Guard against the G F3 class of bug: the DAL's set-based writes (ExecuteUpdateAsync) bypass BaseBL.SetEntityFields, so they
// must stamp the update user themselves. Legacy saved through the infrastructure repository, which stamped it; a migrated
// write that forgets it leaves the previous editor's id on the row (found in the 2026-09-27 parity review, G F3).
//
// The check reads the DAL source (the write is a LINQ expression - there is no runtime hook on the in-memory provider).
// A write that legitimately does not stamp the user is listed in AllowedWithoutUpdateUserId with the reason.
[TestFixture]
public partial class DalAuditStampGuardTests
{
    // method name -> why its write does not stamp UpdateUserId.
    private static readonly Dictionary<string, string> AllowedWithoutUpdateUserId = new()
    {
        ["LinkRequestsToAuthenticationFile"] = "legacy usp_CertificateOfOrigins_UpdateImportAuthenticationRequest only set AuthenticationFileId, no audit columns",
    };

    [Test]
    public void EverySetBasedDalWriteStampsTheUpdateUser()
    {
        var source = File.ReadAllText(FindDalSource());
        var offenders = new List<string>();

        foreach (Match write in ExecuteUpdateRegex().Matches(source))
        {
            // The statement runs from the call to its terminating semicolon.
            var end = source.IndexOf(';', write.Index);
            var statement = source[write.Index..end];
            var method = EnclosingMethodName(source, write.Index);

            if (!statement.Contains("UpdateUserId", StringComparison.Ordinal) && !AllowedWithoutUpdateUserId.ContainsKey(method))
            {
                offenders.Add(method);
            }
        }

        Assert.That(offenders, Is.Empty,
            "set-based writes must stamp UpdateUserId (take a userId parameter): " + string.Join(", ", offenders.Distinct()));
    }

    [Test]
    public void TheAllowListHoldsNoStaleEntries()
    {
        var source = File.ReadAllText(FindDalSource());

        Assert.Multiple(() =>
        {
            foreach (var method in AllowedWithoutUpdateUserId.Keys)
            {
                Assert.That(source, Does.Contain("Task<bool> " + method + "("), $"{method} is allow-listed but no longer exists");
            }
        });
    }

    private static string EnclosingMethodName(string source, int index)
    {
        var declarations = MethodDeclarationRegex().Matches(source[..index]);
        return declarations.Count == 0 ? "<unknown>" : declarations[^1].Groups["name"].Value;
    }

    private static string FindDalSource()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "CertificateOfOrigins.DAL", "CertificateOfOriginsDal.cs");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException("CertificateOfOriginsDal.cs not found above " + AppContext.BaseDirectory);
    }

    [GeneratedRegex(@"\.ExecuteUpdateAsync\(")]
    private static partial Regex ExecuteUpdateRegex();

    [GeneratedRegex(@"public\s+async\s+Task(<[^>]+>)?\s+(?<name>\w+)\s*\(")]
    private static partial Regex MethodDeclarationRegex();
}

using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateOfOrigins.Test;

// Parity finding E-F12: legacy returned the decision lookup with every column (GetQuery<CertificateOfOriginsDecision>().ToList());
// the migration dropped EndDate and IsAutomatic.
[TestFixture]
public class DecisionLookupColumnsTests
{
    [Test]
    public async Task TheDecisionLookupCarriesEndDateAndIsAutomatic()
    {
        var options = new DbContextOptionsBuilder<CertificateOfOriginsDbContext>()
            .UseInMemoryDatabase($"coo-decisions-{Guid.NewGuid()}")
            .Options;
        var endDate = new DateTime(2026, 12, 31);
        await using (var seed = new CertificateOfOriginsDbContext(options))
        {
            seed.CertificateOfOriginsDecisions.Add(new CertificateOfOriginsDecision
            {
                Id = 7,
                Name = "אושר",
                EnglishName = "Approved",
                Enumeration = "Approved",
                EndDate = endDate,
                IsAutomatic = true,
            });
            await seed.SaveChangesAsync();
        }

        var services = new ServiceCollection();
        services.AddSingleton(new CertificateOfOriginsDbContext(options));
        services.AddSingleton(new CertificateOfOriginsDbReadOnlyContext(options));
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<CustomsCloud.InfrastructureCore.DAL.IBaseDal>>(Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomsCloud.InfrastructureCore.DAL.IBaseDal>.Instance);
        var dal = new CertificateOfOriginsDal(services.BuildServiceProvider());

        var decision = (await dal.GetAllDecisions()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(decision.EndDate, Is.EqualTo(endDate));
            Assert.That(decision.IsAutomatic, Is.True);
        });
    }
}

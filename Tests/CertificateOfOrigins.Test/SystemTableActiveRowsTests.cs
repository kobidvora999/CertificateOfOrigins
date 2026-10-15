using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateOfOrigins.Test;

// Parity findings E-F13 and C F-23 (second half): legacy read these C-tables through SystemTablesUtil, whose query builder
// returns active rows only (State == 1) unless IgnoreState is set. "Not deleted" (State != 99) is wider: the supplier-delivery
// config's State defaults to 0, an inactive row.
[TestFixture]
public class SystemTableActiveRowsTests
{
    [TestCase(1, true)]
    [TestCase(0, false)]
    [TestCase(99, false)]
    public async Task ASupplierDeliveryCountryCountsOnlyWhenItsConfigRowIsActive(int state, bool expected)
    {
        var dal = await CreateDal(db => db.CertificateOfOriginsSupplierDeliveryCountryConfigs.Add(
            new CertificateOfOriginsSupplierDeliveryCountryConfig { Id = 1, ConutryId = 380, State = state }));

        var result = await dal.IsSupplierDeliveryCountry(380);

        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase(1, true)]
    [TestCase(0, false)]
    public async Task AnOriginCriterionResolvesOnlyWhenItIsActive(int state, bool expected)
    {
        var dal = await CreateDal(db => db.OriginCriterions.Add(
            new OriginCriterion { Id = 3, OriginCriterionCode = "P", CertificateOfOriginTypeCodeId = 2, EnglishName = "P", State = state }));

        var result = await dal.GetOriginCriterion("P", 2);

        Assert.That(result is not null, Is.EqualTo(expected));
    }

    private static async Task<CertificateOfOriginsDal> CreateDal(Action<CertificateOfOriginsDbContext> seedRows)
    {
        var options = new DbContextOptionsBuilder<CertificateOfOriginsDbContext>()
            .UseInMemoryDatabase($"coo-active-{Guid.NewGuid()}")
            .Options;
        await using (var seed = new CertificateOfOriginsDbContext(options))
        {
            seedRows(seed);
            await seed.SaveChangesAsync();
        }

        var services = new ServiceCollection();
        services.AddSingleton(new CertificateOfOriginsDbContext(options));
        services.AddSingleton(new CertificateOfOriginsDbReadOnlyContext(options));
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<CustomsCloud.InfrastructureCore.DAL.IBaseDal>>(Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomsCloud.InfrastructureCore.DAL.IBaseDal>.Instance);
        var dal = new CertificateOfOriginsDal(services.BuildServiceProvider());
        return dal;
    }
}

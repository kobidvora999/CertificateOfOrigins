using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateOfOrigins.Test;

// Parity finding C F-23: message 2280 finds "the certificate with this number" the way legacy's GetCertificateOfOriginByExternalId
// did - the first row of usp_CertificateOfOrigins_GetCertificateOfOriginsByFilter, which reads active rows only (State = 1)
// and orders by CreateDate descending. Legacy matched the number with LIKE '%number%'; pure REST puts the number in the URL,
// so it is matched exactly (developer decision, 29.10.2026).
[TestFixture]
public class LatestCertificateByNumberTests
{
    [Test]
    public async Task ADeletedVersionIsNotTheCertificateOfTheNumber()
    {
        var dal = await CreateDal(
            NewCertificate(1, state: 1, createDate: new DateTime(2026, 1, 1)),
            NewCertificate(2, state: 99, createDate: new DateTime(2026, 2, 1)));

        var result = await dal.GetLatestCertificateByNumberForFeedback("IL0000000001");

        Assert.That(result?.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task TheNewestByCreateDateWins()
    {
        var dal = await CreateDal(
            NewCertificate(5, state: 1, createDate: new DateTime(2026, 3, 1)),
            NewCertificate(6, state: 1, createDate: new DateTime(2026, 1, 1)));

        var result = await dal.GetLatestCertificateByNumberForFeedback("IL0000000001");

        Assert.That(result?.Id, Is.EqualTo(5));
    }

    // The legacy SP matched with LIKE '%' + number + '%', so a partial number reached (and could cancel) a longer one.
    [Test]
    public async Task ANumberContainedInALongerOneIsNotTheCertificate()
    {
        var dal = await CreateDal(NewCertificate(9, state: 1, createDate: new DateTime(2026, 1, 1)));

        var result = await dal.GetLatestCertificateByNumberForFeedback("IL000000000");

        Assert.That(result, Is.Null);
    }

    private static CertificateOfOrigin NewCertificate(int id, int state, DateTime createDate)
    {
        return new CertificateOfOrigin
        {
            Id = id,
            CertificateNumber = "IL0000000001",
            Title = "IL0000000001",
            State = state,
            CreateDate = createDate,
            TimeStamp = [0, 0, 0, 0, 0, 0, 0, 1],
        };
    }

    private static async Task<CertificateOfOriginsDal> CreateDal(params CertificateOfOrigin[] certificates)
    {
        var options = new DbContextOptionsBuilder<CertificateOfOriginsDbContext>()
            .UseInMemoryDatabase($"coo-latest-{Guid.NewGuid()}")
            .Options;
        await using (var seed = new CertificateOfOriginsDbContext(options))
        {
            seed.CertificateOfOrigins.AddRange(certificates);
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

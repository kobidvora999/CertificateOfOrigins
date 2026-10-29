using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CertificateOfOrigins.Test;

// Parity finding H-3: legacy saved the export request and its three child collections with one Repository.Save +
// CommitAllChanges. The migration committed the parent, then deleted the dropped children straight in the database
// (ExecuteDeleteAsync), then committed the adds and updates - so a failing last step left the parent saved and the dropped
// children already gone. The merge now only STAGES every change (the deletes included); the BL's single SaveChangesAsync
// commits them together.
[TestFixture]
public class ExportRequestChildrenMergeTests
{
    [Test]
    public async Task TheMergeStagesTheDeletesInsteadOfWritingThemToTheDatabase()
    {
        var (dal, dbContext, options) = CreateDal();
        await Seed(dbContext, requestId: 1, customsItemIds: [10, 11]);

        // Item 10 is sent back (kept), item 11 is dropped by the client, and a new item (Id 0) is added.
        var parent = await dbContext.Set<ExportDocumentAuthenticationRequest>().AsNoTracking().SingleAsync(r => r.Id == 1);
        await dal.MergeExportDocumentAuthenticationRequestChildren(
            parent,
            [new CustomsItemToExportDocumentAuthenticationRequest { Id = 10, CustomsItemId = 999 }, new CustomsItemToExportDocumentAuthenticationRequest { Id = 0, CustomsItemId = 555 }],
            [],
            []);

        // Before the commit nothing reached the database: a second context still sees the stored rows untouched.
        await using (var before = new CertificateOfOriginsDbContext(options))
        {
            var stored = await before.Set<CustomsItemToExportDocumentAuthenticationRequest>().AsNoTracking().OrderBy(c => c.Id).ToListAsync();
            Assert.That(stored.Select(c => (c.Id, c.CustomsItemId)), Is.EqualTo(new[] { (10, 1), (11, 1) }),
                "the dropped child is not deleted before the commit");
        }

        await dbContext.SaveChangesAsync();

        await using var after = new CertificateOfOriginsDbContext(options);
        var committed = await after.Set<CustomsItemToExportDocumentAuthenticationRequest>().AsNoTracking().OrderBy(c => c.CustomsItemId).ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(committed.Select(c => c.CustomsItemId), Is.EqualTo(new[] { 555, 999 }), "11 deleted, 10 updated, one inserted");
            Assert.That(committed.All(c => c.ExportDocumentAuthenticationRequestId == 1), Is.True);
        });
    }

    [Test]
    public async Task ANewParentGetsItsChildrenBoundByNavigationInTheSameCommit()
    {
        var (dal, dbContext, options) = CreateDal();

        // The parent is tracked as Added and has no id yet: the children must be bound through the navigation.
        var parent = NewParent();
        dbContext.Add(parent);
        await dal.MergeExportDocumentAuthenticationRequestChildren(
            parent,
            [new CustomsItemToExportDocumentAuthenticationRequest { CustomsItemId = 7 }],
            [],
            []);

        await dbContext.SaveChangesAsync();

        await using var after = new CertificateOfOriginsDbContext(options);
        var item = await after.Set<CustomsItemToExportDocumentAuthenticationRequest>().AsNoTracking().SingleAsync();
        Assert.That(item.ExportDocumentAuthenticationRequestId, Is.EqualTo(parent.Id).And.Not.Zero);
    }

    private static (CertificateOfOriginsDal Dal, CertificateOfOriginsDbContext DbContext, DbContextOptions<CertificateOfOriginsDbContext> Options) CreateDal()
    {
        var options = new DbContextOptionsBuilder<CertificateOfOriginsDbContext>()
            .UseInMemoryDatabase($"coo-merge-{Guid.NewGuid()}")
            .Options;
        var dbContext = new CertificateOfOriginsDbContext(options);

        var services = new ServiceCollection();
        services.AddSingleton(dbContext);
        services.AddSingleton<Microsoft.Extensions.Logging.ILogger<CustomsCloud.InfrastructureCore.DAL.IBaseDal>>(Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomsCloud.InfrastructureCore.DAL.IBaseDal>.Instance);
        var dal = new CertificateOfOriginsDal(services.BuildServiceProvider());
        return (dal, dbContext, options);
    }

    private static ExportDocumentAuthenticationRequest NewParent()
    {
        return new ExportDocumentAuthenticationRequest
        {
            TypeId = 1,
            Title = "EXP-1",
            OrganizationUnitId = 1,
            CustomerId = 1,
            AuthenticationDocumentTypeId = 1,
            AuthenticationRequestNotes = "notes",
            TimeStamp = [0, 0, 0, 0, 0, 0, 0, 1],
        };
    }

    private static async Task Seed(CertificateOfOriginsDbContext dbContext, int requestId, int[] customsItemIds)
    {
        var parent = NewParent();
        parent.Id = requestId;
        dbContext.Add(parent);
        foreach (var id in customsItemIds)
        {
            dbContext.Add(new CustomsItemToExportDocumentAuthenticationRequest { Id = id, ExportDocumentAuthenticationRequestId = requestId, CustomsItemId = 1 });
        }

        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();
    }
}

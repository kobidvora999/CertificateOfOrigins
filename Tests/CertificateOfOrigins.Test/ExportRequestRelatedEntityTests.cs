using System.Reflection;
using CertificateOfOrigins.BL;
using CertificateOfOrigins.BL.Proxies;
using CertificateOfOrigins.DAL;
using CertificateOfOrigins.Model.CertificateOfOriginsDb;
using CertificateOfOrigins.Model.ModelDTOs;
using CustomsCloud.InfrastructureCore.Interfaces.Http;
using CustomsCloud.InfrastructureCore.Lookup;
using CustomsCloud.InfrastructureCore.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CertificateOfOrigins.Test;

// Parity finding H-4: legacy built the entity link of the document attach and of the status message with
// `new VirtualEntity(entity)` (SendMessageDTO wraps it the same way), which copies the entity's Title and CustomerID. The
// migration sent only the id and the type, so the Documents and MessageManagement services received no title and CustomerId 0.
[TestFixture]
public class ExportRequestRelatedEntityTests
{
    private const string RequestTitle = "EXP-1001";
    private const int RequestCustomerId = 777;

    [Test]
    public async Task TheDocumentAttachCarriesTheRequestTitleAndCustomer()
    {
        var captured = await Save();

        Assert.Multiple(() =>
        {
            Assert.That(captured.AttachedEntity, Is.Not.Null);
            Assert.That(captured.AttachedEntity!.Title, Is.EqualTo(RequestTitle));
            Assert.That(captured.AttachedEntity.CustomerId, Is.EqualTo(RequestCustomerId));
            Assert.That(captured.AttachedEntity.EntityType, Is.EqualTo((int)EEntityType.ExportDocumentAuthenticationRequest));
            Assert.That(captured.AttachedEntity.Id, Is.Not.Zero, "the id is the saved request's");
        });
    }

    [Test]
    public async Task TheStatusMessageCarriesTheRequestTitleAndCustomer()
    {
        var captured = await Save();

        Assert.Multiple(() =>
        {
            Assert.That(captured.MessageRelatedEntity, Is.Not.Null);
            Assert.That(captured.MessageRelatedEntity!.Title, Is.EqualTo(RequestTitle));
            Assert.That(captured.MessageRelatedEntity.CustomerId, Is.EqualTo(RequestCustomerId));
            Assert.That(captured.MessageRelatedEntity.Id, Is.EqualTo(captured.AttachedEntity!.Id));
        });
    }

    // Parity finding H-11: legacy read the status name from the enum table (SystemTables), so an edited row shows up in the
    // message; the migration took it from a hard-coded attribute.
    [Test]
    public async Task TheStatusMessageCarriesTheStatusNameFromTheStatusTable()
    {
        var captured = await Save();

        Assert.That(captured.MessageParameters, Is.EqualTo(new[] { captured.AttachedEntity!.Id.ToString(), "שם מהטבלה" }));
    }

    private static async Task<Captured> Save()
    {
        var captured = new Captured();
        var dbOptions = new DbContextOptionsBuilder<CertificateOfOriginsDbContext>()
            .UseInMemoryDatabase($"coo-export-{Guid.NewGuid()}")
            .Options;
        using var dbContext = new CertificateOfOriginsDbContext(dbOptions);

        var dataLayer = Fake<ICertificateOfOriginsDal>((method, args) => method.Name switch
        {
            "Add" => AddToContext(dbContext, args![0]!),
            "get_DbContext" => dbContext,
            "SaveChangesAsync" => dbContext.SaveChangesAsync(),
            "MergeExportDocumentAuthenticationRequestChildren" => Task.CompletedTask,
            "GetExportAuthenticationRequestStatusName" => Task.FromResult<string?>("שם מהטבלה"),
            "GetExportDocumentAuthenticationRequestById" => Task.FromResult<ExportDocumentAuthenticationRequest?>(
                dbContext.Set<ExportDocumentAuthenticationRequest>().AsNoTracking().FirstOrDefault(r => r.Id == (int)args![0]!)),
            _ => null,
        });

        var documentsProxy = Fake<IDocumentsProxy>((method, args) =>
        {
            if (method.Name == "AttachDocumentsToEntity")
            {
                captured.AttachedEntity = ((DocumentsToEntityDto)args![0]!).Entity;
            }

            return null;
        });

        var messageProxy = Fake<IMessageManagementProxy>((method, args) =>
        {
            if (method.Name == "SendMessage")
            {
                captured.MessageRelatedEntity = ((SendMessageDto)args![0]!).RelatedEntity;
                captured.MessageParameters = ((SendMessageDto)args[0]!).MessageParameters;
            }

            return null;
        });

        var builderType = typeof(IEventUtil).GetMethod(nameof(IEventUtil.CreatBuilder))!.ReturnType;
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<ExportDocumentAuthenticationRequestBl>>(NullLogger<ExportDocumentAuthenticationRequestBl>.Instance);
        services.AddSingleton(dataLayer);
        services.AddSingleton(Fake<IRequestMetadata>((method, _) => method.Name == "get_UserId" ? (int?)7 : null));
        services.AddSingleton(documentsProxy);
        services.AddSingleton(messageProxy);
        services.AddSingleton(Fake<IEventUtil>((method, _) => method.Name == nameof(IEventUtil.CreatBuilder) ? FakeChainingBuilder(builderType) : null));
        services.AddSingleton(Fake<ICustomerProxy>());

        var bl = new ExportDocumentAuthenticationRequestBl(services.BuildServiceProvider(), Fake<ILookupUtil>());

        // A new request moving to ReadyForProfessionalTreatment: raises the status events AND sends the status message;
        // one additional document is attached after the save.
        await bl.SaveExportDocumentAuthenticationRequest(new SaveExportDocumentAuthenticationRequestRequestDto
        {
            Id = 0,
            TypeId = 1,
            Title = RequestTitle,
            OrganizationUnitId = 1,
            CustomerId = RequestCustomerId,
            AuthenticationDocumentTypeId = 1,
            StatusId = (int)EExportAuthenticationRequestStatus.ReadyForProfessionalTreatment,
            OriginalStatusId = 0,
            AuthenticationRequestNotes = "notes",
            TimeStamp = [0, 0, 0, 0, 0, 0, 0, 1],
            ListOfAdditionalDocumentsIds = [5],
        });

        return captured;
    }

    private static object? AddToContext(CertificateOfOriginsDbContext dbContext, object entity)
    {
        dbContext.Add(entity);
        return null;
    }

    private sealed class Captured
    {
        public VirtualEntityDto? AttachedEntity;
        public VirtualEntityDto? MessageRelatedEntity;
        public IEnumerable<string>? MessageParameters;
    }

    // --- Minimal dependency-free interface faking over System.Reflection.DispatchProxy (no mocking package). ---

    private static T Fake<T>(Func<MethodInfo, object?[]?, object?>? handler = null) where T : class
    {
        var proxy = DispatchProxy.Create<T, InterfaceFake>();
        ((InterfaceFake)(object)proxy).Handler = (method, args) => handler?.Invoke(method, args) ?? DefaultReturn(method);
        return proxy;
    }

    // The event builder's type is known only at runtime; every fluent call returns the same proxy.
    private static object FakeChainingBuilder(Type builderType)
    {
        var proxy = typeof(DispatchProxy).GetMethods()
            .First(m => m.Name == nameof(DispatchProxy.Create) && m.GetGenericArguments().Length == 2 && m.GetParameters().Length == 0)
            .MakeGenericMethod(builderType, typeof(InterfaceFake))
            .Invoke(null, null)!;
        ((InterfaceFake)proxy).Handler = (method, _) => method.ReturnType == builderType ? proxy : DefaultReturn(method);
        return proxy;
    }

    private static object? DefaultReturn(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType == typeof(void))
        {
            return null;
        }

        if (returnType == typeof(Task))
        {
            return Task.CompletedTask;
        }

        if (returnType == typeof(ValueTask))
        {
            return new ValueTask();
        }

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var inner = returnType.GetGenericArguments()[0];
            return typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(inner)
                .Invoke(null, [inner.IsValueType ? Activator.CreateInstance(inner) : null])!;
        }

        return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
    }

    public class InterfaceFake : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = (_, _) => null;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            return Handler(targetMethod!, args);
        }
    }
}

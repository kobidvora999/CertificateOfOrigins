using System.Reflection;
using CertificateOfOrigins.BL;
using CustomsCloud.InfrastructureCore.BL.Exceptions;

namespace CertificateOfOrigins.Test;

// CR-N1: an unregistered template id is a bad request (400). It was thrown with the one-argument RestValidationException
// constructor, which itself throws InvalidOperationException ("errors parameter is null or empty") — a 500.
[TestFixture]
public class TemplateUnknownIdTests
{
    [Test]
    public void AnUnregisteredTemplateIdIsAValidationError()
    {
        var getTemplateMeta = typeof(CertificateOfOriginsBl).GetMethod("GetTemplateMeta", BindingFlags.NonPublic | BindingFlags.Static)!;

        var thrown = Assert.Throws<TargetInvocationException>(() => getTemplateMeta.Invoke(null, [999]))!.InnerException;

        Assert.That(thrown, Is.InstanceOf<RestValidationException>());
    }
}

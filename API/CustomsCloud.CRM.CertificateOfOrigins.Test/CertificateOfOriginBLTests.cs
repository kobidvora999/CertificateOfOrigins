using CustomsCloud.CRM.CertificateOfOrigins.BL;
using CustomsCloud.CRM.CertificateOfOrigins.DAL;
using CustomsCloud.InfrastructureCore.BL;
using CustomsCloud.InfrastructureCore;
using CustomsCloud.InfrastructureCore.UnitTest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CustomsCloud.CRM.CertificateOfOrigins.Test
{
    public class CertificateOfOriginBLTests : BaseTest
    {
        public override void RegisterServices(IConfiguration configuration, IServiceCollection services)
        {
            // === Add Services ===
            services.AddDbContext<CertificateOfOriginDbContext>(connectionStringName: "CertificateOfOrigins");

            /// services.AddQueueService();
            /// services.AddCacheService();
            /// services.AddSystemTablesService();

            // === Add Project Layers ===
            services.AddDataLayer<ICertificateOfOriginDal, CertificateOfOriginDal>();
            services.AddBusinessLayer<CertificateOfOriginBl>();

            // === Add Proxies ===
        }

        [Test]
        public async Task Test1()
        {
            var bl = Resolve<CertificateOfOriginBl>();
            await Task.Delay(1000);
            Assert.Pass();
        }
    }
}

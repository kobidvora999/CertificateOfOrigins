
using System.ComponentModel;
using Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.AuthenticationRequestFile;
using Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.AuthenticationRequestSearch;
using Customs.CRM.CertificateOfOrigins.Client.Api.AuthenticationRequest.ImportProcessForm;
using Customs.CRM.CertificateOfOrigins.Client.Api.CertificateOfOriginsEdit;
using Customs.CRM.CertificateOfOrigins.Client.Api.CertificateOfOriginsSearch;
using Customs.CRM.CertificateOfOrigins.Client.Api.ExportDocumentAuthenticationRequestEdit;
using Customs.CRM.CertificateOfOrigins.Client.Api.ExportDocumentAuthenticationRequestSearch;
using Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.AuthenticationRequestFile;
using Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.AuthenticationRequestSearch;
using Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.ImportProcessForm;
using Customs.CRM.CertificateOfOrigins.Client.Module.CertificateOfOriginsEdit;
using Customs.CRM.CertificateOfOrigins.Client.Module.CertificateOfOriginsSearch;
using Customs.CRM.CertificateOfOrigins.Client.Module.ExportDocumentAuthenticationRequestEdit;
using Customs.CRM.CertificateOfOrigins.Client.Module.ExportDocumentAuthenticationRequestSearch;
using Customs.CRM.CertificateOfOrigion.Client.External.Api;
using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Assets;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Services.Module.NavigationManager;
using Customs.Inf.MMI.Services.Module.Search;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;
using Microsoft.Practices.Prism.Modularity;
using System.Windows;
using Customs.CRM.CertificateOfOrigins.Client.Module.AuthenticationRequest.DealFileAuthenticationRequest;

namespace Customs.CRM.CertificateOfOrigins.Client.Module
{
    [Module(ModuleName ="CertificateOfOrigins")]//ModuleNames.
    [Description(ModuleNames.ApplicationModule)]
    [ModuleDependency(ModuleNames.Events)]
    [ModuleDependency(ModuleNames.UserManagement)]
    [ModuleDependency(ModuleNames.CommonServices)]
    [ModuleDependency(ModuleNames.DocumentsManager)]
    [ModuleDependency(ModuleNames.CustomsBook)]
    public class CertificateOfOriginsModuleManager : ModuleManagerBase
    {
        /// <summary>
        /// Notifies the module that it has be initialized.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            RegisterTypeIfMissing(typeof(ICertificateOfOriginsSearchPresenter), typeof(CertificateOfOriginsSearchPresenter), true);

            RegisterTypeIfMissing(typeof(ICertificateOfOriginsEditPresenter), typeof(CertificateOfOriginsEditPresenter), false);

            RegisterTypeIfMissing(typeof(IImportProcessFormPresenter), typeof(ImportProcessFormPresenter), false);

            RegisterTypeIfMissing(typeof(IAuthenticationRequestSearchPresenter), typeof(AuthenticationRequestSearchPresenter), false);
            RegisterTypeIfMissing(typeof(IAuthenticationRequestFilePresenter), typeof(AuthenticationRequestFilePresenter), false);
            RegisterTypeIfMissing(typeof(IExportDocumentAuthenticationRequestPresenter), typeof(ExportDocumentAuthenticationRequestPresenter), false);
            RegisterTypeIfMissing(typeof(IExportDocumentAuthenticationRequestSearchPresenter), typeof(ExportDocumentAuthenticationRequestSearchPresenter), false);
            RegisterTypeIfMissing(typeof(IImportProcessFormExternalPresenter), typeof(ImportProcessFormPresenter), false);
            RegisterTypeIfMissing(typeof(IImportAuthenticationRequestExternalPresenter), typeof(DealFileAuthenticationRequestPresenterEditPresenter), false);

            var navMap = new NavigationMappingBase
            {
                EntityType = EEntityType.ImportAuthenticationRequest,
                MappingType = NavigationMappingType.Search,
                FactoryType = typeof(IImportProcessFormPresenter),
                NavigationItemVisibility = Visibility.Collapsed,
                IsFactoryItem = false
            };

            SearchServiceManager.Instance.Register("temp", new SearchConsumerItem()
            {
                Mapping = navMap,
                SearchRegexAndMaskType = (MalamTeam.Infrastructure.GeneralServices.Environment.Enums.ERegexAndMaskType?) ERegexAndMaskType.RegexNumericUpTo10Digits
            });
            NavigationEngine.Instance.Register(navMap);

        }
    }
}
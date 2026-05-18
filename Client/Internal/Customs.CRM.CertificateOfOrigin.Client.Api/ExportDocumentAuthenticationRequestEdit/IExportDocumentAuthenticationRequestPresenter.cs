using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Services.Api.NavigationManager;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Api.ExportDocumentAuthenticationRequestEdit
{
    /// <summary>
    /// IExportDocumentAuthenticationRequestEditPresenter
    /// </summary>
    [NavigationMapping(EEntityType.ExportDocumentAuthenticationRequest, NavigationMappingType.New | NavigationMappingType.Edit)]
    public interface IExportDocumentAuthenticationRequestPresenter : IPresenter
    {

    }
}
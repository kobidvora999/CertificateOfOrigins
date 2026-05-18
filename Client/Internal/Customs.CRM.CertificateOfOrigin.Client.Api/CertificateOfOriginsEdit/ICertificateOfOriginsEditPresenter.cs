using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Common.Navigation;
using Customs.Inf.MMI.Services.Api.NavigationManager;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Api.CertificateOfOriginsEdit
{
    /// <summary>
    /// ICertificateOfOriginsEditPresenter
    /// </summary>
    [NavigationMapping(EEntityType.CertificateOfOrigin, NavigationMappingType.New | NavigationMappingType.Edit)]
    public interface ICertificateOfOriginsEditPresenter : IPresenter
    {

    }
}
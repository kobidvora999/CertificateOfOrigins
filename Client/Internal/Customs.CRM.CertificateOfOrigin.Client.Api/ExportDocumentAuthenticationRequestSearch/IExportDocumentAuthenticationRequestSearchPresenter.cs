using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Services.Api.Search;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Api.ExportDocumentAuthenticationRequestSearch
{
    [NavigationSearchMapping(EEntityType.ExportDocumentAuthenticationRequest, ERegexAndMaskType.RegexNumericUpTo10Digits)]
    public interface IExportDocumentAuthenticationRequestSearchPresenter : IPresenter, ISearchable
    {

    }
}
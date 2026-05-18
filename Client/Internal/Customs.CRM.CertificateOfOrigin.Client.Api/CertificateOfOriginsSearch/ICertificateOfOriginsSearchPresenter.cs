using Customs.Inf.MMI.Common.CAL;
using Customs.Inf.MMI.Services.Api.Search;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.Client.Api.CertificateOfOriginsSearch
{    
    [NavigationSearchMapping(EEntityType.CertificateOfOrigin, ERegexAndMaskType.RegexNumericUpTo10Digits)]
    public interface ICertificateOfOriginsSearchPresenter : ISearchable
    {

    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Customs.KnowledgeStore.CustomsBook.ExternalCommon.Common;
using Customs.KnowledgeStore.CustomsBook.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public class CustomsBookServicesAdapter : BaseServiceAdapter<ICustomsBookExternalProxy>, ICustomsBookServicesAdapter
    {
        public int? GetCustomsItemIdByFullClassificationSync(string fullClassification, ECustomsBookType bookType)
        {
            return ExternalProxy.GetCustomsItemIdByFullClassificationSync(fullClassification, bookType, DateTime.Today); //todo: find the right date
        }

        public bool IsTradeAgreementForCountry(int countryId, int tradeAgreementId, bool isCountryGroup)
        {
            return ExternalProxy.IsTradeAgreementForCountry(new IfExistTradeAgreementForCountryFilter
                {
                    TradeAgreementId = tradeAgreementId,
                    CountryOrGroupId = countryId,
                    IsGroup = isCountryGroup,
                    ReferenceDate = DateTime.Now
                });
        }

        public List<CustomsItemDTO> GetCustomsItemsByIdsSync(List<CustomsItemsIdsCacheFilter> customsItemsIdsCacheFilters)
        {
            return ExternalProxy.GetCustomsItemsByIdsSync(customsItemsIdsCacheFilters);
        }
    }
}

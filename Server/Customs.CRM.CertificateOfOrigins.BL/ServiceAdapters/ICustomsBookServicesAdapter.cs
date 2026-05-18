using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Customs.KnowledgeStore.CustomsBook.ExternalCommon.Common;

using MalamTeam.Infrastructure.GeneralServices.Environment.Enums;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public interface ICustomsBookServicesAdapter
    {
        int? GetCustomsItemIdByFullClassificationSync(string fullClassification, ECustomsBookType bookType);

        bool IsTradeAgreementForCountry(int countryId, int tradeAgreementId, bool isCountryGroup);
        List<CustomsItemDTO> GetCustomsItemsByIdsSync(List<CustomsItemsIdsCacheFilter> customsItemsIdsCacheFilters);
    }

}

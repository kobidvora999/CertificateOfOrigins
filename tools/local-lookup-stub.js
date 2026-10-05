// Local lookup-source stub for offline dev/Postman runs.
//
// The WebApi's ILookupUtil resolvers (Country/City/DocumentType/OrganizationUnit) load their data over HTTP from
// the SystemTables-family source services (GET {service}/lookup/{Type} -> JSON array of ILookup). Those services
// are NOT part of this repo and are not running on a dev box, so every lookup-enriching endpoint returns HTTP 500
// (SocketException to localhost:9000/9006/9015). The mock header (x-mock-mode) covers PROXIES, not these resolvers.
//
// This stub stands in for those source services so the full internal-workload can run locally. It serves each
// registered lookup type as a JSON array of {id,name,state,description,englishName} for ids 1..RANGE, so any id the
// test data references resolves to a name. Unknown /lookup/* paths return [] (still HTTP 200 -> no 500).
//
// Ports (from the readiness log): 9000 = Country + City, 9005 = SystemTables, 9006 = DocumentType, 9015 = OrganizationUnit,
// 9026 = Cargos, 9029 = Sites.
//
// LOCAL lookup types (BL/Lookups, registered with AddLocalLookup) are lookups the platform package does not have yet.
// They load the same way (GET lookup/{Type}) and are served here with their extra fields. TODO(internal): each one is
// removed from here once the platform has the type and its real source serves it.
// Run:  node tools/local-lookup-stub.js         (Ctrl+C to stop)
// Then start the WebApi (see the readiness-gate bypass) and run the Postman collection.

const http = require('http');

const RANGE = 500;
const PORTS = {
  9000: ['Country', 'City', 'MeasurementUnit', 'CountryGroup', 'CountryCountryGroup', 'CurrencyType'],
  9005: ['InternationalSite'],
  9006: ['DocumentType'],
  9026: ['PackingType'],
  9029: ['Site'],
  9015: ['OrganizationUnit'],
};

// Extra fields of the local lookup types, by id. Ids not listed get only the base fields.
// InternationalSite: the UN/LOCODEs the Postman collections send (portOfEntrance / exitPort / exportPort).
const EXTRA = {
  InternationalSite: {
    1: { locode: 'ILHFA', englishName: 'Haifa' },
    2: { locode: 'ILASH', englishName: 'Ashdod' },
    3: { locode: 'DEHAM', englishName: 'Hamburg' },
  },
  // Site: the customs-house external site numbers the Postman collections send, each pointing at an org unit.
  // CountryGroup: the group ids the collections send (1, 5).
  CountryGroup: {
    1: { englishName: 'Country group 1', isForTradeAgreement: true },
    5: { englishName: 'Country group 5', isForTradeAgreement: true },
  },
  // CountryCountryGroup: every country the collections send in both groups — the retired CountryGroupMockProxy answered
  // "in the group" by default. A "not in the group" scenario sends a country that is not listed here.
  CountryCountryGroup: Object.fromEntries([32, 99, 138, 376].flatMap((countryId, i) => [1, 5].map((countryGroupId, j) =>
    [i * 2 + j + 1, { name: '', countryId, countryGroupId }]))),
  // CurrencyType: what the collections use, on the ids the retired CurrencyTypeMockProxy gave them (USD = 237 by code;
  // id 1 = EUR by id).
  CurrencyType: {
    1: { currencyCode: 'EUR', englishName: 'Euro' },
    237: { currencyCode: 'USD', englishName: 'US Dollar' },
  },
  // PackingType: the package-type codes the collections send, on the ids the retired PackingTypeMockProxy gave them
  // (BOX-40 = 379 = the container packing type, CertificateOfOriginsConsts.PackingTypeContainer).
  PackingType: {
    155: { commonCode: 'BX', englishName: 'Box' },
    379: { commonCode: 'BOX-40', englishName: 'Container 40' },
  },
  Site: {
    // SITE01 -> org unit 407: the id the retired SiteMockProxy derived for SITE01, so the collections' expectations hold.
    407: { externalSiteNumberForMessages: 'SITE01', organizationUnitId: 407, typeId: 1, englishName: 'Site SITE01' },
  },
};

// Extra fields on rows of PLATFORM lookup types (merged into the 1..RANGE rows).
// Country: the alpha-2 codes the Postman collections send, on the ids the retired CountryMockProxy gave them
// (IL = 376, the CountryIsrael parameter; DE = 138).
const OVERRIDES = {
  Country: {
    376: { countryAlphaCode2: 'IL', englishName: 'Israel', isCountry: true },
    138: { countryAlphaCode2: 'DE', englishName: 'Germany', isCountry: true },
  },
  // MeasurementUnit: the measure-type code the collections send, on the id the retired MeasurementUnitMockProxy gave it.
  MeasurementUnit: {
    147: { externalIdnum: 'KG', englishName: 'Kilogram' },
  },
};

function items(type) {
  const extra = EXTRA[type];
  if (extra) {
    return Object.entries(extra).map(([id, fields]) => ({
      id: Number(id), name: fields.englishName, state: 1, description: fields.englishName, ...fields,
    }));
  }

  const out = [];
  for (let id = 1; id <= RANGE; id++) {
    out.push({
      id,
      name: `${type} ${id}`,
      state: 1,
      description: `${type} ${id}`,
      englishName: `${type} ${id}`,
    });
  }
  const overrides = OVERRIDES[type] || {};
  return out.map(item => ({ ...item, ...(overrides[item.id] || {}) }));
}

function makeServer(port, types) {
  const server = http.createServer((req, res) => {
    // path like /lookup/Country  (case-insensitive match on the type segment)
    const m = /\/lookup\/([A-Za-z]+)/.exec(req.url || '');
    const type = m && m[1];
    res.setHeader('Content-Type', 'application/json; charset=utf-8');
    if (type && types.some(t => t.toLowerCase() === type.toLowerCase())) {
      res.statusCode = 200;
      res.end(JSON.stringify(items(type)));
    } else {
      // unknown lookup type on this port -> empty array (valid, keeps the resolver from throwing)
      res.statusCode = 200;
      res.end('[]');
    }
  });
  server.listen(port, '127.0.0.1', () => console.log(`[lookup-stub] :${port} serving ${types.join(', ')}`));
  server.on('error', e => console.error(`[lookup-stub] :${port} ${e.code || e.message}`));
}

for (const [port, types] of Object.entries(PORTS)) {
  makeServer(Number(port), types);
}
console.log('[lookup-stub] up. Ctrl+C to stop.');

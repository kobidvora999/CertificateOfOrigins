// Local lookup-source stub for CertificateOfOrigins Postman runs (local only; inside, the real SystemTables-family services answer).
//
// The service's ILookupUtil resolvers load their data over HTTP from SystemTables-family services that do not run on
// this box. This stub serves every lookup type CertificateOfOrigins registers, on the ports its readiness log names, with
// a small consistent data set the collections' bodies use (IL / DE / US / FR, USD / EUR / ILS, KG, BX, site SIT01).
// Global parameters (GET /parameters/{name}) are answered on every port.
//
// Run: node Postman/internal-workload-test/local-lookup-stub.js   (then clear the Resolver.* keys in Redis if the data changed)
const http = require('http');

const RANGE = 500;
const generic = (type) => Array.from({ length: RANGE }, (_, i) => ({
  id: i + 1, name: `${type} ${i + 1}`, state: 1, description: `${type} ${i + 1}`, englishName: `${type} ${i + 1}`,
}));
const named = (rows) => rows.map((r) => ({ state: 1, description: r.name, englishName: r.name, ...r }));

// Country ids: CountryIsrael = 376 in the seed, the lifecycle collection reconciles DE as 138, the mocks use 32 and 840.
const ALPHA2 = { 40: 'AT', 376: 'IL', 138: 'DE', 840: 'US', 250: 'FR', 826: 'GB', 792: 'TR', 32: 'AR', 76: 'BR', 600: 'PY', 858: 'UY', 380: 'IT', 724: 'ES', 156: 'CN' };
const COUNTRIES = Array.from({ length: 900 }, (_, i) => ({
  id: i + 1, name: `Country ${i + 1}`, state: 1, description: `Country ${i + 1}`, englishName: `Country ${i + 1}`,
  countryAlphaCode2: ALPHA2[i + 1] ?? null,
}));

const DATA = {
  Country: COUNTRIES,
  City: generic('City'),
  MeasurementUnit: named([{ id: 1, name: 'Kilogram', externalIdnum: 'KG' }, { id: 2, name: 'Unit', externalIdnum: 'U' }]),
  // Groups 1..8 exist (the collections send 1, 5, 7; 9 is the 'group does not exist' case); only 1 (EU) and 2 (MERCOSUR) have members.
  CountryGroup: named(Array.from({ length: 8 }, (_, i) => ({ id: i + 1, name: ['EU', 'MERCOSUR'][i] ?? `Group ${i + 1}`, isForTradeAgreement: true }))),
  CountryCountryGroup: named([[138, 1], [250, 1], [380, 1], [724, 1], [32, 2], [76, 2], [600, 2], [858, 2]]
    .map(([countryId, countryGroupId], i) => ({ id: i + 1, name: `${countryId} in ${countryGroupId}`, countryId, countryGroupId }))),
  CurrencyType: named([{ id: 1, name: 'Dollar', currencyCode: 'USD' }, { id: 2, name: 'Euro', currencyCode: 'EUR' }, { id: 3, name: 'Shekel', currencyCode: 'ILS' }]),
  DocumentType: generic('DocumentType'),
  OrganizationUnit: generic('OrganizationUnit'),
  DataDictionaryField: generic('DataDictionaryField'),
  InternationalSite: named([{ id: 1, name: 'Haifa', locode: 'ILHFA' }, { id: 2, name: 'Ashdod', locode: 'ILASH' }]),
  PackingType: named([{ id: 1, name: 'Box', commonCode: 'BX' }, { id: 2, name: 'Pallet', commonCode: 'PX' }]),
  Site: named([{ id: 1, name: 'Site 1', typeId: 1, externalSiteNumberForMessages: 'SIT01', organizationUnitId: 1 }]),
};

const PORTS = {
  9000: ['Country', 'City', 'MeasurementUnit', 'CountryGroup', 'CountryCountryGroup', 'CurrencyType'],
  9005: ['DataDictionaryField', 'InternationalSite'],
  9006: ['DocumentType'],
  9015: ['OrganizationUnit'],
  9026: ['PackingType'],
  9029: ['Site'],
};

// Stub values for local runs only, not the real configuration.
const PARAMETERS = {
  countryisrael: 376,
  isneedtolockcertificateoforigin: true,
  issuecertificateoforiginbyworker: false,
  certificateoforiginqueryurl: 'https://query.example/{0}',
  additionalrequestsforsearchindays: 30,
  certificateoforiginsdocumentsfilter: '',
  countriesexemptedfromsendingtheplaceofmanufacture: '',
};

for (const [port, types] of Object.entries(PORTS)) {
  http.createServer((req, res) => {
    console.log(`[stub] :${port} ${req.method} ${req.url}`);
    res.setHeader('Content-Type', 'application/json; charset=utf-8');
    const pm = /\/parameters\/([A-Za-z0-9_.-]+)/.exec(req.url || '');
    if (pm) {
      const value = PARAMETERS[pm[1].toLowerCase()];
      res.statusCode = value === undefined ? 404 : 200;
      res.end(value === undefined ? JSON.stringify({ error: `no stub value for parameter '${pm[1]}'` }) : String(value));
      return;
    }
    const m = /\/lookup\/([A-Za-z]+)/.exec(req.url || '');
    const type = m && types.find((t) => t.toLowerCase() === m[1].toLowerCase());
    res.statusCode = 200;
    res.end(JSON.stringify(type ? DATA[type] : []));
  }).listen(Number(port), () => console.log(`[stub] :${port} serving ${types.join(', ')}`));
}

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
// Ports (from the readiness log): 9000 = Country + City, 9006 = DocumentType, 9015 = OrganizationUnit.
// Run:  node tools/local-lookup-stub.js         (Ctrl+C to stop)
// Then start the WebApi (see the readiness-gate bypass) and run the Postman collection.

const http = require('http');

const RANGE = 500;
const PORTS = {
  9000: ['Country', 'City'],
  9006: ['DocumentType'],
  9015: ['OrganizationUnit'],
};

function items(type) {
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
  return out;
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

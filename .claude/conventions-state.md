# Conventions State

aligned-through: C5
date: 2026-07-22
notes: >
  C4 — the platform mock-proxy pattern is applied (AddProxy<I,Real,Mock> + IMockProxy), but the REAL
  Customers endpoint (Customer/CustomersByIds) is still unverified (TODO(blocking) in ServicesConfiguration).
  Tests enable all mocks via the global 'x-mock-mode: x-mock-mode' header (InfrastructureCore.Proxy 1.10.80+;
  the old per-interface 'x-mock-proxy' list was removed). The collection prerequest sets this one header.

history:
- C1 (2026-07-22): כבר תאם — bindings מפורשים בכל endpoint, ה-BL זורק RestNotFoundException ל-route-key not-found, אין GetById-in-query.
- C2 (2026-07-22): תוקן — הוסף הבלוק הקנוני של CustomsCloud ל-.gitignore + סעיף "File Hygiene" ל-CLAUDE.md.
- C3 (2026-07-22): כבר תאם — ה-controllers משתמשים ב-ModelDTOs בלבד (אין ישות Model/*Db חוצה גבול), אין mapper ידני.
- C4 (2026-07-22): תוקן — CustomerProxy/Mock עברו לתבנית AddProxy<ICustomerProxy, CustomerProxy, CustomerMockProxy> (REAL כברירת מחדל, mock דרך x-mock-proxy), CustomerMockProxy מממש IMockProxy ומקבל IProxyMockUtil, AddHttpProxy+AddRestProxy ב-DI, prerequest ברמת האוסף ב-Postman. אומת חי.
- C5 (2026-07-22): N/A — אין מסלול העלאת צרופה נכנסת מומר בשירות עדיין.

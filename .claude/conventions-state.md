# Conventions State

aligned-through: C5
date: 2026-07-22
notes: >
  C4 — the platform mock-proxy pattern is applied (AddProxy<I,Real,Mock> + IMockProxy + x-mock-proxy),
  but the REAL Customers endpoint (Customer/CustomersByIds) is still unverified (TODO(blocking) in
  ServicesConfiguration). Tests select the mock via the collection-level x-mock-proxy prerequest.

history:
- C1 (2026-07-22): כבר תאם — bindings מפורשים בכל endpoint, ה-BL זורק RestNotFoundException ל-route-key not-found, אין GetById-in-query.
- C2 (2026-07-22): תוקן — הוסף הבלוק הקנוני של CustomsCloud ל-.gitignore + סעיף "File Hygiene" ל-CLAUDE.md.
- C3 (2026-07-22): כבר תאם — ה-controllers משתמשים ב-ModelDTOs בלבד (אין ישות Model/*Db חוצה גבול), אין mapper ידני.
- C4 (2026-07-22): תוקן — CustomerProxy/Mock עברו לתבנית AddProxy<ICustomerProxy, CustomerProxy, CustomerMockProxy> (REAL כברירת מחדל, mock דרך x-mock-proxy), CustomerMockProxy מממש IMockProxy ומקבל IProxyMockUtil, AddHttpProxy+AddRestProxy ב-DI, prerequest ברמת האוסף ב-Postman. אומת חי.
- C5 (2026-07-22): N/A — אין מסלול העלאת צרופה נכנסת מומר בשירות עדיין.

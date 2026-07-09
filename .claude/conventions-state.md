# Conventions State
aligned-through: C1
date: 2026-07-09
notes: >
  C1 חלקי: (א) CustomerProxy.GetCustomerById נשאר query-style — ממתין ליישור repo ה-Customers
  (route Customer/{id} + ExecuteWithoutValidationAsync); לאחר היישור שם — לעדכן את ה-proxy כאן.
  (ב) חוזה ה-404 של ExportDocumentAuthenticationRequest/{id} לא אומת בריצה — באג קדם-קיים
  (MaxCountExceededInterceptor: פרויקציית ה-DAL מחזירה 34 עמודות > 30) מפיל כל קריאה ב-500;
  לתקן את ה-DAL ולאמת את ה-404.
history:
- C1 (2026-07-09): הומרו 6 endpoints ל-route-style + [FromRoute] (CertificateOfOrigin/{id},
  AuthenticationRequest/{documentId}, AuthenticationRequest/File/{fileId},
  ExportDocumentAuthenticationRequest/{id}, CustomerInformation/{customerId},
  CustomerInformationByCountry/{countryId}); CertificateOfOriginID/{certificateNumber} — route
  בלי חוזה 404 (החלטת מפתח: סמנטיקת null נשמרת לצרכני GoodsItem); 4 המרות
  RestValidationException("404")→RestNotFoundException + 4 השלמות throw במקום return null;
  נחשף חוזה התבניות הגנרי TemplateData/{templateId}/{entityId} + GenerateTemplate/{templateId}/{entityId};
  7 בקשות Postman הומרו לצורת path + נוספו 6 תרחישי 404; אומת מול שירות רץ — OpenAPI מכיל את כל
  7 ה-routes, חוזה 404 עובד (4/5; החריג — הבאג הקדם-קיים ב-notes), מפתח-חלופי מחזיר 204/null.

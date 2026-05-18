namespace Customs.CRM.CertificateOfOrigins.Client.Api
{
	public class ModuleCommands
	{
		/// <summary>
		/// חיפוש תעודת מקור
		/// </summary>
		public const string SearchCertificateOfOriginsCommand = "SearchCertificateOfOriginsCommand";
		/// <summary>
		/// ניקוי פילטר תעודת מקור
		/// </summary>
		public const string CertificateOfOriginsClearCommand = "CertificateOfOriginsClearCommand";
		/// <summary>
		/// ניווט למסך עריכה
		/// </summary>
		public const string CertificateOfOriginsRowDoubleClickCommand = "CertificateOfOriginsRowDoubleClickCommand";
		/// <summary>
		/// שמירת תעודת מקור
		/// </summary>
		public const string SaveCertificateOfOriginsCommand = "SaveCertificateOfOriginsCommand";
		/// <summary>
		/// עריכת תעודת מקור
		/// </summary>
		public const string EditCertificateOfOriginsCommand = "EditCertificateOfOriginsCommand";
		/// <summary>
		/// ביטול עריכת תעודת מקור
		/// </summary>
		public const string CancelCertificateOfOriginsCommand = "CancelCertificateOfOriginsCommand";
		/// <summary>
		/// רענון פרטי תעודת מקור
		/// </summary>
		public const string RefreshCertificateOfOriginsCommand = "RefreshCertificateOfOriginsCommand";
		/// <summary>
		/// דרישה לבטוחה
		/// </summary>
		public const string GetCollateralRequestCommand = "GetCollateralRequestCommand";
		/// <summary>
		/// עריכת בקשת אימות
		/// </summary>
		public const string EditImportProcessCommand = "EditImportProcessCommand";
		/// <summary>
		/// שמירת בקשת אימות
		/// </summary>
		public const string SaveImportProcessCommand = "SaveImportProcessCommand";
		/// <summary>
		/// ביטול בקשת אימות
		/// </summary>
		public const string CancelImportProcessCommand = "CancelImportProcessCommand";
		/// <summary>
		/// רענון בקשת אימות
		/// </summary>
		public const string RefreshlImportProcessCommand = "RefreshlImportProcessCommand";
		/// <summary>
		/// בחירת פרט מכס חדש
		/// </summary>
		public const string SelectCustomItemCommand = "SelectCustomItemCommand";
		/// <summary>
		/// חיפוש בקשת אימות (יבוא)
		/// </summary>
		public const string SearchAuthenticationRequestCommand = "SearchAuthenticationRequestCommand";
		/// <summary>
		/// ניקוי חיפוש בקשת אימות  (יבוא)
		/// </summary>
		public const string AuthenticationRequestClearCommand = "AuthenticationRequestClearCommand";
		/// <summary>
		/// בחירת בקשת אימות  (יבוא)
		/// </summary>
		public const string AuthenticationRequestRowDoubleClickCommand = "AuthenticationRequestRowDoubleClickCommand";
		/// <summary>
		/// בחירת מסמך
		/// </summary>
		public const string SelectDocumentChangeCommand = "SelectDocumentChangeCommand";
		/// <summary>
		/// איגוד לפניה
		/// </summary>
		public const string CheckedAuthenticationRequestCommand = "CheckedAuthenticationRequestCommand";
		/// <summary>
		/// איגוד בקשות לפניה
		/// </summary>
		public const string MergeRequestsCommand = "MergeRequestsCommand";
		/// <summary>
		/// ביטול בתיק אימות
		/// </summary>
		public const string CancelAuthenticationRequestFileCommand = "CancelAuthenticationRequestFileCommand";
		/// <summary>
		/// עריכה בתיק אימות
		/// </summary>
		public const string EditAuthenticationRequestFileCommand = "EditAuthenticationRequestFileCommand";
		/// <summary>
		/// שמירה בתיק אימות
		/// </summary>
		public const string SaveAuthenticationRequestFileCommand = "SaveAuthenticationRequestFileCommand";
		/// <summary>
		/// רענון מסך תיק אימות
		/// </summary>
		public const string RefreshAuthenticationRequestFileCommand = "RefreshAuthenticationRequestFileCommand";
		/// <summary>
		/// שמירת מסמכים בבקשת אימות
		/// </summary>
		public const string SaveRequestDocumentsCommand = "SaveRequestDocumentsCommand";
		/// <summary>
		/// פתיחת מסך בקשת אימות יבוא
		/// </summary>
		public const string OpenImportAuthenticationRequestCommand = "OpenImportAuthenticationRequestCommand";
		/// <summary>
		/// שליחת פניה
		/// </summary>
		public const string RequestDeliverNotificationCommand = "RequestDeliverNotificationCommand";
		/// <summary>
		/// שליחת תזכורת
		/// </summary>
		public const string RemindDeliverNotificationCommand = "RemindDeliverNotificationCommand";
		/// <summary>
		/// סגירת פופ אפ אימות מסמך
		/// </summary>
		public const string ImportProcessFormViewmodelClosingCommand = "ImportProcessFormViewmodelClosingCommand";
		/// <summary>
		/// בחירת בית מכס זר
		/// </summary>
		public const string ForeignCustomerSelectedChangeCommand = "ForeignCustomerSelectedChangeCommand";
		/// <summary>
		/// ניווט ממשימה לבקשת אימות יבוא מתוך תיק אימות
		/// </summary>
		public const string NavigateToImportAuthenticationRequestFromUserTaskCommand = "NavigateToImportAuthenticationRequestFromUserTaskCommand";
		/// <summary>
		/// הוספה של אזור חדש
		/// </summary>
		public const string NewExportAuthenticationRequestManufactingAreaCommand = "NewExportAuthenticationRequestManufactingAreaCommand";
		/// <summary>
		/// שינוי סוג מסמך העדפה
		/// </summary>
		public const string PreferenceDocumentTypeIDChangedCommand = "PreferenceDocumentTypeIDChangedCommand";
		/// <summary>
		/// בחירת חשבונית
		/// </summary>
		public const string SelectInvoiceChangeCommand = "SelectInvoiceChangeCommand";
		/// <summary>
		/// האם קיים בקשות נוספות עבור יצרן או ספק
		/// </summary>
		public const string IsExistsAdditionalRequestsForImporterOrVendorCommand = "IsExistsAdditionalRequestsForImporterOrVendorCommand";
		/// <summary>
		/// שינוי בקשה
		/// </summary>
		public const string RequestSelection = "RequestSelection";
		/// <summary>
		/// שלח תזכורת ליבואן
		/// </summary>
		public const string SendReminderForImporter = "SendReminderForImporter";
		/// <summary>
		/// שלח דיוור ליבואן
		/// </summary>
		public const string SendDeliveryForImporter = "SendDeliveryForImporter";
	}
}


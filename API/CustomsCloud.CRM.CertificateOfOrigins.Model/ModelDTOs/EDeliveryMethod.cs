namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Import authentication-request file delivery method (CRM.CertificateOfOrigins_enum_DeliveryMethod). Values are the
// source of truth from the platform enum (not invented).
public enum EDeliveryMethod
{
    WasNotSend = 1,
    PostedMailing = 2,
    SentByEmailRequest = 3,
    FirstRemindSent = 4,
    SecondRemindSent = 5,
}

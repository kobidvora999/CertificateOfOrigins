namespace CertificateOfOrigins.Model.ModelDTOs;

// A template data contract with a <FreeText> tag — filled with the user's free text when the template allows it
// (legacy MergerOfTemplateArguments.AddFreeText).
public interface IFreeTextTemplateData
{
    string? FreeText { get; set; }
}

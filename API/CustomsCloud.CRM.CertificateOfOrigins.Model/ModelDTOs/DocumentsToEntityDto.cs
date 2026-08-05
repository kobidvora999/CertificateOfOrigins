namespace CustomsCloud.CRM.CertificateOfOrigins.Model.ModelDTOs;

// Legacy DocumentsToEntityDTO — attaches a set of documents to an entity (Documents microservice). Only the used
// fields are modelled (the entity + the document ids).
public class DocumentsToEntityDto
{
    public VirtualEntityDto? Entity { get; set; }

    public List<int> DocumentIds { get; set; } = [];
}

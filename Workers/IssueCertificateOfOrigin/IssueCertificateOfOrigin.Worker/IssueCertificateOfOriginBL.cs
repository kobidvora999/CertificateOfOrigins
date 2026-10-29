using CommonServicesExternal;
using Customs.CRM.CertificateOfOrigins.ExternalCommon.Common;
using Customs.Infrastructure.DocumentManagement.ExternalCommon;
using ERequestReason = CertificateOfOrigins.Model.ModelDTOs.ERequestReason;
using IssueCertificateDto = CertificateOfOrigins.Model.ModelDTOs.IssueCertificateDto;
using IssueCertificateOfOrigin.Worker.Proxies;
using malam.com.customs;
using System.Globalization;
using Task = System.Threading.Tasks.Task;
using VirtualEntity = malam.com.customs.VirtualEntity;

namespace IssueCertificateOfOrigin.Worker
{
    public class IssueCertificateOfOriginBL(IServiceProvider serviceProvider)
    {
        private readonly ICommonExternalProxy _commonExternalProxy = serviceProvider.GetRequiredService<ICommonExternalProxy>();
        private readonly IDocumentsExternalProxy _documentsExternalProxy = serviceProvider.GetRequiredService<IDocumentsExternalProxy>();
        private readonly ILogger<IssueCertificateOfOriginBL> _logger = serviceProvider.GetRequiredService<ILogger<IssueCertificateOfOriginBL>>();

        public async Task<GenerateReportByParamsResponse?> GenerateReport(int reportID, int certificateOfOriginId)
        {
            _logger.LogDebug("Generate report of certificate of origin id {CertificateOfOriginId}, report id {ReportID}", certificateOfOriginId, reportID);

            var generateRequest = new GenerateReportByParamsRequest()
            {
                reportID = reportID,
                reportParameters = new KeyValuePairOfstringstring[]
                      {
                            new KeyValuePairOfstringstring()
                            {
                                key = IssueCertificateOfOriginConsts.EntityID,
                                value = certificateOfOriginId.ToString(CultureInfo.InvariantCulture)
                            },
                            new KeyValuePairOfstringstring()
                            {
                                key = IssueCertificateOfOriginConsts.AdditionalInfo,
                                value = string.Empty
                            }
                      },
                fileFormat = IssueCertificateOfOriginConsts.PDF
            };
            return await _commonExternalProxy.GenerateReportAsync(generateRequest);
        }

        public async Task SaveCertificateOfOriginDocument(SaveCertificateAttachmentsArgsDTO saveCertificateAttachmentsArgsDTO, IssueCertificateDto issueCertificateDto)
        {
            foreach (var certificateTemplate in saveCertificateAttachmentsArgsDTO.CertificatesTemplates)
            {
                var isDraft = saveCertificateAttachmentsArgsDTO.CertificateRequestReasonCode == (int)ERequestReason.Draft || saveCertificateAttachmentsArgsDTO.AdditionalInfo == IssueCertificateOfOriginConsts.IsDraft ? IssueCertificateOfOriginConsts.Draft : IssueCertificateOfOriginConsts.Final;
                var document = new DocumentDTO()
                {
                    Title = $"{issueCertificateDto.CertificateTypeName} - {isDraft}",
                    FileName = string.Format(System.Globalization.CultureInfo.InvariantCulture, IssueCertificateOfOriginConsts.CertificateName, issueCertificateDto.CertificateTypeName, saveCertificateAttachmentsArgsDTO.CertificateNumber),
                    TypeID = certificateTemplate.DocumentTypeID,
                    OrganizationUnitID = issueCertificateDto.OrganizationUnitId,
                    EntityId = saveCertificateAttachmentsArgsDTO.CertificateID,
                    EntityTypeId = (int)EEntityType.CertificateOfOrigin
                };
                if (document.TypeID == (int)EDocumentType.ExportCertificateOfOrigin)
                {
                    document.DocumentAdditionalFieldValues = new DocumentAdditionalFieldValueDTO[]
                    {
                        new DocumentAdditionalFieldValueDTO()
                        {
                            Value = saveCertificateAttachmentsArgsDTO.CertificateNumber,
                            DocumentAdditionaFieldID = 46
                        }
                    };
                }

                var documentsOfCertificates = await _documentsExternalProxy.GetDocumentsByEntitySync(saveCertificateAttachmentsArgsDTO.CertificateID, EEntityType.CertificateOfOrigin);
                var documentEntity = new VirtualEntity()
                {
                    ID = saveCertificateAttachmentsArgsDTO.CertificateID,
                    EntityType = EEntityType.CertificateOfOrigin
                };
                if (documentsOfCertificates != null && documentsOfCertificates.Length > 0)
                {
                    await _documentsExternalProxy.DeleteDocument(documentsOfCertificates.Select(c => c.ID).ToArray(), documentEntity);
                }

                await _documentsExternalProxy.UploadDocumentAndSave(document, certificateTemplate.Content);
            }
        }
    }
}

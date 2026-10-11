using CloudNative.CloudEvents;
using Customs.CRM.CertificateOfOrigins.ExternalCommon.Common;
using IssueCertificateOfOrigin.Worker.Dal;
using IssueCertificateOfOrigin.Worker.Dto;
using IssueCertificateOfOrigin.Worker.Enums;
using IssueCertificateOfOrigin.Worker.Proxies;
using CustomsCloud.InfrastructureCore.Utils.Customers;
using CustomsCloud.InfrastructureCore.Worker;
using malam.com.customs;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using Task = System.Threading.Tasks.Task;

namespace IssueCertificateOfOrigin.Worker;

internal class IssueCertificateOfOriginHandler(IServiceProvider provider)
{
    private readonly IssueCertificateOfOriginBL issueCertificateOfOriginBL = provider.GetRequiredService<IssueCertificateOfOriginBL>();

    private readonly IIssueCertificateDal dataLayer = provider.GetRequiredService<IIssueCertificateDal>();
    private readonly ICustomerUtil _customerUtil = provider.GetRequiredService<ICustomerUtil>();
    private readonly IAppSettings appSettings = provider.GetRequiredService<IAppSettings>();

    public async Task Handle(CloudEvent? cloudEvent)
    {
        ArgumentNullException.ThrowIfNull(cloudEvent);
        var issueCertificateDto = cloudEvent.GetEntity<IssueCertificateDto>();
        await Handle(issueCertificateDto);
    }

    public async Task Handle(IssueCertificateDto? issueCertificateDto)
    {
        await Task.Delay(5000);

        if (issueCertificateDto != null && issueCertificateDto.CertificateOfOriginStatusId == (int)ECertificateOfOriginStatusCode.Published && issueCertificateDto.IsInPublishingProcess)
        {
            var generateResponse = await issueCertificateOfOriginBL.GenerateReport(issueCertificateDto.ReportId, issueCertificateDto.CertificateOfOriginId);
            if (generateResponse != null)
            {
                var saveCertificateAttachmentsArgsDTO = new SaveCertificateAttachmentsArgsDTO()
                {
                    CertificateID = issueCertificateDto.CertificateOfOriginId,
                    CertificateNumber = issueCertificateDto.CertificateNumber,
                    CertificateRequestReasonCode = issueCertificateDto.RequestReasonCode,
                    CertificateTypeID = issueCertificateDto.CertificateTypeID,
                    CertificatesTemplates = new TemplateResult[] { new TemplateResult() { Content = generateResponse.GenerateReportByParamsResult, IsPdfFormat = true, DocumentTypeID = 329 } },
                    AdditionalInfo = string.Empty
                };
                await issueCertificateOfOriginBL.SaveCertificateOfOriginDocument(saveCertificateAttachmentsArgsDTO, issueCertificateDto);
                await dataLayer.UpdateIsInPublishingProcess(issueCertificateDto.CertificateOfOriginId, false);

                var attachment = new Dto.Attachment
                {
                    content = generateResponse.GenerateReportByParamsResult,
                    fileName = IssueCertificateOfOriginConsts.CertificateNameExtension,
                };

                await SendRequestFeedbackAsync(issueCertificateDto, attachment);
            }
        }
    }

    public async Task SendRequestFeedbackAsync(IssueCertificateDto issueCertificateDto, Dto.Attachment? attachment)
    {
        var certificateOfOriginRequestFeedback = CreateCertificateOfOriginRequestFeedbackAsync(issueCertificateDto);

        var requestFeedback = new PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback()
        {
            CertificateOfOriginRequestFeedback = certificateOfOriginRequestFeedback,
            Attachment = attachment
        };

        var customer = await _customerUtil.GetCustomerById(issueCertificateDto.CreateCustomerID);
        if (customer != null)
        {
            var outgoingMessageDetails = new OutgoingMessageDetails
            {
                ConsumerEntityType = EEntityType.Customer,
                ConsumerId = issueCertificateDto.CreateCustomerID,
                ConsumerEntityTypeHebrewName = IssueCertificateOfOriginConsts.EntityTypeCustomer,
                DestinationExternalId = customer.Title,
            };

            outgoingMessageDetails.RelatedEntities.Add(new LiteEntity(issueCertificateDto.CertificateOfOriginId, (int)EEntityType.CertificateOfOrigin));

            await SendOutgoingMessage(requestFeedback, outgoingMessageDetails);
        }
    }

    private async Task SendOutgoingMessage(PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback certificateOfOriginMessage, OutgoingMessageDetails outgoingMessageDetails)
    {
        var commonProxy = provider.GetRequiredService<ICommonCloudProxy>();
        var message = SerializeItem(certificateOfOriginMessage);
        ArgumentNullException.ThrowIfNull(message);

        var request = new OutgoingMessageRequest()
        {
            ServiceName = PC_NG_2281_MSG02_CertificateOfOriginRequestFeedback.SendServiceName,
            MessageXml = message.OuterXml,
            RequestDetails = outgoingMessageDetails,
        };

        await commonProxy.SendOutgoingMessage(request);
    }

    private static XmlElement? SerializeItem(object message)
    {
        var ser = new XmlSerializer(message.GetType());
        using var mem = new MemoryStream();
        ser.Serialize(mem, message);
        mem.Seek(0, SeekOrigin.Begin);
        var doc = new XmlDocument();
        doc.Load(mem);

        return doc.DocumentElement;
    }

    public CertificateOfOriginRequestFeedback CreateCertificateOfOriginRequestFeedbackAsync(IssueCertificateDto issueCertificateDto)
    {
        var queryURLFormat = string.Format(CultureInfo.InvariantCulture, appSettings.CertificateOfOriginQueryURL, issueCertificateDto.GUID.ToString());

        var requestFeedback = new CertificateOfOriginRequestFeedback
        {
            RejectCancelReason = issueCertificateDto.RejectCancelReason,
            InternalApplication = issueCertificateDto.InternalApplication,
            CertificateOfOriginTypeCode = issueCertificateDto.CertificateTypeID,
            CertificateOfOriginStatusCode = issueCertificateDto.CertificateOfOriginStatusId,
            CertificateID = issueCertificateDto.CertificateNumber,
            FeedbackRemark = issueCertificateDto.FeedbackRemark,
            QueryURL = queryURLFormat,
            RequestReasonCode = issueCertificateDto.RequestReasonCode,
        };

        return requestFeedback;
    }
}
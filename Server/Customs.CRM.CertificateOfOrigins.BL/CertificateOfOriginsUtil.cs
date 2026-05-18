using System.Collections.Generic;
using System.Linq;
using Customs.CRM.Entities;
using Customs.CertificateOfOrigins.Entities;
using Customs.Inf.CertificateOfOrigins.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.ORM;
using MalamTeam.Infrastructure.GeneralServices.EAISchema;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Extensions;

namespace Customs.CRM.CertificateOfOrigins.BL
{
    public class CertificateOfOriginsUtil : BaseLayer
    {
        private IUnitOfWork _uow;

        public CertificateOfOriginsUtil(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public List<DetailsPerCertificate> GetDetailsPerCertificate(int certificateOfOriginTypeCodeId)
        {
            return _uow.Repository.GetQuery<DetailsPerCertificate>().Where(v => v.CertificateOfOriginTypeCodeID == certificateOfOriginTypeCodeId).ToList();
        }

        public List<CertificateDetailsTypeCodeEnum> GetCertificateDetailsTypeCode()
        {
            return _uow.Repository.GetQuery<CertificateDetailsTypeCodeEnum>().ToList();
        }

        public CertificateOfOriginTypeCodeEnum GetCertificateTypeCode(int certificateOfOriginTypeCodeId)
        {
            return _uow.Repository.GetQuery<CertificateOfOriginTypeCodeEnum>().FirstOrDefault(v => v.ID == certificateOfOriginTypeCodeId);
        }

        public List<int> GetTradeAgreementsForCertificateType(int certificateOfOriginTypeCodeId)
        {
            var tradeAgreements = _uow.Repository.GetQuery<CertificateOfOriginTypeByTradeAgreement>().Where(v => v.CertificateOfOriginTypeCodeID == certificateOfOriginTypeCodeId)
                                                                                                            .Select(v => v.TradeAgreementID).ToList();
            return tradeAgreements;
        }

        public PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackCertificateOfOriginRequestFeedback CreateCertificateOfOriginRequestFeedback(CertificateOfOrigin certificate)
        {
            var queryURLFormat = Configuration.GetConfig<string>("CertificateOfOriginQueryURL");

            queryURLFormat = string.Format(queryURLFormat, certificate.GUID.ToString());
            
            var requestFeedback = new PC_NG_2281_MSG02_CertificateOfOriginRequestFeedbackCertificateOfOriginRequestFeedback
                                      {
                                          rejectCancelReason = certificate.RejectCancelReason,
                                          internalApplication = certificate.InternalApplication,
                                          certificateOfOriginTypeCode = certificate.TypeID,
                                          certificateOfOriginStatusCode = certificate.CertificateOfOriginStatusID,
                                          certificateID = certificate.CertificateNumber,
                                          FeedbackRemark = certificate.FeedbackRemark,
                                          QueryURL = queryURLFormat,
                                          requestReasonCode = certificate.RequestReasonCode,
                     

            };

            if (certificate.IssuingDate.HasValue && certificate.IsDeclarationReleased.HasValue )
            {
                requestFeedback.IssueDateIfReleased = certificate.IsDeclarationReleased.Value ? certificate.IssuingDate: null;
                requestFeedback.IssueDateIfReleasedSpecified = requestFeedback.IssueDateIfReleased != null;
            
                requestFeedback.IssueDateIfNotReleased = !certificate.IsDeclarationReleased.Value ? certificate.IssuingDate : null; 
                requestFeedback.IssueDateIfNotReleasedSpecified = requestFeedback.IssueDateIfNotReleased != null;
            }

            return requestFeedback;
        }

        public Attachment[] CreateAttachments(CertificateOfOrigin certificate)
        {
            var bl = new CertificateOfOriginsBL(_uow);

            var templates = bl.PrintCertificateOfOriginAndSaveAttachments(certificate, string.Empty);

            if (!templates.IsNullOrEmpty())
            {
                var index = 0;
                var attachments = new Attachment[templates.Count];

                foreach (var template in templates)
                {
                    var attachment = new Attachment
                    {
                        DocumentTypeID = template.DocumentTypeID,
                        content = template.Content,
                        fileName = template.FileName
                    };

                    attachments[index] = attachment;
                    index++;
                }

                return attachments;

            }

            return null;
        }

        //CR 82472
            public List<string> GetCertificateOfOriginByDeclaration(string exportDeclarationNumber)
        {
            var certificateNumbers =
                _uow.Repository.GetQuery<CertificateOfOrigin>().Where(
                    c =>
                    c.ExportDeclarationNumber.Equals(exportDeclarationNumber) &&
                    c.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Cancelled &&
                    c.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Rejected &&
                    c.CertificateOfOriginStatusID != (int)ECertificateOfOriginStatus.Error).Select(c => c.CertificateNumber).ToList();
            return certificateNumbers;
        }
    }
}

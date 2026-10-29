using Api.Models.CertificateOfOriginsCommunity;
using AutoMapper;
using CertificateOfOrigins.Model.ModelDTOs;

namespace CertificateOfOrigins.BL.MapperProfile;

// Generated pure-REST contract types (.spec/OpenApi) <-> the internal shapes the message flow works on.
public class PureRestContractProfile : Profile
{
    public PureRestContractProfile()
    {
        // The contract's optional arrays (exception, attachment) are null when empty, not [].
        AllowNullCollections = true;

        // Community certificate-of-origin request (community/CertificateOfOrigins) -> the internal message DTO the
        // validation engine reads. The certificate numbers come from the URL; the BL sets them.
        CreateMap<CertificateOfOriginRequest, CertificateOfOriginRequestMessageDto>();
        CreateMap<AgentRequest, CertificateOfOriginAgentRequestDto>();
        CreateMap<CertificateOfOrigin, CertificateOfOriginMessageDto>()
            .ForMember(dest => dest.InvoiceDetails, opt => opt.MapFrom(src => src.InvoiceDetail))
            .ForMember(dest => dest.InSufficentworkingInd, opt => opt.MapFrom(src => src.InsufficientWorkingInd))
            .ForMember(dest => dest.InsufficentWorkingText, opt => opt.MapFrom(src => src.InsufficientWorkingText));
        CreateMap<NonManipulationCertificate, NonManipulationCertificateMessageDto>();
        CreateMap<InvoiceDetail, CertificateOfOriginMessageInvoiceDetailDto>()
            .ForMember(dest => dest.ItemDetails, opt => opt.MapFrom(src => src.ItemDetail));
        CreateMap<ItemDetail, CertificateOfOriginMessageItemDetailDto>();

        // Internal feedback -> the response: the feedback block moves to the root (R9).
        CreateMap<CertificateOfOriginRequestFeedbackResponseDto, CertificateOfOriginResult>()
            .IncludeMembers(src => src.Feedback)
            .ForMember(dest => dest.Exception, opt => opt.MapFrom(src => src.Exceptions))
            .ForMember(dest => dest.Attachment, opt => opt.MapFrom(src => src.Attachments));
        CreateMap<CertificateOfOriginRequestFeedbackDto, CertificateOfOriginResult>(MemberList.None);
        CreateMap<CertificateOfOriginExceptionDto, MessageException>();
        CreateMap<CertificateOfOriginMessageAttachmentDto, Attachment>();
    }
}

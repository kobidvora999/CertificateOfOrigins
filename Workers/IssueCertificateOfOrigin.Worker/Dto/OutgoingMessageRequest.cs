namespace IssueCertificateOfOrigin.Worker.Dto
{
    public class OutgoingMessageRequest
    {
        public string ServiceName { get; set; } = null!;
        public string MessageXml { get; set; } = null!;
        public OutgoingMessageDetails RequestDetails { get; set; } = null!;
    }
}
using malam.com.customs;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IssueCertificateOfOrigin.Worker.Dto;
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
public record LiteEntity(int Id, int EntityTypeId);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

public class OutgoingMessageDetails
{
    private string? _alternativeServiceName;

    public OutgoingMessageDetails()
    {
        ConsumerEntityType = EEntityType.Customer;
    }

    public EEntityType ConsumerEntityType { get; set; }

    public string? ConsumerEntityTypeHebrewName { get; set; }

    public List<LiteEntity> RelatedEntities { get; set; } = [];

    public string? DestinationExternalId { get; set; }

    public int ConsumerId { get; set; }

    public string? ExternalIdRequestHeader { get; set; }

    public bool IsExportMessage { get; set; }

    public string? AlternativeServiceName => _alternativeServiceName;

    public void SetAlternativeServiceName(string? alternativeServiceName)
    {
        _alternativeServiceName = alternativeServiceName;
    }
}
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CertificateOfOrigins.Model.ModelDTOs;

// The legacy portal contract (GetPC_Web_9096) serialized every DateTime as a date-only string, "yyyy-MM-dd"
// (JSONSerializeHelper: DataContractJsonSerializer with DateTimeFormat("yyyy-MM-dd")). The platform serializer would
// emit an ISO date-time (a time part the portal never received), so the web response's dates carry this converter.
public sealed class WebDateJsonConverter : JsonConverter<DateTime>
{
    public const string Format = "yyyy-MM-dd";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return DateTime.Parse(value!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}

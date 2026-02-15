using System.Text.Json;
using System.Text.Json.Serialization;
using Ordering.API.Domain.ValueObjects;

namespace Ordering.API.Infrastructure.Serialization;

public class CustomerNameJsonConverter : JsonConverter<CustomerName>
{
    public override CustomerName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new CustomerName(reader.GetString());

    public override void Write(Utf8JsonWriter writer, CustomerName value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}

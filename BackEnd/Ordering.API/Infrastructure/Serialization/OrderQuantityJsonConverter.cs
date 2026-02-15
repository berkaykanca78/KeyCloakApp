using System.Text.Json;
using System.Text.Json.Serialization;
using Ordering.API.Domain.ValueObjects;

namespace Ordering.API.Infrastructure.Serialization;

public class OrderQuantityJsonConverter : JsonConverter<OrderQuantity>
{
    public override OrderQuantity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new OrderQuantity(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, OrderQuantity value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);
}

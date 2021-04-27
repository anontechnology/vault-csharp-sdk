using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IO.Anontech.Vizivault.Schema {
  internal class PrimitiveSchemaConverter : JsonConverter<PrimitiveSchema> {
    public override PrimitiveSchema Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      string schemaString = reader.GetString();
      return (schemaString == null) ? default : (PrimitiveSchema) Enum.Parse(typeof(PrimitiveSchema), schemaString);
    }

    public override void Write(Utf8JsonWriter writer, PrimitiveSchema value, JsonSerializerOptions options) {
      writer.WriteStringValue(value.ToString().ToLower());
    }
  }
}
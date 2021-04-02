using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IO.Anontech.Vizivault {
  internal class DateTimeConverter : JsonConverter<DateTime> {
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
      string dateString = reader.GetString();
      return (dateString == null) ? default : DateTime.Parse(dateString);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) {
      writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffZ"));
    }
  }
}
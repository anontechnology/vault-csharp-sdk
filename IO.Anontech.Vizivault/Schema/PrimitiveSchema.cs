using System.Text.Json.Serialization;

namespace IO.Anontech.Vizivault.Schema {

  /// <summary>
  /// An attribute schema that is used to indicate that an attribute's value should be a single, unstructured piece of data.
  /// </summary>
  [JsonConverter(typeof(PrimitiveSchemaConverter))]
  public enum PrimitiveSchema {
    String, Integer, Boolean, Float, File, Date
  }
}
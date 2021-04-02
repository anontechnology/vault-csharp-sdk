using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IO.Anontech.Vizivault {

  /// <summary>
  /// Represents one piece of information stored in the vault.
  /// </summary>
  public class AttributeValue {

    /// <summary>
    /// A unique identifier for this attribute
    /// </summary>
    public string DataPointId {get; set;}

    /// <summary>
    /// The id of the user or entity this attribute belongs to
    /// </summary>
    public string UserId {get; set;}

    /// <summary>
    /// The name of the attribute definition that defines this attribute
    /// </summary>
    [JsonPropertyName("attribute")]
    public string AttributeKey {get; set;}

    public string Sensitivity {get; set;}

    /// <summary>
    /// The value of this attribute
    /// </summary>
    public object Value {get; set;}
    
    /// <summary>
    /// A list of regulations that are applicable to this attribute
    /// </summary>
    public List<string> Regulations {get; set;}

    /// <summary>
    /// A list of tags that are applicable to this attribute
    /// </summary>
    public List<string> Tags {get; set;}

    /// <summary>
    /// If this is set to true, the value of this attribute is not stored in the vault (and the vault is used only for reporting on it)
    /// </summary>
    public bool ReportOnly {get; set;}

    /// <summary>
    /// When this attribute was first stored in the vault
    /// </summary>
    public DateTime? CreatedDate {get; internal set;}

    /// <summary>
    /// When this attribute was most recently modified
    /// </summary>
    public DateTime? ModifiedDate {get; internal set;}

    internal AttributeValue() {
      Regulations = new List<string>();
      Tags = new List<string>();
    }
    
    public AttributeValue(string attributeDefName) : this() {
      AttributeKey = attributeDefName;
    }

    /// <summary>
    /// Attempts to retrieve the value of this attribute as the specific type.
    /// </summary>
    /// <typeparam name="T">The type to return the value as.</typeparam>
    /// <returns>The value of this attribute as type T, or default if not possible</returns>
    public T GetValueAs<T>() {
      if(Value is T tValue) {
        return tValue;
      } else if(Value is JsonElement jsonValue) {
        return JsonSerializer.Deserialize<T>(jsonValue.GetRawText());
      }
      return default;
    }
    
  }
}
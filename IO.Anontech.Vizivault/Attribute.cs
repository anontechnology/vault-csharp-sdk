using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IO.Anontech.Vizivault {

  public class Attribute {
    
    public string DataPointId {get; set;}

    public string UserId {get; set;}

    [JsonPropertyName("attribute")]
    public string AttributeKey {get; set;}

    public string Sensitivity {get; set;}

    public object Value {get; set;}
    
    public List<string> Regulations {get; set;}

    public List<string> Tags {get; set;}

    [JsonIgnore]
    public DateTime CreatedDate {get; set;}

    [JsonIgnore]
    public DateTime ModifiedDate {get; set;}

    internal Attribute() {
      Regulations = new List<string>();
      Tags = new List<string>();
    }
    
    public Attribute(string attributeDefName) : this() {
      AttributeKey = attributeDefName;
    }

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
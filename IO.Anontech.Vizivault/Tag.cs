using System;
using System.Text.Json.Serialization;

namespace IO.Anontech.Vizivault {

  public class Tag {
    public string Name {get; set;}

    [JsonIgnore]
    public DateTime CreatedDate {get; set;}
    
    [JsonIgnore]
    public DateTime ModifiedDate {get; set;}

    public Tag(string name) {
      this.Name = name;
    }

    internal Tag() {
    }

    
  }

}

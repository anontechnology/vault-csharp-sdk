using System;
using System.Text.Json.Serialization;

namespace IO.Anontech.Vizivault {

  public class Tag {
    public string Name {get; set;}

    public DateTime CreatedDate {get; set;}

    public DateTime ModifiedDate {get; set;}

    public Tag(string name) {
      this.Name = name;
    }

    internal Tag() {
    }

    
  }

}

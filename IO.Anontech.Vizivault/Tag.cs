using System;

namespace IO.Anontech.Vizivault {

  /// <summary>
  /// A tag that can be used to create arbitrary groupings of data.
  /// </summary>
  public class Tag {

    /// <summary>
    /// A human-readable string that uniquely identifies this tag.
    /// </summary>
    public string Name {get; set;}

    public DateTime? CreatedDate {get; set;}

    public DateTime? ModifiedDate {get; set;}

    public Tag(string name) {
      this.Name = name;
    }

    internal Tag() {
    }

    
  }

}

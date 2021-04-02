using System;
using IO.Anontech.Vizivault.Tagging;

namespace IO.Anontech.Vizivault {
  public class Regulation {
    public string Key {get; set;}
    public string Name {get; set;}
    public string Url {get; set;}

    public RegulationRule Rule {get; set;}

    public DateTime? CreatedDate {get; set;}
    public DateTime? ModifiedDate {get; set;}
  }
}
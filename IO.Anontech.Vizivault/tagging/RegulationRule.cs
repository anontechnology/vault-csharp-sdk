using System.Text.Json.Serialization;

namespace IO.Anontech.Vizivault.Tagging {

  [JsonConverter(typeof(RegulationRuleConverter))]
  public abstract class RegulationRule {
    
    public string Type{get;}

    protected RegulationRule(string type) {
      this.Type = type;
    }
  }

}
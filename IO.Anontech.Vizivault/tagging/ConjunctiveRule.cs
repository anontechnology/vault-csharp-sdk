using System.Collections.Generic;

namespace IO.Anontech.Vizivault.Tagging {

  public class ConjunctiveRule : RegulationRule {
    public List<RegulationRule> Constraints {get; set;}
    public ConjunctiveRule() : base("all") {
      Constraints = new List<RegulationRule>();
    }

    public void AddRule(RegulationRule rule) {
      Constraints.Add(rule);
    }
  }
}
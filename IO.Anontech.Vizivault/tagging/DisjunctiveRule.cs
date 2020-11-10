using System.Collections.Generic;

namespace IO.Anontech.Vizivault.Tagging {

  public class DisjunctiveRule : RegulationRule {
    public List<RegulationRule> Constraints {get; set;}
    public DisjunctiveRule() : base("any") {
      Constraints = new List<RegulationRule>();
    }

    public void AddRule(RegulationRule rule) {
      Constraints.Add(rule);
    }
  }
}
using System.Collections.Generic;

namespace IO.Anontech.Vizivault.Tagging {

  public class AttributeRule : RegulationRule {

    public enum AttributeListOperator {
      Any, None
    }

    public AttributeListOperator Operator {get; set;}
    public List<string> Attributes {get; set;}
    public AttributeRule() : base("attribute") {
      Attributes = new List<string>();
    }
    public AttributeRule(List<string> attributes, AttributeListOperator @operator) : base("attribute") {
      this.Attributes = attributes;
      this.Operator = @operator;
    }
  }

}
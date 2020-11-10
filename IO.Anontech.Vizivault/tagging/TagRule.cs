using System.Collections.Generic;

namespace IO.Anontech.Vizivault.Tagging {

  public class TagRule : RegulationRule {

    public enum TagListOperator {
      Any, All, None
    }

    public TagListOperator Operator {get; set;}
    public List<string> Tags {get; set;}
    public TagRule() : base("tag") {
      Tags = new List<string>();
    }
    
    public TagRule(List<string> tags, TagListOperator @operator) : base("tag") {
      this.Tags = tags;
      this.Operator = @operator;
    }
  }

}
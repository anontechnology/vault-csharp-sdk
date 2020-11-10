using System.Collections.Generic;

namespace IO.Anontech.Vizivault.Tagging {

  public class UserRule : RegulationRule {

    public enum UserValuePredicate {
      Eq, Neq, Lt, Gt, Leq, Geq, Before, After
    }
    
    public string Attribute {get; set;}
    public string Value {get; set;}
    public UserValuePredicate Predicate {get; set;}

    public UserRule() : base("user") {
    }

    public UserRule(string attribute, UserValuePredicate predicate, string value) : base("user") {
      this.Attribute = attribute;
      this.Value = value;
      this.Predicate = predicate;
    }
  }

}

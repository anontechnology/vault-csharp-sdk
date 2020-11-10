using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IO.Anontech.Vizivault.Tagging {
  internal class RegulationRuleConverter : JsonConverter<RegulationRule> {

    public override void Write(Utf8JsonWriter writer, RegulationRule rule, JsonSerializerOptions options) {
      JsonSerializer.Serialize<object>(writer, rule, options);
    }

    private static string Capitalize(string s) {
      return s.First().ToString().ToUpper() + s.Substring(1);
    }

    private RegulationRule JsonElementToRule(JsonElement element) {
      string ruleType = element.GetProperty("type").GetString();
      switch(ruleType) {
        case "any": {
          return new DisjunctiveRule() {
            Constraints = element.GetProperty("constraints").EnumerateArray().Select(e => JsonElementToRule(e)).ToList()
          };
        }
        case "all": {
          return new ConjunctiveRule() {
            Constraints = element.GetProperty("constraints").EnumerateArray().Select(e => JsonElementToRule(e)).ToList()
          };
        }
        case "attribute": {
          string opString = element.GetProperty("operator").GetString();
          if(! Enum.TryParse(Capitalize(opString), out AttributeRule.AttributeListOperator attributeOperator)) {
            throw new JsonException($"Cannot deserialize attribute rule: unknown operator {opString}");
          }
          return new AttributeRule() {
            Attributes = element.GetProperty("attributes").EnumerateArray().Select(e => e.GetString()).ToList(),
            Operator = attributeOperator
          };
        }
        case "tag": {
          string opString = element.GetProperty("operator").GetString();
          if(! Enum.TryParse(Capitalize(opString), out TagRule.TagListOperator tagOperator)) {
            throw new JsonException($"Cannot deserialize tag rule: unknown operator {opString}");
          }
          return new TagRule() {
            Tags = element.GetProperty("tags").EnumerateArray().Select(e => e.GetString()).ToList(),
            Operator = tagOperator
          };
        }
        case "user": {
          string opString = element.GetProperty("predicate").GetString();
          if(! Enum.TryParse(Capitalize(opString), out UserRule.UserValuePredicate userPredicate)) {
            throw new JsonException($"Cannot deserialize user rule: unknown predicate {opString}");
          }
          return new UserRule() {
            Value = element.GetProperty("value").GetString(),
            Attribute = element.GetProperty("attribute").GetString(),
            Predicate = userPredicate
          };
        }
        default: throw new JsonException($"Cannot deserialize rule: unknown constraint type {ruleType}");
      }
    }
    
    public override RegulationRule Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) {
      return JsonElementToRule(JsonDocument.ParseValue(ref reader).RootElement);
    }
  }
}
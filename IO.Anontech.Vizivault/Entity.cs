using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace IO.Anontech.Vizivault {

  public class Entity {

    private readonly Dictionary<string, Attribute> attributes;
    private readonly Dictionary<string, List<Attribute>> repeatedAttributes;

    internal ISet<Attribute> ChangedAttributes {get;}
    internal ISet<string> DeletedAttributes {get;}

    [JsonPropertyName("id")]
    public string Id {get; set;}

    public List<string> Tags {get; set;}

    public Entity() {
      ChangedAttributes = new HashSet<Attribute>();
      DeletedAttributes = new HashSet<string>();
      attributes = new Dictionary<string, Attribute>();
      repeatedAttributes = new Dictionary<string, List<Attribute>>();
    }

    public Entity(string id) : this() {
      this.Id = id;
    }

    internal void Purge() {
      attributes.Clear();
    }

    public void AddAttribute(string attributeKey, object value) {
      Attribute attribute = new Attribute() {
        AttributeKey = attributeKey,
        Value = value
      };

      AddAttribute(attribute);
    }

    public void AddAttribute(Attribute attribute) {
      AddAttributeWithoutPendingChange(attribute);
      ChangedAttributes.Add(attribute);
    }

    internal void AddAttributeWithoutPendingChange(Attribute attribute) {
      string attributeKey = attribute.AttributeKey;
      if(repeatedAttributes.ContainsKey(attributeKey)){
        repeatedAttributes[attributeKey].Add(attribute);
      } else if(attributes.ContainsKey(attributeKey)) {
        List<Attribute> repeatableValues = new List<Attribute> { attributes[attributeKey], attribute };
        attributes.Remove(attributeKey);
        repeatedAttributes[attributeKey] = repeatableValues;
      } else {
        attributes[attributeKey] = attribute;
      }
    }

    public Attribute GetAttribute(string attributeKey) {
      if(repeatedAttributes.ContainsKey(attributeKey)) {
        if(repeatedAttributes[attributeKey].Count == 1) {
          return repeatedAttributes[attributeKey][0];
        } else {
          throw new ApplicationException("Attribute has multiple values"); // TODO better exception
        }
      }
      if(attributes.ContainsKey(attributeKey)) return attributes[attributeKey];
      return null;
    }
    public List<Attribute> GetAttributes(string attributeKey) {
      if(attributes.ContainsKey(attributeKey)) {
        return new List<Attribute>{attributes[attributeKey]};
      } else {
        return repeatedAttributes[attributeKey];
      }
    }

    public IEnumerable<Attribute> Attributes {
      get {
        return repeatedAttributes.Values.AsEnumerable().Append(attributes.Values.ToList()).Aggregate(Enumerable.Empty<Attribute>(), (a, b) => a.Concat(b));
      }
    }

    public void ClearAttribute(string attributeKey) {
      attributes.Remove(attributeKey);
      repeatedAttributes.Remove(attributeKey);
      DeletedAttributes.Add(attributeKey);
    }

    // some methods that would be nice to have:
    // see if an attribute has multiple values
    // get an attribute as a string
    // get an attribute as a list of strings
    // get an attribute as a list of objects

    // auto-create an attribute schema from an object might be useful? can copy LAVHA code for that

    // need to figure out how to represent "add this value" vs "overwrite this value" in the context of repeatable attributes
  }
}
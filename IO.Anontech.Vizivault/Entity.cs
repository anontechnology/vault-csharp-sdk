using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace IO.Anontech.Vizivault {

  /// <summary>
  /// Represents an entity that has attributes stored in the vault.
  /// </summary>
  public class Entity {

    private readonly Dictionary<string, Attribute> attributes;
    private readonly Dictionary<string, List<Attribute>> repeatedAttributes;

    internal ISet<Attribute> ChangedAttributes {get;}
    internal ISet<string> DeletedAttributes {get;}

    /// <summary>
    /// A unique identifier for this entity, which is used to link it with its attributes.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id {get; set;}

    /// <summary>
    /// Tags that will be applied to all of this entity's attributes.
    /// </summary>
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

    /// <summary>
    /// Creates a new attribute of this entity.
    /// To commit this change to the vault, it is necessary to call vault.Save() afterwards.
    /// </summary>
    /// <param name="attributeKey">The attribute name to add</param>
    /// <param name="value">The value of the named attribute</param>
    public void AddAttribute(string attributeKey, object value) {
      Attribute attribute = new Attribute() {
        AttributeKey = attributeKey,
        Value = value
      };

      AddAttribute(attribute);
    }

    /// <summary>
    /// Creates a new attribute of this entity.
    /// To commit this change to the vault, it is necessary to call vault.Save() afterwards.
    /// </summary>
    /// <param name="attribute">An attribute object containing the name and value of the attribute being added, along with optional metadata</param>
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

    /// <summary>
    /// Gets a single-valued attribute from this entity
    /// </summary>
    /// <param name="attributeKey">The name of the attribute to retrieve</param>
    /// <returns>An attribute of this entity with the specified name</returns>
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

    /// <summary>
    /// Gets multiple values of a repeatable attribute from this entity
    /// </summary>
    /// <param name="attributeKey">The name of the attribute to retrieve</param>
    /// <returns>A list containing all of this entity's attributes that match the given attribute name</returns>
    public List<Attribute> GetAttributes(string attributeKey) {
      if(attributes.ContainsKey(attributeKey)) {
        return new List<Attribute>{attributes[attributeKey]};
      } else {
        return repeatedAttributes[attributeKey];
      }
    }

    /// <summary>
    /// A collection of all attributes of this entity
    /// </summary>
    public IEnumerable<Attribute> Attributes {
      get {
        return repeatedAttributes.Values.AsEnumerable().Append(attributes.Values.ToList()).Aggregate(Enumerable.Empty<Attribute>(), (a, b) => a.Concat(b));
      }
    }

    /// <summary>
    /// Deletes an attribute of this entity.
    /// To commit this change to the vault, it is necessary to call vault.Save() afterwards.
    /// </summary>
    /// <param name="attributeKey">The name of the attribute to delete</param>
    public void ClearAttribute(string attributeKey) {
      attributes.Remove(attributeKey);
      repeatedAttributes.Remove(attributeKey);
      DeletedAttributes.Add(attributeKey);
    }

    // potential future additions:
    // see if an attribute has multiple values
    // get an attribute as a string
    // get an attribute as a list of strings
    // get an attribute as a list of objects
    // need to figure out how to represent "add this value" vs "overwrite this value" in the context of repeatable attributes
  }
}
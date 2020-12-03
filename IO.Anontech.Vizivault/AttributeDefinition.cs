using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using IO.Anontech.Vizivault.Schema;

namespace IO.Anontech.Vizivault {

  /// <summary>
  /// Defines metadata for all attributes with the same attribute name.
  /// </summary>
  public class AttributeDefinition {

    /// <summary>
    /// The name of this attribute definition.
    /// </summary>
    public string Name {get; set;}

    public string Key {
      get {
        return Name;
      }
      set {
        Name = value;
      }
    }

    public AttributeDefinition() {
      Tags = new List<string>();
      Schema = PrimitiveSchema.String;
    }

    public AttributeDefinition(string name) : this() {
      this.Name = name;
    }

    /// <summary>
    /// An example value this attribute can take.
    /// </summary>
    public string Hint {get; set;}

    /// <summary>
    /// Whether one entity can have multiple values for this attribute.
    /// </summary>
    public bool Repeatable {get; set;}

    /// <summary>
    /// Whether this attribute's value should be indexed to allow searching on it.
    /// </summary>
    public bool Indexed {get; set;}

    /// <summary>
    /// When this attribute was created.
    /// This property is maintained by the vault and does not usually need to be changed.
    /// </summary>
    public DateTime CreatedDate {get; set;}
    
    /// <summary>
    /// When this attribute was last modified.
    /// This property is maintained by the vault and does not usually need to be changed.
    /// </summary>
    public DateTime ModifiedDate {get; set;}

    /// <summary>
    /// A list of tags that should be applied to all attributes under this definition.
    /// </summary>
    public List<string> Tags {get; set;}
    
    /// <summary>
    /// A representation of the expected structure of this attribute's value.
    /// </summary>
    public object Schema {get; private set;}

    /// <summary>
    /// Indicates that this attribute will be used to store simple, unstructured data.
    /// </summary>
    /// <param name="schema">The type of unstructured data to store</param>
    public void SetSchema(PrimitiveSchema schema) {
      this.Schema = schema;
    }

    private void AddFieldSchema(Dictionary<string, object> schemaObject, PropertyInfo f) {
      foreach (System.Attribute a in System.Attribute.GetCustomAttributes(f)) {
        if(a is SchemaIgnoreAttribute) {
          return;
        } else if(a is SchemaOverrideAttribute) {
          schemaObject.Add(f.Name, (a as SchemaOverrideAttribute).Primitive.ToString().ToLowerInvariant());
          return;
        }
      }

      Type propType = f.PropertyType;

      if(propType.IsArray){
        schemaObject.Add($"[{f.Name}]", ConstructSchema(propType.GetElementType()));
      } else if(propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>)) {

        //IEnumerable<Type> enumerableTypes = f.PropertyType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

         schemaObject.Add($"[{f.Name}]", ConstructSchema(propType.GenericTypeArguments[0]));
      } else {
        schemaObject.Add(f.Name, ConstructSchema(f.PropertyType));
      }
    }

    private object ConstructSchema(Type type) {
      if(type.Equals(typeof(string))) return "string";
      else if(type.Equals(typeof(DateTime))) return "date";
      else if(type.Equals(typeof(int))) return "int"; // TODO handle all integral types - byte, short, int, long
      else if(type.Equals(typeof(double)) || type.Equals(typeof(float))) return "float";
      else if(type.Equals(typeof(bool))) return "bool";
      
      if(type.IsEnum) return "string";

      Dictionary<string, object> schemaObject = new Dictionary<string, object>();

      while(!type.Equals(typeof(object))){
        foreach(PropertyInfo prop in type.GetProperties()) {
          AddFieldSchema(schemaObject, prop);
        }
        type = type.BaseType;
      }
      
      return schemaObject;
    }

    /// <summary>
    /// Used to indicate that this attribute will store values of the specified type.
    /// Autogenerates a schema object from the structure of the given type.
    /// This should only be used for complex, structured data; for simpler data, use SetSchema(PrimitiveSchema)
    /// </summary>
    /// <param name="schemaClass">A type that values of this attribute will belong to</param>
    public void SchemaFromClass(Type schemaClass) {
      this.Schema = ConstructSchema(schemaClass);
    }
  }

  // In the future, could also add methods allowing users to construct their own schema objects
}

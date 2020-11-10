using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using IO.Anontech.Vizivault.Schema;

namespace IO.Anontech.Vizivault {

  public class AttributeDefinition {

    
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
      Schema = "string";
    }

    public AttributeDefinition(string name) : this() {
      this.Name = name;
    }

    public string Hint {get; set;}
    public bool Repeatable {get; set;}
    public bool Indexed {get; set;}

    [JsonIgnore]
    public DateTime CreatedDate {get; set;}
    [JsonIgnore]
    public DateTime ModifiedDate {get; set;}
    public List<string> Tags {get; set;}
    public object Schema {get; private set;}

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

    public void SchemaFromClass(Type schemaClass) {
      this.Schema = ConstructSchema(schemaClass);
    }
  }

    // subattribute builder...?
    // okay yeah we don't do the arbitrary setSchema, probably, instead there's methods for constructing a schema
}

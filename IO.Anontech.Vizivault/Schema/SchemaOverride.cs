using System;

namespace IO.Anontech.Vizivault.Schema {

  [AttributeUsage(AttributeTargets.Property)]
  
  /// <summary>
  /// Properties in your model classes can be marked with [SchemaOverride] to indicate that the corresponding sub-atttributes in a generated attribute schema should have a specified primitive schema,
  /// rather than the schema that would otherwise be generated.
  /// 
  /// This is most useful for indicating extremely large data that should be stored as a file, or for classes that should be serialized as a string instead of a JSON object when they are stored in the vault.
  /// </summary>
  public class SchemaOverrideAttribute : System.Attribute {
    private readonly PrimitiveSchema primitive;
    public PrimitiveSchema Primitive {
      get {
        return primitive;
      }

    }
    public SchemaOverrideAttribute(PrimitiveSchema primitive) {
      this.primitive = primitive;
    }
  }

}
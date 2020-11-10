using System;

namespace IO.Anontech.Vizivault.Schema {

  [AttributeUsage(AttributeTargets.Property)]
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
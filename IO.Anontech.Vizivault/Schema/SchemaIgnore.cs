using System;

namespace IO.Anontech.Vizivault.Schema {

  /// <summary>
  /// Properties in your model classes can be marked with [SchemaIgnore] to indicate that attribute schemas based on them should not incorporate those properties.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public class SchemaIgnoreAttribute : System.Attribute {
  }

}
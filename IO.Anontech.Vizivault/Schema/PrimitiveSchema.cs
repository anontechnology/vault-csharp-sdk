namespace IO.Anontech.Vizivault.Schema {

  public enum PrimitiveSchema {
    String, Integer, Boolean, Float, File, Date
  }

  public static class SchemaExtensions {
    public static string ToString(this PrimitiveSchema schema) {
      return "unimplemented";
    }
  }
}
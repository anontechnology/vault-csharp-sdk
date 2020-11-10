using System;
using System.Collections.Generic;

using IO.Anontech.Vizivault.Schema;
using Xunit;

namespace IO.Anontech.Vizivault.Test {

  public class AttributeDefinitionUnitTest {

    public class ClassForTestingSchemaGeneration {
      [SchemaIgnore]
      public string TransientString {get; set;}

      [SchemaOverride(PrimitiveSchema.File)]
      public string VeryLongString {get; set;}

      public int ExampleInt {get; set;}
      public string ExampleString {get; set;}

      public int[] ExampleIntArray {get; set;}
      public string[] ExampleStringArray {get; set;}

      public List<int> ExampleIntList {get; set;}
      public List<string> ExampleStringList {get; set;}

      public class InnerClass {
        public string ExampleString {get; set;}
        public int[] ExampleIntArray {get; set;}
      }

      public InnerClass Nested {get; set;}

      public List<InnerClass> NestedList {get; set;}
    }
    
    [Fact]
    public void TestAttributeSchemaFromClass() {

      AttributeDefinition def = new AttributeDefinition();
      def.SchemaFromClass(typeof(ClassForTestingSchemaGeneration));

      Assert.IsType<Dictionary<string, object>>(def.Schema);
      Dictionary<string, object> schemaDict = def.Schema as Dictionary<string, object>;
      Assert.Collection(schemaDict,
        (pair) => {
          Assert.Equal("VeryLongString", pair.Key);
          Assert.Equal("file", pair.Value);
        },
        (pair) => {
          Assert.Equal("ExampleInt", pair.Key);
          Assert.Equal("int", pair.Value);
        },
        (pair) => {
          Assert.Equal("ExampleString", pair.Key);
          Assert.Equal("string", pair.Value);
        },
        (pair) => {
          Assert.Equal("[ExampleIntArray]", pair.Key);
          Assert.Equal("int", pair.Value);
        },
        (pair) => {
          Assert.Equal("[ExampleStringArray]", pair.Key);
          Assert.Equal("string", pair.Value);
        },
        (pair) => {
          Assert.Equal("[ExampleIntList]", pair.Key);
          Assert.Equal("int", pair.Value);
        },
        (pair) => {
          Assert.Equal("[ExampleStringList]", pair.Key);
          Assert.Equal("string", pair.Value);
        },
        (pair) => {
          Assert.Equal("Nested", pair.Key);
          Assert.Collection(pair.Value as Dictionary<string, object>,
            (pair) => {
              Assert.Equal("ExampleString", pair.Key);
              Assert.Equal("string", pair.Value);
            },
            (pair) => {
              Assert.Equal("[ExampleIntArray]", pair.Key);
              Assert.Equal("int", pair.Value);
            }
          );
        },
        (pair) => {
          Assert.Equal("[NestedList]", pair.Key);
          Assert.Collection(pair.Value as Dictionary<string, object>,
            (pair) => {
              Assert.Equal("ExampleString", pair.Key);
              Assert.Equal("string", pair.Value);
            },
            (pair) => {
              Assert.Equal("[ExampleIntArray]", pair.Key);
              Assert.Equal("int", pair.Value);
            }
          );
        }
      );
    }
  }
}
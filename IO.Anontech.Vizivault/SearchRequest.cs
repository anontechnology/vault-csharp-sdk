using System;
using System.Collections.Generic;

namespace IO.Anontech.Vizivault {

  /// <summary>
  /// A set of criteria that can be used to filter data from the vault.
  /// </summary>
  public class SearchRequest {
    public List<string> Regulations;

    public class ValueSearchRequest {
      public string Attribute {get; set;}
      public string Value {get; set;}

      public ValueSearchRequest(string attribute, string value) {
        this.Attribute = attribute;
        this.Value = value;
      }
    }

    /// <summary>
    /// A list of key-value pairs, used to search on exact values of attributes.
    /// Note that only indexed attributes can be searched this way.
    /// </summary>
    public List<ValueSearchRequest> Values {get; set;}

    /// <summary>
    /// A list of attribute names. The search query will return only attributes that have one of these names.
    /// </summary>
    public List<string> Attributes {get; set;}

    public string Sensitivity {get; set;}

    public string UserId {get; set;}

    public string Country {get; set;}
    public string Subdivision {get; set;}
    public string City {get; set;}

    public DateTime? MinCreatedDate {get; set;}
    public DateTime? MaxCreatedDate {get; set;}
    public DateTime? MinModifiedDate {get; set;}
    public DateTime? MaxModifiedDate {get; set;}

    public SearchRequest(){
      Regulations = new List<string>();
      Values = new List<ValueSearchRequest>();
      Attributes = new List<string>();
    }

    public SearchRequest(string attribute, string value) : this() {
      AddValueQuery(attribute, value);
    }

    public void AddValueQuery(string attribute, string value) {
      Values.Add(new ValueSearchRequest(attribute, value));
    }
  }
}
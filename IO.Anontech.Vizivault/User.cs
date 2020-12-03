namespace IO.Anontech.Vizivault {
  
  /// <summary>
  /// Represents a User, which is a specialized entity that corresponds to a person
  /// </summary>
  public class User : Entity {
    public User(string id) : base(id) { }

    public User() : base() {}
  }
}
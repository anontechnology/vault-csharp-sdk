using System.Collections.Generic;

namespace IO.Anontech.Vizivault {
    internal class EntityDefinition {
      public string Id {get; set;}
      public List<string> Tags {get; set;}

      internal EntityDefinition(Entity entity) {
        this.Id = entity.Id;
        this.Tags = entity.Tags;
      }
    }
}
using System.Collections.Generic;

namespace IO.Anontech.Vizivault {
  internal class StorageRequest {
    public IEnumerable<AttributeValue> Data {set; get;}

    public string Origin{set; get;}
  }
}
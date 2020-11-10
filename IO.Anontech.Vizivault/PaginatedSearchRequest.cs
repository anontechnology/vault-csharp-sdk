using System;
using System.Collections.Generic;

namespace IO.Anontech.Vizivault {
  internal class PaginatedSearchRequest {
    public SearchRequest Query {get; set;}

    public int Page {get; set;}

    public int Count {get; set;}
  }
}
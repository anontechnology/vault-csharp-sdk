using System;
using System.Net;
using System.Net.Http;

namespace IO.Anontech.Vizivault {

  public class VaultResponseException : Exception {

    public HttpStatusCode StatusCode {get;}

    public VaultResponseException(string message, HttpStatusCode status) : base(message) {
      this.StatusCode = status;
    }

  }
}
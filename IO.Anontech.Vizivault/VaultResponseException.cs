using System;
using System.Net;
using System.Net.Http;

namespace IO.Anontech.Vizivault {

  /// <summary>
  /// Thrown to indicate that the ViziVault server returned a HTTP error code.
  /// </summary>
  public class VaultResponseException : Exception {

    public HttpStatusCode StatusCode {get;}

    public VaultResponseException(string message, HttpStatusCode status) : base(message) {
      this.StatusCode = status;
    }

  }
}
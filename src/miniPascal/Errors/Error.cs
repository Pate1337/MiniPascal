using System;

namespace Errors
{
  [Serializable()]
  public class Error : System.Exception
  {
    protected string message;
    public Error() : base() { }
    public Error(string message) : base(message) { this.message = message; }
    public Error(string message, System.Exception inner) : base(message, inner) { this.message = message; }

    // A constructor is needed for serialization when an
    // exception propagates from a remoting server to the client. 
    protected Error(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    
    public override string ToString()
    {
      return "ERROR: " + this.message;
    }
  }
}
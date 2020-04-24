using System;
using Nodes;
using FileHandler;
using IO;

namespace Errors
{
  [Serializable()]
  public class Error : System.Exception
  {
    protected string message;
    protected Location Location;
    protected string Type;
    protected string lineContent;
    public Error() : base() { }
    public Error(string message) : base(message) {
      this.message = message;
      this.Type = "ERROR";
    }
    public Error(string message, System.Exception inner) : base(message, inner) {
      this.message = message;
      this.Type = "ERROR";
    }

    // A constructor is needed for serialization when an
    // exception propagates from a remoting server to the client. 
    protected Error(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    
    public Error(string message, Location loc, Reader reader)
    {
      this.message = message;
      this.Location = loc;
      this.Type = "ERROR";
      this.lineContent = reader.ReadLine(this.Location.Line);
    }
    public override string ToString()
    {
      if (this.Location != null)
      {
        string arrow = this.FormArrow();
        return $"{this.Type} (line {this.Location.Line}, column {this.Location.Column}): {this.message} [{this.Location.File}]\n\n\t{this.lineContent}\n\t{arrow}\n";
      }
      return "ERROR: " + this.message;
    }
    protected string FormArrow()
    {
      if (this.Location == null) return "";
      string arrow = "";
      int i = 0;
      while (i < this.Location.Column)
      {
        arrow += " ";
        i++;
      }
      arrow += "^";
      return arrow;
    }
    public void Print(IOHandler io)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      io.WriteLine(this.ToString());
      Console.ResetColor();
    }
  }
}
using FileHandler;
using Nodes;

namespace Errors
{
  public class SyntaxError : Error
  {
    public SyntaxError() {}
    public SyntaxError(string message, Location loc, Reader reader)
    {
      this.message = message;
      this.Location = loc;
      this.Type = "SYNTAX ERROR";
      this.lineContent = reader.ReadLine(this.Location.Line);
    }
    public override string ToString()
    {
      string arrow = this.FormArrow();
      return this.Type + " (line " + this.Location.Line + ", column " + this.Location.Column + "): " + this.message + "[" + this.Location.File + "]\n\n\t" + this.lineContent + "\n\t" + arrow + "\n";
    }
  }
}
using FileHandler;
using Semantic;
using Nodes;

namespace Errors
{
  public class OperationError : Error
  {
    private string op;
    private BuiltInType t1;
    private BuiltInType t2;
    public OperationError() {}
    public OperationError(string op, BuiltInType t1, BuiltInType t2, Location loc, Reader reader)
    {
      this.message = "";
      this.Location = loc;
      this.Type = "OPERATION ERROR";
      this.lineContent = reader.ReadLine(this.Location.Line);
      this.op = op;
      this.t1 = t1;
      this.t2 = t2;
    }
    public override string ToString()
    {
      string arrow = this.FormArrow();
      return $"{this.Type} (line {this.Location.Line}, column {this.Location.Column}): Can not do operation \"{this.op}\" between {this.t1} and {this.t2}. {this.message}[{this.Location.File}]\n\n\t{this.lineContent}\n\t{arrow}\n";
    }
  }
}
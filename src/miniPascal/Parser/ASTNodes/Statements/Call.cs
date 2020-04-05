using Semantic;

namespace Nodes
{
  public class Call : Node, Statement, Factor
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public Arguments Arguments { get; set; }
    public bool Size { get; set; }
    public Call()
    {
      this.Size = false;
      this.Style = "Call";
      this.Arguments = null;
    }
    void Statement.Visit(Visitor v)
    {
      v.VisitCall(this);
    }
    BuiltInType Factor.Visit(Visitor v)
    {
      return v.VisitCall(this);
    }
  }
}
using Semantic;

namespace Nodes
{
  public class WriteStatement : Node, Statement
  {
    public string Style { get; set; }
    public Arguments Arguments { get; set; }
    public WriteStatement()
    {
      this.Style = "WriteStatement";
      this.Arguments = null;
    }
    public void Visit(Visitor v)
    {
      v.VisitWriteStatement(this);
    }
  }
}
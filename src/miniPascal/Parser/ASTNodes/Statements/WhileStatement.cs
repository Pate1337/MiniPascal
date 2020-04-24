using Semantic;

namespace Nodes
{
  public class WhileStatement : Node, Statement
  {
    public string Style { get; set; }
    public Expression BooleanExpression { get; set; }
    public Statement Statement { get; set; }
    public Location Location { get; set; }
    public WhileStatement()
    {
      this.Style = "WhileStatement";
    }
    public void Visit(Visitor v)
    {
      v.VisitWhileStatement(this);
    }
  }
}
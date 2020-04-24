using Semantic;

namespace Nodes
{
  public class AssertStatement : Node, Statement
  {
    public string Style { get; set; }
    public Expression BooleanExpression { get; set; }
    public Location Location { get; set; }
    public AssertStatement()
    {
      this.Style = "AssertStatement";
    }
    public void Visit(Visitor v)
    {
      v.VisitAssertStatement(this);
    }
  }
}
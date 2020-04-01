using Semantic;

namespace Nodes
{
  public class ClosedExpression : Node, Factor, Expression
  {
    public string Style { get; set; }
    public bool Size { get; set; }
    public Expression Expression { get; set; }
    public ClosedExpression()
    {
      this.Style = "ClosedExpression";
      this.Size = false;
    }
    public void Visit(Visitor v)
    {
      v.VisitClosedExpression(this);
    }
  }
}
using Semantic;

namespace Nodes
{
  public class ClosedExpression : Node, Factor, Expression
  {
    public string Style { get; set; }
    public bool Size { get; set; }
    public Expression Expression { get; set; }
    // Location of Size
    public Location SizeLocation { get; set; }
    public ClosedExpression()
    {
      this.Style = "ClosedExpression";
      this.Size = false;
      this.SizeLocation = null;
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitClosedExpression(this);
    }
  }
}
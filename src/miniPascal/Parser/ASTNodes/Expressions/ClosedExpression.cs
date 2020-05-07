using Semantic;

namespace Nodes
{
  public class ClosedExpression : Node, Factor, Expression
  {
    public string Style { get; set; }
    public bool Size { get; set; }
    public Expression Expression { get; set; }
    public Location SizeLocation { get; set; }
    // Location of LeftParenthesis
    public Location Location { get; set; }
    public BuiltInType Type { get; set; }
    public ClosedExpression()
    {
      this.Style = "ClosedExpression";
      this.Size = false;
      this.SizeLocation = null;
      this.Type = BuiltInType.Error;
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitClosedExpression(this);
    }
  }
}
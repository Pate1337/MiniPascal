using Semantic;

namespace Nodes
{
  public class BooleanExpression : Node, Expression
  {
    public string Style { get; set; }
    public SimpleExpression Left { get; set; }
    public string RelationalOperator { get; set; }
    public SimpleExpression Right { get; set; }
    // Location of RelationalOperator
    public Location Location { get; set; }
    public BooleanExpression()
    {
      this.Style = "BooleanExpression";
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitBooleanExpression(this);
    }
  }
}
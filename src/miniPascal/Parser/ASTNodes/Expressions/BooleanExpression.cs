using Semantic;

namespace Nodes
{
  // TODO: Change the name of this class.
  // Only used with RelationalOperators: <, >, <=, =, etc.
  public class BooleanExpression : Node, Expression
  {
    public string Style { get; set; }
    public SimpleExpression Left { get; set; }
    public string RelationalOperator { get; set; }
    public SimpleExpression Right { get; set; }
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
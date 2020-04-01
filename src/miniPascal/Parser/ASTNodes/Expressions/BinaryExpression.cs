using Semantic;

namespace Nodes
{
  // TODO: Change the name of this class.
  // Only used with RelationalOperators: <, >, <=, =, etc.
  public class BinaryExpression : Node, Expression
  {
    public string Style { get; set; }
    public SimpleExpression Left { get; set; }
    public string RelationalOperator { get; set; }
    public SimpleExpression Right { get; set; }
    public BinaryExpression()
    {
      this.Style = "BinaryExpression";
    }
    public void Visit(Visitor v)
    {
      v.VisitBinaryExpression(this);
    }
  }
}
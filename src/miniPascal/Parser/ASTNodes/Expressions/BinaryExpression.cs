namespace Nodes
{
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
  }
}
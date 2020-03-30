using System.Collections.Generic;

namespace Nodes
{
  public class SimpleExpression : Node, Expression
  {
    public string Style { get; set; }
    public string Sign { get; set; }
    public Term Term { get; set; }
    public List<SimpleExpressionAddition> Additions { get; set; }
    public SimpleExpression()
    {
      this.Style = "SimpleExpression";
      this.Sign = "+";
      this.Additions = new List<SimpleExpressionAddition>();
    }
  }
}
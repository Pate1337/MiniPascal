using System.Collections.Generic;
using Semantic;

namespace Nodes
{
  public class SimpleExpression : Node, Expression
  {
    public string Style { get; set; }
    public string Sign { get; set; }
    public Term Term { get; set; }
    public List<SimpleExpressionAddition> Additions { get; set; }
    // Location of possible Sign
    public Location Location { get; set; }
    public SimpleExpression()
    {
      this.Style = "SimpleExpression";
      this.Sign = null;
      this.Additions = new List<SimpleExpressionAddition>();
      this.Location = null;
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitSimpleExpression(this);
    }
  }
}
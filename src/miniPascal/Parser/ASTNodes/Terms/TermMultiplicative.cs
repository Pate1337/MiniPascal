using Semantic;

namespace Nodes
{
  public class TermMultiplicative : Node
  {
    public string MultiplyingOperator { get; set; }
    public Factor Factor { get; set; }
    public string Style { get; set; }
    public TermMultiplicative()
    {
      this.Style = "TermMultiplicative";
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitTermMultiplicative(this);
    }
  }
}
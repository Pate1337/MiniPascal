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
    public void Visit(Visitor v)
    {
      v.VisitTermMultiplicative(this);
    }
  }
}
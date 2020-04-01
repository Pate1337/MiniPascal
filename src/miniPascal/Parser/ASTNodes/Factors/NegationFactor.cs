using Semantic;

namespace Nodes
{
  public class NegationFactor : Node, Factor
  {
    public string Style { get; set; }
    public bool Size { get; set; }
    public Factor Factor { get; set; }
    public NegationFactor()
    {
      this.Style = "NegationFactor";
      this.Size = false;
    }
    public void Visit(Visitor v)
    {
      v.VisitNegationFactor(this);
    }
  }
}
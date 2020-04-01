using Semantic;

namespace Nodes
{
  public class RealLiteral : Node, Factor
  {
    public string Style { get; set; }
    public bool Size { get; set; }
    public string Value { get; set; }

    public RealLiteral()
    {
      this.Style = "RealLiteral";
      this.Size = false;
    }
    public void Visit(Visitor v)
    {
      v.VisitRealLiteral(this);
    }
  }
}
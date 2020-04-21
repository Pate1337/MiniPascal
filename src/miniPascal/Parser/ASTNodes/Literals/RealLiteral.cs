using Semantic;

namespace Nodes
{
  public class RealLiteral : Node, Factor
  {
    public string Style { get; set; }
    public bool Size { get; set; }
    public string Value { get; set; }
    public Location SizeLocation { get; set; }
    public Location Location { get; set; }
    public RealLiteral()
    {
      this.Style = "RealLiteral";
      this.Size = false;
      this.SizeLocation = null;
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitRealLiteral(this);
    }
  }
}
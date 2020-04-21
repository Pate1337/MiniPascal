using Semantic;

namespace Nodes
{
  public class NegationFactor : Node, Factor
  {
    public string Style { get; set; }
    public bool Size { get; set; }
    public Factor Factor { get; set; }
    public Location SizeLocation { get; set; }
    // Location of "not"
    public Location Location { get; set; }
    public NegationFactor()
    {
      this.Style = "NegationFactor";
      this.Size = false;
      this.SizeLocation = null;
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitNegationFactor(this);
    }
  }
}
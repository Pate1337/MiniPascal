using Semantic;

namespace Nodes
{
  public class IntegerLiteral : Node, Factor
  {
    public string Style { get; set; }
    public bool Size { get; set; }
    public string Value { get; set; }
    public Location SizeLocation { get; set; }
    public IntegerLiteral()
    {
      this.Style = "IntegerLiteral";
      this.Size = false;
      this.SizeLocation = null;
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitIntegerLiteral(this);
    }
  }
}
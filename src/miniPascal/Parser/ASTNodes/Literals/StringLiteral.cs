using Semantic;

namespace Nodes
{
  public class StringLiteral : Node, Factor
  {
    public string Style { get; set; }
    public bool Size { get; set; }
    public string Value { get; set; }
    public Location SizeLocation { get; set; }

    public StringLiteral()
    {
      this.Style = "StringLiteral";
      this.Size = false;
      this.SizeLocation = null;
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitStringLiteral(this);
    }
  }
}
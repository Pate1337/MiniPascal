using Semantic;

namespace Nodes
{
  public class StringLiteral : Node, Factor
  {
    public string Style { get; set; }
    public bool Size { get; set; }
    public string Value { get; set; }

    public StringLiteral()
    {
      this.Style = "StringLiteral";
      this.Size = false;
    }
    public void Visit(Visitor v)
    {
      v.VisitStringLiteral(this);
    }
  }
}
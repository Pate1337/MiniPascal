using Semantic;

namespace Nodes
{
  public class SimpleType : Node, Type
  {
    public string Style { get; set; }
    public string Type { get; set; }
    public SimpleType()
    {
      this.Style = "SimpleType";
    }
    public void Visit(Visitor v)
    {
      v.VisitSimpleType(this);
    }
  }
}
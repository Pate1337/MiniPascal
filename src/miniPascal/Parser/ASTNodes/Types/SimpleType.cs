using Semantic;

namespace Nodes
{
  public class SimpleType : Type, Node
  {
    public string Style { get; set; }
    public BuiltInType Type { get; set; }
    public SimpleType()
    {
      this.Style = "SimpleType";
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitSimpleType(this);
    }
  }
}
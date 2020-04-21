using Semantic;

namespace Nodes
{
  public class ValueParameter : Node, Parameter
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public Type Type { get; set; }
    public Location Location { get; set; }
    public ValueParameter()
    {
      this.Style = "ValueParameter";
    }
    public SymbolTableEntry Visit(Visitor v)
    {
      return v.VisitValueParameter(this);
    }
  }
}
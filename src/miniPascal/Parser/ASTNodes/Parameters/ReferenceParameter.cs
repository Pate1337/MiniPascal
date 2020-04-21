using Semantic;

namespace Nodes
{
  public class ReferenceParameter : Node, Parameter
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public Type Type { get; set; }
    public Location Location { get; set; }
    public ReferenceParameter()
    {
      this.Style = "ReferenceParameter";
    }
    public SymbolTableEntry Visit(Visitor v)
    {
      return v.VisitReferenceParameter(this);
    }
  }
}
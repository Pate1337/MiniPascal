using System.Collections.Generic;
using Semantic;

namespace Nodes
{
  public class Function : Node, NodeWithBlock
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public Block Block { get; set; }
    public Type Type { get; set; }
    public List<Parameter> Parameters;
    public Function()
    {
      this.Style = "Function";
      this.Parameters = new List<Parameter>();
    }
    public void Visit(Visitor v)
    {
      v.VisitFunction(this);
    }
  }
}
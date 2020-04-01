using System.Collections.Generic;
using Semantic;

namespace Nodes
{
  public class Procedure : Node, NodeWithBlock
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public Block Block { get; set; }
    public List<Parameter> Parameters { get; set; }
    public Procedure()
    {
      this.Style = "Procedure";
      this.Parameters = new List<Parameter>();
    }
    public void Visit(Visitor v)
    {
      v.VisitProcedure(this);
    }
  }
}
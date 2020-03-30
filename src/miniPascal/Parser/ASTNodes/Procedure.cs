using System.Collections.Generic;

namespace Nodes
{
  public class Procedure : Node, NodeWithBlock
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public Block Block { get; set; }
    public List<Parameter> Parameters;
    public Procedure()
    {
      this.Style = "Procedure";
      this.Parameters = new List<Parameter>();
    }
  }
}
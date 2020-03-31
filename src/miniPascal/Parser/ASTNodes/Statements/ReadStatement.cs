using System.Collections.Generic;

namespace Nodes
{
  public class ReadStatement : Node, Statement
  {
    public string Style { get; set; }
    public List<Variable> Variables { get; set; }
    public ReadStatement()
    {
      this.Style = "ReadStatement";
      this.Variables = new List<Variable>();
    }
  }
}
using System.Collections.Generic;
using Semantic;

namespace Nodes
{
  public class ReadStatement : Node, Statement
  {
    public string Style { get; set; }
    public List<Variable> Variables { get; set; }
    public Location Location { get; set; }
    public ReadStatement()
    {
      this.Style = "ReadStatement";
      this.Variables = new List<Variable>();
    }
    public void Visit(Visitor v)
    {
      v.VisitReadStatement(this);
    }
  }
}
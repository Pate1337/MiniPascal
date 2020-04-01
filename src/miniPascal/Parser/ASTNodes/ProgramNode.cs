using System.Collections.Generic;
using Semantic;

namespace Nodes
{
  public class ProgramNode : Node, NodeWithBlock
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public List<Procedure> Procedures {get; set; }
    public List<Function> Functions { get; set; }
    public Block Block { get; set; }
    public ProgramNode()
    {
      this.Style = "Program";
      this.Procedures = new List<Procedure>();
      this.Functions = new List<Function>();
    }
    public void AddProcedure(Procedure p)
    {
      this.Procedures.Add(p);
    }
    public void AddFunction(Function f)
    {
      this.Functions.Add(f);
    }
    public void Visit(Visitor v)
    {
      v.VisitProgram(this);
    }
  }
}
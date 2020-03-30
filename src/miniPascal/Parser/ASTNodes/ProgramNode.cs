using System.Collections.Generic;

namespace Nodes
{
  public class ProgramNode : Node, NodeWithBlock
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public List<Procedure> Procedures;
    public List<Function> Functions;
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
  }
}
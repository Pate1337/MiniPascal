using Semantic;

namespace Nodes
{
  public class AssignmentStatement : Node, Statement
  {
    public string Style { get; set; }
    public Variable Variable { get; set; }
    public Expression Expression { get; set; }
    public Location Location { get; set; }
    public AssignmentStatement()
    {
      this.Style = "AssignmentStatement";
    }
    public void Visit(Visitor v)
    {
      v.VisitAssignmentStatement(this);
    }
  }
}
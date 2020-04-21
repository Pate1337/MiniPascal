using Semantic;

namespace Nodes
{
  public class Variable : Node, Factor
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public bool Size { get; set; }
    // If IntegerExpression is set, means x[IntegerExpression]
    public Expression IntegerExpression { get; set; }
    public Location SizeLocation { get; set; }
    // Location of Name
    public Location Location { get; set; }
    public Variable()
    {
      this.Size = false;
      this.Style = "Variable";
      this.IntegerExpression = null;
      this.SizeLocation = null;
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitVariable(this);
    }
  }
}
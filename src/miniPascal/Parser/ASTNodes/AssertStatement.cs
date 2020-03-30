namespace Nodes
{
  public class AssertStatement : Node, Statement
  {
    public string Style { get; set; }
    public Expression BooleanExpression { get; set; }
    public AssertStatement()
    {
      this.Style = "AssertStatement";
    }
  }
}
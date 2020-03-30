namespace Nodes
{
  public class WhileStatement : Node, Statement
  {
    public string Style { get; set; }
    public Expression BooleanExpression { get; set; }
    public Statement Statement { get; set; }
    public WhileStatement()
    {
      this.Style = "WhileStatement";
    }
  }
}
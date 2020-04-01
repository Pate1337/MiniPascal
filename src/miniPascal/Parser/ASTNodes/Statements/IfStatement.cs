using Semantic;

namespace Nodes
{
  public class IfStatement : Node, Statement
  {
    public string Style { get; set; }
    public Expression BooleanExpression { get; set; }
    public Statement ThenStatement { get; set; }
    public Statement ElseStatement { get; set; }
    public IfStatement()
    {
      this.Style = "IfStatement";
    }
    public void Visit(Visitor v)
    {
      
    }
  }
}
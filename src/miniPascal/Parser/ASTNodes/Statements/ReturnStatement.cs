using Semantic;

namespace Nodes
{
  public class ReturnStatement : Node, Statement
  {
    public string Style { get; set; }
    public Expression Expression { get; set; }
    public ReturnStatement()
    {
      this.Style = "ReturnStatement";
    }
    public void Visit(Visitor v)
    {
      
    }
  }
}
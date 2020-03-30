using System.Collections.Generic;

namespace Nodes
{
  public class Block : Node, Statement
  {
    public string Style { get; set; }
    public List<Statement> statements;
    public Block(string style)
    {
      this.Style = style;
      this.statements = new List<Statement>();
    }
    public void AddStatement(Statement stmt)
    {
      this.statements.Add(stmt);
    }
  }
}
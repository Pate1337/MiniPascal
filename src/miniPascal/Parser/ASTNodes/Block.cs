using System.Collections.Generic;
using Semantic;

namespace Nodes
{
  public class Block : Node, Statement
  {
    public string Style { get; set; }
    public List<Statement> statements { get; set; }
    public Block(string style)
    {
      this.Style = style;
      this.statements = new List<Statement>();
    }
    public void AddStatement(Statement stmt)
    {
      this.statements.Add(stmt);
    }
    public void Visit(Visitor v)
    {
      v.VisitBlock(this);
    }
  }
}
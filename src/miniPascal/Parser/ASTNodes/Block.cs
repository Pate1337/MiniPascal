using System.Collections.Generic;
using Semantic;

namespace Nodes
{
  public class Block : Node, Statement
  {
    public string Style { get; set; }
    public List<Statement> statements { get; set; }
    public Location Location { get; set; }
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
      v.VisitBlock(this, false);
    }
    public void Visit(Visitor v, bool needsToReturnValue)
    {
      v.VisitBlock(this, needsToReturnValue);
    }
    /*
    * This is used by TypeCheckVisitor.
    */
    /*public BuiltInType Visit(Visitor v, BuiltInType expectedType)
    {
      return v.VisitBlock(this, expectedType);
    }*/
  }
}
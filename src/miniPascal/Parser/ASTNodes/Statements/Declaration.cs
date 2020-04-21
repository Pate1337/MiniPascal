using System.Collections.Generic;
using Semantic;
using Lexer;

namespace Nodes
{
  public class Declaration : Node, Statement
  {
    public string Style { get; set; }
    // public List<string> Identifiers { get; set; }
    public List<Token> Identifiers { get; set; }
    public Type Type { get; set; }
    public Declaration()
    {
      this.Style = "Declaration";
      // this.Identifiers = new List<string>();
      this.Identifiers = new List<Token>();
    }
    public void Visit(Visitor v)
    {
      v.VisitDeclaration(this);
    }
  }
}
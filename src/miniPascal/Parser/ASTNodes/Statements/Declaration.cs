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
    public Location Location { get; set; }
    public BuiltInType BuiltInType { get; set; }
    public Declaration()
    {
      this.Style = "Declaration";
      // this.Identifiers = new List<string>();
      this.Identifiers = new List<Token>();
      this.BuiltInType = BuiltInType.Error;
    }
    public void Visit(Visitor v)
    {
      v.VisitDeclaration(this);
    }
  }
}
using System.Collections.Generic;
using Semantic;

namespace Nodes
{
  public class Arguments : Node
  {
    public string Style { get; set; }
    public List<Expression> Expressions { get; set; }
    public Arguments()
    {
      this.Style = "Arguments";
      this.Expressions = new List<Expression>();
    }
    public void Visit(Visitor v)
    {
      v.VisitArguments(this);
    }
  }
}
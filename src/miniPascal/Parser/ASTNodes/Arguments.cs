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
    public List<BuiltInType> Visit(Visitor v)
    {
      return v.VisitArguments(this);
    }
  }
}
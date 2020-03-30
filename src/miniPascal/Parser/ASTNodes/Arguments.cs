using System.Collections.Generic;

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
  }
}
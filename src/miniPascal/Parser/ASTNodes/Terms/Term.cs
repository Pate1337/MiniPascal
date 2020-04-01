using System.Collections.Generic;
using Semantic;

namespace Nodes
{
  public class Term : Node
  {
    public string Style { get; set; }
    public Factor Factor { get; set; }
    public List<TermMultiplicative> Multiplicatives { get; set; }
    public Term()
    {
      this.Style = "Term";
      this.Multiplicatives = new List<TermMultiplicative>();
    }
    public void Visit(Visitor v)
    {
      v.VisitTerm(this);
    }
  }
}
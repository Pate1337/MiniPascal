using Semantic;

namespace Nodes
{
  public interface Statement
  {
    string Style { get; set; }
    void Visit(Visitor v);
  }
}
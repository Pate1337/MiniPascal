using Semantic;

namespace Nodes
{
  public interface Node
  {
    string Style { get; set; }
    void Visit(Visitor v);
  }
}
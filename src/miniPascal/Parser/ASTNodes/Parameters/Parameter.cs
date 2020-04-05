using Semantic;

namespace Nodes
{
  public interface Parameter
  {
    string Name { get; set; }
    Type Type { get; set; }
    void Visit(Visitor v);
  }
}
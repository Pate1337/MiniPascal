using Semantic;

namespace Nodes
{
  public interface Type
  {
    BuiltInType Type { get; set; }
    BuiltInType Visit(Visitor v);
  }
}
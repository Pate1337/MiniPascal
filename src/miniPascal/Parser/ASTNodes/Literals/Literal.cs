using Semantic;

namespace Nodes
{
  public interface Literal
  {
    BuiltInType Visit(Visitor v);
    string Value { get; set; }
  }
}
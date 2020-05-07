using Semantic;

namespace Nodes
{
  public interface Expression
  {
    string Style { get; set; }
    BuiltInType Visit(Visitor v);
    BuiltInType Type { get; set; }
  }
}
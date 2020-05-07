using Semantic;

namespace Nodes
{
  public interface Parameter
  {
    string Name { get; set; }
    Type Type { get; set; }
    SymbolTableEntry Visit(Visitor v);
    Location Location { get; set; }
  }
}
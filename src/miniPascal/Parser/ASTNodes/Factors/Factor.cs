using Semantic;

namespace Nodes
{
  public interface Factor
  {
    bool Size { get; set; }
    BuiltInType Visit(Visitor v);
    Location SizeLocation { get; set; }
    Location Location { get; set; }
  }
}
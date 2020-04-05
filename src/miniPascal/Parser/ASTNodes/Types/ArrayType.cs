using Semantic;

namespace Nodes
{
  public class ArrayType : Type, Node
  {
    public string Style { get; set; }
    public BuiltInType Type { get; set; }
    public Expression IntegerExpression { get; set; }  
    public ArrayType()
    {
      this.Style = "ArrayType";
    }
    public BuiltInType Visit(Visitor v)
    {
      return v.VisitArrayType(this);
    }
  }
}
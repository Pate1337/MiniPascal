namespace Nodes
{
  public class ArrayType : Node, Type
  {
    public string Style { get; set; }
    public string Type { get; set; }
    public Expression IntegerExpression { get; set; }  
    public ArrayType()
    {
      this.Style = "ArrayType";
    }
  }
}
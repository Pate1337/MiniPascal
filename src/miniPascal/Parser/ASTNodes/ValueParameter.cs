namespace Nodes
{
  public class ValueParameter : Node, Parameter
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public Type Type { get; set; }
    public ValueParameter()
    {
      this.Style = "ValueParameter";
    }
  }
}
namespace Nodes
{
  public class ReferenceParameter : Node, Parameter
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public Type Type { get; set; }
    public ReferenceParameter()
    {
      this.Style = "RefParameter";
    }
  }
}
namespace Nodes
{
  public class Variable : Node, Factor
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public bool Size { get; set; }
    public Expression IntegerExpression { get; set; }
    public Variable()
    {
      this.Size = false;
      this.Style = "Variable";
    }
  }
}
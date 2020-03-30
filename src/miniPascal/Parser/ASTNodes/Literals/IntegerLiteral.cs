namespace Nodes
{
  public class IntegerLiteral : Node, Factor
  {
    public string Style { get; set; }
    public bool Size { get; set; }
    public int Value { get; set; }

    public IntegerLiteral()
    {
      this.Style = "IntegerLiteral";
      this.Size = false;
    }
  }
}
namespace Nodes
{
  public class TermMultiplicative : Node
  {
    public string MultiplyingOperator { get; set; }
    public Factor Factor { get; set; }
    public string Style { get; set; }
    public TermMultiplicative()
    {
      this.Style = "TermMultiplicative";
    }
  }
}
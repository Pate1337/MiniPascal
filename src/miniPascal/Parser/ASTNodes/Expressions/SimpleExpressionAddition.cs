namespace Nodes
{
  public class SimpleExpressionAddition : Node
  {
    public string AddingOperator { get; set; }
    public Term Term { get; set; }
    public string Style { get; set; }
    public SimpleExpressionAddition()
    {
      this.Style = "SimpleExpressionAddition";
    }
  }
}
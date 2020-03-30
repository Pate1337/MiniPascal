namespace Nodes
{
  public class WriteStatement : Node, Statement
  {
    public string Style { get; set; }
    public Arguments Arguments { get; set; }
    public WriteStatement()
    {
      this.Style = "WriteStatement";
    }
  }
}
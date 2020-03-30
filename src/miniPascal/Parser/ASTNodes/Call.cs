namespace Nodes
{
  public class Call : Node, Statement, Factor
  {
    public string Style { get; set; }
    public string Name { get; set; }
    public Arguments Arguments { get; set; }
    public bool Size { get; set; }
    public Call()
    {
      this.Size = false;
      this.Style = "Call";
    }
  }
}
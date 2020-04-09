namespace Nodes
{
  public class Location
  {
    public int Line { get; set; }
    public int Column { get; set; }
    public string File { get; set; }

    public Location(int line, int column, string file)
    {
      this.Line = line;
      this.Column = column;
      this.File = file;
    }
    public override string ToString()
    {
      return $"Line: {this.Line}, Column: {this.Column}, File: {this.File}";
    }
  }
}
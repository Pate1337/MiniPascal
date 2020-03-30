using FileHandler;

namespace Errors
{
  public class SyntaxError : Error
  {
    private string lineContent;
    private int line;
    private int column;
    private string Type;
    public SyntaxError() {}
    public SyntaxError(string message, int line, int column, Reader reader)
    {
      this.message = message;
      this.line = line;
      this.column = column;
      this.Type = "SYNTAX ERROR";
      this.lineContent = reader.ReadLine(line);
    }
    public override string ToString()
    {
      string arrow = this.FormArrow();
      return this.Type + " (line " + this.line + ", column " + this.column + "): " + this.message + "\n\n\t" + this.lineContent + "\n\t" + arrow + "\n";
    }
    protected string FormArrow()
    {
      string arrow = "";
      int i = 0;
      while (i < this.column)
      {
        arrow += " ";
        i++;
      }
      arrow += "^";
      return arrow;
    }
  }
}
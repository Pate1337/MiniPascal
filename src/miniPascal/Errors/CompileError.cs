namespace Errors
{
  public class CompileError : Error
  {
    public CompileError() {}
    public CompileError(string t, string message)
    {
      this.message = message;
      this.Type = t == "e" ? "EXECUTION ERROR" : "COMPILE ERROR";
    }
    public override string ToString()
    {
      // string arrow = this.FormArrow();
      return $"{this.Type}: {this.message}\n";
    }
  }
}
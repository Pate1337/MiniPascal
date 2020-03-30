using Errors;

namespace miniPascal
{
  public class ArgHandler
  {
    public string FileName { get; set; }
    public bool PrintAst { get; set; }

    public ArgHandler(string[] args)
    {
      bool fileDefined = false;
      this.PrintAst = false;
      this.FileName = "";
      foreach (string arg in args)
      {
        if (arg == "-ast") this.PrintAst = true;
        if (arg[0] != '-' && fileDefined)
        {
          throw new Error("Only filename can be defined without '-'. Other arguments must have '-' in front!");
        }
        if (arg[0] != '-')
        {
          fileDefined = true;
          this.FileName = arg;
        }
      }
      if (!fileDefined) throw new Error("No file defined in args!");
    }
  }
}
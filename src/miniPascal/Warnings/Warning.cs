using System;
using IO;
using Nodes;

namespace Errors
{
  public class Warning
  {
    private string message;
    private Location location;
    public Warning(string message, Location loc)
    {
      this.message = message;
      this.location = loc;
    }
    public void Print(IOHandler io)
    {
      Console.ForegroundColor = ConsoleColor.Blue;
      io.WriteLine($"WARNING (line {this.location.Line}, column {this.location.Column}): {this.message} [{this.location.File}]");
      Console.ResetColor();
    }
  }
}
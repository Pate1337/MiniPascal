using System;
using System.Collections.Generic;

namespace IO
{
  public class SystemIO : IOHandler
  {
    public string ReadLine()
    {
      return Console.ReadLine();
    }
    public void WriteLine(string line)
    {
      Console.WriteLine(line);
    }
    public void Write(string text)
    {
      Console.Write(text);
    }
    public List<string> GetOutput()
    {
      return new List<string>();
    }
  }
}
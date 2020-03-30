using System.Collections.Generic;
using System.Linq;

namespace FileHandler
{
  public class TestReader : Reader
  {
    public string FileName { get; set; }
    public List<string> Lines { get; set; }
    private int index;

    public TestReader(string[] lines)
    {
      this.index = 0;
      this.FileName = "Test";
      this.Lines = CreateLines(lines);
    }
    private List<string> CreateLines(string[] lines)
    {
      return lines.OfType<string>().ToList();
    }
    public string ReadNextLine()
    {
      if (this.index <= this.Lines.Count - 1) return this.Lines[this.index++];
      return null;
    }
    public string ReadLine(int line)
    {
      return this.Lines[line - 1];
    }
  }
}
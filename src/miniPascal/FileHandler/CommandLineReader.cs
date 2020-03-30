using System.Collections.Generic;
using IO;

namespace FileHandler
{
  /* This could be used for a Command line tool.
  * For this project tho, this is not needed.
  */
  public class CommandLineReader : Reader
  {
    private IOHandler reader;
    // These could be useful, if there are many FileReaders for a program (in array)
    public string FileName { get; set; }
    public List<string> Lines { get; set; }

    public CommandLineReader(IOHandler io)
    {
      this.reader = io;
      this.FileName = "in";
      this.Lines = new List<string>();
    }
    public string ReadNextLine()
    {
      string line = this.reader.ReadLine();
      if (line == "quit()") return null;
      this.Lines.Add(line);
      return line;
    }
    public string ReadLine(int line)
    {
      return this.Lines[line - 1];
    }
  }
}
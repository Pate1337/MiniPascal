using Errors;
using System.Collections.Generic;
using System.IO;

namespace FileHandler
{
  public class FileReader : Reader
  {
    private StreamReader reader;
    // These could be useful, if there are many FileReaders for a program (in array)
    public string FileName { get; set; }
    public List<string> Lines { get; set; }

    public FileReader(string file)
    {
      this.FileName = file;
      if (!Utils.File.Exists(file)) throw new Error($"File {file} could not be found!");
      this.reader = Utils.File.CreateStreamReader(file);
      this.Lines = new List<string>();
    }
    public string ReadNextLine()
    {
      try
      {
        string line = this.reader.ReadLine();
        if (line == null)
        {
          this.reader.Close(); // Release resources
        }
        this.Lines.Add(line); // Add null aswell to keep track of line count
        return line;
      }
      catch (System.ObjectDisposedException)
      {
        return null; // Keep returning null if StreamReader closed
      }
    }
    public string ReadLine(int line)
    {
      return this.Lines[line - 1];
    }
  }
}
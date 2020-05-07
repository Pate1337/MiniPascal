using Errors;
using System.Collections.Generic;
using System.IO;

namespace FileHandler
{
  public class FileWriter
  {
    private StreamWriter writer;
    public string FileName { get; set; }

    public FileWriter(string file)
    {
      this.FileName = file;
      this.writer = Utils.File.CreateStreamWriter(file);
    }
    public void WriteLine(string text)
    {
      try
      {
        this.writer.WriteLine(text);
      }
      catch (System.ObjectDisposedException)
      {
       System.Console.WriteLine("FileWriter has been closed");
      }
    }
    public void Write(string text)
    {
      try
      {
        this.writer.Write(text);
      }
      catch (System.ObjectDisposedException)
      {
       System.Console.WriteLine("FileWriter has been closed");
      }
    }
    public void Close()
    {
      this.writer.Close();
    }
  }
}
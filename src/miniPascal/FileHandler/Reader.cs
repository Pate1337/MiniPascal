using System.Collections.Generic;

namespace FileHandler
{
  public interface Reader
  {
    // These could be useful, if there are many FileReaders for a program (in array)
    string FileName { get; set; }
    List<string> Lines { get; set; }
    string ReadNextLine();
    string ReadLine(int line);
  }
}
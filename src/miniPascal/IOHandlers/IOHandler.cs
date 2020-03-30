using System.Collections.Generic;

namespace IO
{
  public interface IOHandler
  {
    string ReadLine();
    void WriteLine(string line);
    void Write(string text);
    List<string> GetOutput();
  }
}
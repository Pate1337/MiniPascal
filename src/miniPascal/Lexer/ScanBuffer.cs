using FileHandler;
// using System;

namespace Lexer
{
  public class ScanBuffer
  {
    private string buffer;
    private int pos;
    private bool empty = false;
    private Reader reader;

    public ScanBuffer(Reader reader)
    {
      this.reader = reader;
      this.buffer = this.reader.ReadNextLine();
      this.pos = 0;
    }
    public char ReadChar()
    {
      int index = this.pos;
      if (this.buffer == null || index == this.buffer.Length)
      {
        string line = this.reader.ReadNextLine(); // Returns null on last \n and the following
        if (line != null)
        {
          this.buffer += $"\n{line}";
        }
        else
        {
          this.buffer += ""; // Set some content for GetLexeme() call after
          this.empty = true;
          return '#'; // Does not matter what is returned
        }
      }
      this.pos++;
      return this.buffer[index];
    }
    public string GetLexeme(bool redoLast)
    {
      int i = 0;
      if (redoLast) i = 1;
      string lexeme = this.buffer.Substring(0, this.pos - i);
      this.buffer = this.buffer.Substring(this.pos - i);
      this.pos = 0;
      return lexeme;
    }
    public bool IsEmpty()
    {
      return this.empty;
    }
  }
}
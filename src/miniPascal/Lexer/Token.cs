namespace Lexer
{
  public class Token
  {
    public Token(TokenType type, string value)
    {
      Type = type;
      Value = value;
      LineNumber = 0;
      Column = 0;
    }
    public Token(TokenType type, string value, int lineNumber, int column)
    {
      Type = type;
      Value = value;
      LineNumber = lineNumber;
      Column = column;
    }
    public override string ToString()
    {
      return $"(Value: {Value}, Type: {Type}, Line: {LineNumber}, Column: {Column})";
    }

    public TokenType Type { get; set; }
    public string Value { get; set; }
    public int LineNumber { get; set; }
    public int Column { get; set; }
  }
}
namespace Lexer
{
  public class Token
  {
    public Token(TokenType type, string value)
    {
      Type = type;
      Value = value;
      OriginalValue = value;
      Location = new Nodes.Location(0, 0, "empty");
    }
    public Token(TokenType type, string value, int lineNumber, int column, string file)
    {
      Type = type;
      Value = value;
      OriginalValue = value;
      Location = new Nodes.Location(lineNumber, column, file);
    }
    public override string ToString()
    {
      return $"(Value: {Value}, Type: {Type}, Line: {Location.Line}, Column: {Location.Column}, File: {Location.File})";
    }

    public TokenType Type { get; set; }
    public string Value { get; set; }
    public string OriginalValue { get; set; } // Because of case-insensitivity
    public Nodes.Location Location { get; set; }
  }
}
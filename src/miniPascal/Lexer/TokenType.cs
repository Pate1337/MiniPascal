namespace Lexer
{
  public enum TokenType
  {
    Invalid,
    WhiteSpace,
    NewLine,
    EOF,
    SemiColon,
    Identifier,
    IntegerLiteral,
    RealLiteral,
    StringLiteral,
    RelationalOperator,
    AddingOperator,
    MultiplyingOperator,
    Comment,
    MultilineComment,
    LeftParenthesis,
    RightParenthesis,
    LeftBracket,
    RightBracket,
    Dot,
    Comma,
    Colon,
    Assignment,
    Keyword,
    PredefinedIdentifier,
    Negation
  }
}
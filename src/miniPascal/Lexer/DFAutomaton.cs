using System.Text.RegularExpressions;

namespace Lexer
{
  public enum AutomatonAction
  {
    Move,
    Recognize,
    Error
  }
  public class DFAutomaton
  {
    private int state;
    private AutomatonAction action;

    public DFAutomaton()
    {
      this.state = 0;
      this.action = AutomatonAction.Move;
    }
    public TokenType HandleInput(char c)
    {
      if (this.state == 0) return StartState(c.ToString()); // Start
      else if (this.state == 1) return RecognitionState(TokenType.SemiColon); // SemiColon
      else if (this.state == 2) return SuccessState(TokenType.Identifier, c.ToString(), "[a-zA-Z0-9_]"); // Identifier
      else if (this.state == 3) return SuccessState(TokenType.IntegerLiteral, c.ToString(), "[0-9]", "\\.", 4); // IntegerLiteral
      else if (this.state == 4) return ErrorState(c.ToString(), "[0-9]", 5); // Invalid
      else if (this.state == 5) return SuccessState(TokenType.RealLiteral, c.ToString(), "[0-9]", "e", 6);
      else if (this.state == 6) return ErrorState(c.ToString(), "\\+|\\-", 7, "[0-9]", 8); // Invalid
      else if (this.state == 7) return ErrorState(c.ToString(), "[0-9]", 8); // Invalid
      else if (this.state == 8) return SuccessState(TokenType.RealLiteral, c.ToString(), "[0-9]"); // RealLiteral
      else if (this.state == 9) return ErrorState(c.ToString(), "\"", 11, "\\\\", 10, "[^\\\\]|[^\"]");
      else if (this.state == 10) return ErrorState(c.ToString(), ".", 9);
      else if (this.state == 11) return RecognitionState(TokenType.StringLiteral); // StringLiteral
      else if (this.state == 12) return RecognitionState(TokenType.RelationalOperator); // RelationalOperator
      else if (this.state == 13) return SuccessState(TokenType.RelationalOperator, c.ToString(), ">|=", 12); // RelationalOperator
      else if (this.state == 14) return SuccessState(TokenType.RelationalOperator, c.ToString(), "=", 12); // RelationalOperator
      else if (this.state == 15) return RecognitionState(TokenType.AddingOperator); // AddingOperator
      else if (this.state == 16) return RecognitionState(TokenType.MultiplyingOperator); // MultiplyingOperator
      else if (this.state == 17) return SuccessState(TokenType.MultiplyingOperator, c.ToString(), "/", 18); // MultiplyingOperator
      else if (this.state == 18) return SuccessState(TokenType.Comment, c.ToString(), "[^\n|^\r|^\r\n]"); // Comment
      else if (this.state == 19) return ErrorState(c.ToString(), "\\*", 20); // invalid
      else if (this.state == 20) return ErrorState(c.ToString(), "\\*", 21, "[^\\*]"); // Invalid
      else if (this.state == 21) return ErrorState(c.ToString(), "\\}", 22, "[^\\*]", 20); // Invalid
      else if (this.state == 22) return RecognitionState(TokenType.MultilineComment); // MultilineComment
      else if (this.state == 23) return RecognitionState(TokenType.LeftParenthesis); // LeftParenthesis
      else if (this.state == 24) return RecognitionState(TokenType.RightParenthesis); // RightParenthesis
      else if (this.state == 25) return RecognitionState(TokenType.LeftBracket); // LeftBracket
      else if (this.state == 26) return RecognitionState(TokenType.RightBracket); // RightBracket
      else if (this.state == 27) return RecognitionState(TokenType.Dot); // Dot
      else if (this.state == 28) return RecognitionState(TokenType.Comma); // Comma
      else if (this.state == 29) return SuccessState(TokenType.Colon, c.ToString(), "=", 30); // Colon
      else if (this.state == 30) return RecognitionState(TokenType.Assignment); // Assignment
      return TokenType.Invalid;
    }
    private TokenType RecognitionState(TokenType type)
    {
      this.action = AutomatonAction.Recognize;
      this.state = 0;
      return type;
    }
    private TokenType SuccessState(TokenType type, string c, string continueRegex)
    {
      if (!Regex.IsMatch(c, continueRegex)) return RecognitionState(type);
      return type;
    }
    private TokenType SuccessState(TokenType type, string c, string continueRegex, string moveRegex, int nextState)
    {
      if (Regex.IsMatch(c, moveRegex)) this.state = nextState;
      else if (!Regex.IsMatch(c, continueRegex)) return RecognitionState(type);
      return type;
    }
    private TokenType SuccessState(TokenType type, string c, string moveRegex, int nextState)
    {
      if (Regex.IsMatch(c, moveRegex)) this.state = nextState;
      else return RecognitionState(type);
      return type;
    }
    private TokenType ErrorState(string c, string moveRegex, int nextState)
    {
      if (Regex.IsMatch(c, moveRegex)) this.state = nextState;
      else this.state = 0;
      return TokenType.Invalid;
    }
    private TokenType ErrorState(string c, string moveRegex1, int nextState1, string moveRegex2, int nextState2)
    {
      if (Regex.IsMatch(c, moveRegex1)) this.state = nextState1;
      else if (Regex.IsMatch(c, moveRegex2)) this.state = nextState2;
      else this.state = 0;
      return TokenType.Invalid;
    }
    private TokenType ErrorState(string c, string moveRegex1, int nextState1, string moveRegex2, int nextState2, string continueRegex)
    {
      if (Regex.IsMatch(c, moveRegex1)) this.state = nextState1;
      else if (Regex.IsMatch(c, moveRegex2)) this.state = nextState2;
      else if (!Regex.IsMatch(c, continueRegex)) this.state = 0;
      return TokenType.Invalid;
    }
    private TokenType ErrorState(string c, string moveRegex, int nextState, string continueRegex)
    {
      if (Regex.IsMatch(c, moveRegex)) this.state = nextState;
      else if (!Regex.IsMatch(c, continueRegex)) this.state = 0;
      return TokenType.Invalid;
    }
    private TokenType StartState(string c)
    {
      this.action = AutomatonAction.Move;
      if (Regex.IsMatch(c, "\n|\r|\r\n"))
      {
        // NewLine
        this.action = AutomatonAction.Error;
        return TokenType.NewLine;
      }
      else if (Regex.IsMatch(c, "\\s"))
      {
        // WhiteSpace needs to be returned separately
        this.action = AutomatonAction.Error;
        return TokenType.WhiteSpace;
      }
      else if (Regex.IsMatch(c, ";")) this.state = 1;
      else if (Regex.IsMatch(c, "[a-zA-Z]")) this.state = 2;
      else if (Regex.IsMatch(c, "[0-9]")) this.state = 3;
      else if (Regex.IsMatch(c, "\"")) this.state = 9;
      else if (Regex.IsMatch(c, "=")) this.state = 12;
      else if (Regex.IsMatch(c, "<")) this.state = 13;
      else if (Regex.IsMatch(c, ">")) this.state = 14;
      else if (Regex.IsMatch(c, "\\+|\\-")) this.state = 15;
      else if (Regex.IsMatch(c, "\\*|%")) this.state = 16;
      else if (Regex.IsMatch(c, "/")) this.state = 17;
      else if (Regex.IsMatch(c, "\\{")) this.state = 19;
      else if (Regex.IsMatch(c, "\\(")) this.state = 23;
      else if (Regex.IsMatch(c, "\\)")) this.state = 24;
      else if (Regex.IsMatch(c, "\\[")) this.state = 25;
      else if (Regex.IsMatch(c, "\\]")) this.state = 26;
      else if (Regex.IsMatch(c, "\\.")) this.state = 27;
      else if (Regex.IsMatch(c, ",")) this.state = 28;
      else if (Regex.IsMatch(c, ":")) this.state = 29;
      else this.action = AutomatonAction.Error;
      return TokenType.Invalid;
    }
    public AutomatonAction GetAction()
    {
      return this.action;
    }
  }
}
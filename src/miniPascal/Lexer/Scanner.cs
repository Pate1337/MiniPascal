using System.Text.RegularExpressions;
using FileHandler;

namespace Lexer
{
  public class Scanner
  {
    private ScanBuffer buffer;
    private DFAutomaton automaton;
    private int lineNumber;
    private int columnIndex;
    private string[] keywords = {
      "or", "and", "not", "if", "then", "else", "of", "while", "do", "begin",
      "end", "var", "array", "procedure", "function", "program", "assert", "return"
    };
    private string[] predefinedIndetifiers = {
      "Boolean", "false", "integer", "read", "real", "size", "string", "true",
      "writeln"
    };

    public Scanner(Reader reader)
    {
      this.buffer = new ScanBuffer(reader);
      this.automaton = new DFAutomaton();
      this.lineNumber = 1;
      this.columnIndex = 0;
    }
    public Token NextToken()
    {
      if (this.buffer.IsEmpty()) return new Token(TokenType.EOF, "", this.lineNumber, this.columnIndex);
      Token token = new Token(TokenType.WhiteSpace, " ");
      // Skip WhiteSpaces, NewLines, Comments and MultilineComments
      while (
        token.Type == TokenType.WhiteSpace
        || token.Type == TokenType.NewLine
        || token.Type == TokenType.Comment
        || token.Type == TokenType.MultilineComment
        )
      {
        token = RunAutomaton();
        HandleMultilines(token);
      }
      if (token.Type == TokenType.Identifier)
      {
        // TODO: Case non-sensitivity here
        bool isKeyword = HandleKeywords(token);
        if (!isKeyword) HandlePredefinedIdentifiers(token);
      }
      return token;
    }
    private bool HandleKeywords(Token token)
    {
      foreach (string k in this.keywords)
      {
        if (token.Value == k)
        {
          if (token.Value == "or") token.Type = TokenType.AddingOperator;
          else if (token.Value == "and") token.Type = TokenType.MultiplyingOperator;
          else if (token.Value == "not") token.Type = TokenType.Negation;
          else token.Type = TokenType.Keyword;
          return true;
        }
      }
      return false;
    }
    private void HandlePredefinedIdentifiers(Token token)
    {
      foreach (string p in this.predefinedIndetifiers)
      {
        if (token.Value == p)
        {
          token.Type = TokenType.PredefinedIdentifier;
          break;
        }
      }
    }
    private void HandleMultilines(Token token)
    {
      if (token.Type == TokenType.MultilineComment)
      {
        int columnOffsetAfterNewLine = 0;
        int lineOffsetAfterNewLine = 0;
        for (int i = 0; i < token.Value.Length; i++)
        {
          if (Regex.IsMatch(token.Value[i].ToString(), "\n|\r|\r\n"))
          {
            lineOffsetAfterNewLine++;
            columnOffsetAfterNewLine = 0;
          }
          else columnOffsetAfterNewLine++;
        }
        if (lineOffsetAfterNewLine > 0)
        {
          // There was a new line
          this.lineNumber += lineOffsetAfterNewLine;
          this.columnIndex = columnOffsetAfterNewLine;
        }
      }
    }
    private Token RunAutomaton()
    {
      TokenType prevTokenType = TokenType.Invalid;
      string lexeme;
      while (true)
      {
        char currChar = this.buffer.ReadChar();
        prevTokenType = this.automaton.HandleInput(currChar);
        if (this.buffer.IsEmpty()) // Indicates to get the current lexeme and return that
        {
          lexeme = this.buffer.GetLexeme(false);
          if (lexeme.Length == 0)
          {
            return new Token(TokenType.EOF, "", this.lineNumber, this.columnIndex);
          }
          return new Token(prevTokenType, lexeme, this.lineNumber, this.columnIndex - lexeme.Length);
        }
        this.columnIndex++;
        switch (this.automaton.GetAction())
        {
          case AutomatonAction.Move:
            break;
          case AutomatonAction.Recognize:
            lexeme = this.buffer.GetLexeme(true);
            this.columnIndex--;
            return new Token(prevTokenType, lexeme, this.lineNumber, this.columnIndex - lexeme.Length);
          case AutomatonAction.Error:
            // Symbol here is either WhiteSpace, NewLine or Invalid
            lexeme = this.buffer.GetLexeme(false);
            Token t = new Token(prevTokenType, lexeme, this.lineNumber, this.columnIndex - lexeme.Length);
            if (prevTokenType == TokenType.NewLine)
            {
              this.lineNumber++;
              this.columnIndex = 0;
            }
            return t;
        }
      }
    }
  }
}
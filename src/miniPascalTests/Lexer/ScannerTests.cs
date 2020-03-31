using NUnit.Framework;
using FileHandler;
using Lexer;

namespace miniPascalTests
{
  public class ScannerTests
  {
    [SetUp]
    public void Setup()
    {
    }

    private Scanner InitScanner(string[] lines)
    {
      Reader reader = new TestReader(lines);
      return new Scanner(reader);
    }
    private void Matches(string v, TokenType tt, int l, int c, Token token)
    {
      Assert.AreEqual(v, token.Value);
      Assert.AreEqual(tt, token.Type);
      Assert.AreEqual(l, token.LineNumber);
      Assert.AreEqual(c, token.Column);
    }

    [Test]
    public void EmptyFileReturnsEOF1()
    {
      string[] lines = {
        ""
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();

      Assert.AreEqual(TokenType.EOF, token.Type);
    }
    [Test]
    public void EmptyFileReturnsEOF2()
    {
      string[] lines = {
        null
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();

      Assert.AreEqual(TokenType.EOF, token.Type);
    }

    [Test]
    public void RecognizesSemiColon()
    {
      string[] lines = {
        ";"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches(";", TokenType.SemiColon, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesIdentifier1()
    {
      string[] lines = {
        "identifier"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("identifier", TokenType.Identifier, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesIdentifier2()
    {
      string[] lines = {
        "identifier13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("identifier13", TokenType.Identifier, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesIdentifier3()
    {
      string[] lines = {
        "ide1ntifi8er"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("ide1ntifi8er", TokenType.Identifier, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesIdentifier4()
    {
      string[] lines = {
        "iden__tifi4er_"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("iden__tifi4er_", TokenType.Identifier, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesIntegerLiteral()
    {
      string[] lines = {
        "1234"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("1234", TokenType.IntegerLiteral, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void SkipsWhiteSpaces()
    {
      string[] lines = {
        " 1234 ident  "
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("1234", TokenType.IntegerLiteral, 1, 1, token);

      token = scanner.NextToken();
      Matches("ident", TokenType.Identifier, 1, 6, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void SkipsNewLines1()
    {
      string[] lines = {
        " 1234 ident  ",
        "",
        "third"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("1234", TokenType.IntegerLiteral, 1, 1, token);

      token = scanner.NextToken();
      Matches("ident", TokenType.Identifier, 1, 6, token);

      token = scanner.NextToken();
      Matches("third", TokenType.Identifier, 3, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void SkipsNewLines2()
    {
      string[] lines = {
        " 1234 ident  ",
        "\n",
        "third"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("1234", TokenType.IntegerLiteral, 1, 1, token);

      token = scanner.NextToken();
      Matches("ident", TokenType.Identifier, 1, 6, token);

      token = scanner.NextToken();
      Matches("third", TokenType.Identifier, 4, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesRealLiterals()
    {
      string[] lines = {
        "2.1",
        "23.1",
        "24.144",
        "23.1e1",
        "23.12e3",
        "54.16e63",
        "4.23e+7",
        "34.5e-56"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("2.1", TokenType.RealLiteral, 1, 0, token);

      token = scanner.NextToken();
      Matches("23.1", TokenType.RealLiteral, 2, 0, token);

      token = scanner.NextToken();
      Matches("24.144", TokenType.RealLiteral, 3, 0, token);

      token = scanner.NextToken();
      Matches("23.1e1", TokenType.RealLiteral, 4, 0, token);

      token = scanner.NextToken();
      Matches("23.12e3", TokenType.RealLiteral, 5, 0, token);

      token = scanner.NextToken();
      Matches("54.16e63", TokenType.RealLiteral, 6, 0, token);

      token = scanner.NextToken();
      Matches("4.23e+7", TokenType.RealLiteral, 7, 0, token);

      token = scanner.NextToken();
      Matches("34.5e-56", TokenType.RealLiteral, 8, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesInvalidRealLiterals()
    {
      string[] lines = {
        "2."
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("2.", TokenType.Invalid, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesInvalidRealLiterals2()
    {
      string[] lines = {
        "25.",
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("25.", TokenType.Invalid, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesInvalidRealLiterals3()
    {
      string[] lines = {
        "2.3e",
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("2.3e", TokenType.Invalid, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesInvalidRealLiterals4()
    {
      string[] lines = {
        "24.32e",
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("24.32e", TokenType.Invalid, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesInvalidRealLiterals5()
    {
      string[] lines = {
        "24.32e+",
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("24.32e+", TokenType.Invalid, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesInvalidRealLiterals6()
    {
      string[] lines = {
        "2.2e-",
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("2.2e-", TokenType.Invalid, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesStringLiteral()
    {
      string[] lines = {
        "\"string\""
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("\"string\"", TokenType.StringLiteral, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesEmptyStringLiteral()
    {
      string[] lines = {
        "\"\""
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("\"\"", TokenType.StringLiteral, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesStringLiteralWithEscape()
    {
      string[] lines = {
        "\"escape\\\"string\""
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("\"escape\\\"string\"", TokenType.StringLiteral, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void StringLiteralsWithSecondToLastCharBeingEscapeAreInvalid()
    {
      string[] lines = {
        "\"invalid\\\""
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("\"invalid\\\"", TokenType.Invalid, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesEqual()
    {
      string[] lines = {
        "13=13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 0, token);
      token = scanner.NextToken();
      Matches("=", TokenType.RelationalOperator, 1, 2, token);
      token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 3, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesSmallerThan()
    {
      string[] lines = {
        "13<13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 0, token);
      token = scanner.NextToken();
      Matches("<", TokenType.RelationalOperator, 1, 2, token);
      token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 3, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesSmallerOrEqualThan()
    {
      string[] lines = {
        "13<=13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 0, token);
      token = scanner.NextToken();
      Matches("<=", TokenType.RelationalOperator, 1, 2, token);
      token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 4, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesLargerThan()
    {
      string[] lines = {
        "13>13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 0, token);
      token = scanner.NextToken();
      Matches(">", TokenType.RelationalOperator, 1, 2, token);
      token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 3, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesLargerOrEqualThan()
    {
      string[] lines = {
        "13>=13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 0, token);
      token = scanner.NextToken();
      Matches(">=", TokenType.RelationalOperator, 1, 2, token);
      token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 4, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesUnequal()
    {
      string[] lines = {
        "13<>13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 0, token);
      token = scanner.NextToken();
      Matches("<>", TokenType.RelationalOperator, 1, 2, token);
      token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 4, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesAdd()
    {
      string[] lines = {
        "13+13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 0, token);
      token = scanner.NextToken();
      Matches("+", TokenType.AddingOperator, 1, 2, token);
      token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 3, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesSubtract()
    {
      string[] lines = {
        "13-13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 0, token);
      token = scanner.NextToken();
      Matches("-", TokenType.AddingOperator, 1, 2, token);
      token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 3, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesMultiply()
    {
      string[] lines = {
        "13*13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 0, token);
      token = scanner.NextToken();
      Matches("*", TokenType.MultiplyingOperator, 1, 2, token);
      token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 3, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesDivide()
    {
      string[] lines = {
        "13/13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 0, token);
      token = scanner.NextToken();
      Matches("/", TokenType.MultiplyingOperator, 1, 2, token);
      token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 3, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesModulo()
    {
      string[] lines = {
        "13%13"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 0, token);
      token = scanner.NextToken();
      Matches("%", TokenType.MultiplyingOperator, 1, 2, token);
      token = scanner.NextToken();
      Matches("13", TokenType.IntegerLiteral, 1, 3, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void SkipsComment()
    {
      string[] lines = {
        "//comment"
      };
      Scanner scanner = InitScanner(lines);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void CommentUpdatesLineNumberCorrectly()
    {
      string[] lines = {
        "//comment",
        ";"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches(";", TokenType.SemiColon, 2, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void SkipsMultilineComment()
    {
      string[] lines = {
        "{*multilinecomment*}"
      };
      Scanner scanner = InitScanner(lines);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void MultilineCommentUpdatesLineNumberAndColumnCorrectly1()
    {
      string[] lines = {
        "{*multilinecomment*}",
        ";"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches(";", TokenType.SemiColon, 2, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void MultilineCommentUpdatesLineNumberAndColumnCorrectly2()
    {
      string[] lines = {
        "{*multilinecomment*};",
        "12"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches(";", TokenType.SemiColon, 1, 20, token);
      token = scanner.NextToken();
      Matches("12", TokenType.IntegerLiteral, 2, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void MultilineCommentUpdatesLineNumberAndColumnCorrectly3()
    {
      string[] lines = {
        "{*multi",
        "line",
        "comment",
        "*};",
        "12"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches(";", TokenType.SemiColon, 4, 2, token);
      token = scanner.NextToken();
      Matches("12", TokenType.IntegerLiteral, 5, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesLeftParenthesis()
    {
      string[] lines = {
        "("
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("(", TokenType.LeftParenthesis, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesRightParenthesis()
    {
      string[] lines = {
        ")"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches(")", TokenType.RightParenthesis, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesLeftBracket()
    {
      string[] lines = {
        "["
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("[", TokenType.LeftBracket, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesRightBracket()
    {
      string[] lines = {
        "]"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("]", TokenType.RightBracket, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesDot()
    {
      string[] lines = {
        "."
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches(".", TokenType.Dot, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesComma()
    {
      string[] lines = {
        ","
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches(",", TokenType.Comma, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesColon()
    {
      string[] lines = {
        ":"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches(":", TokenType.Colon, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesAssignment()
    {
      string[] lines = {
        ":="
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches(":=", TokenType.Assignment, 1, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesKeywords()
    {
      string[] lines = {
        "or", "and", "not", "if", "then", "else", "of", "while", "do", "begin",
        "end", "var", "array", "procedure", "function", "program", "assert", "return"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("or", TokenType.AddingOperator, 1, 0, token);

      token = scanner.NextToken();
      Matches("and", TokenType.MultiplyingOperator, 2, 0, token);

      token = scanner.NextToken();
      Matches("not", TokenType.Negation, 3, 0, token);

      token = scanner.NextToken();
      Matches("if", TokenType.Keyword, 4, 0, token);

      token = scanner.NextToken();
      Matches("then", TokenType.Keyword, 5, 0, token);

      token = scanner.NextToken();
      Matches("else", TokenType.Keyword, 6, 0, token);

      token = scanner.NextToken();
      Matches("of", TokenType.Keyword, 7, 0, token);

      token = scanner.NextToken();
      Matches("while", TokenType.Keyword, 8, 0, token);

      token = scanner.NextToken();
      Matches("do", TokenType.Keyword, 9, 0, token);

      token = scanner.NextToken();
      Matches("begin", TokenType.Keyword, 10, 0, token);

      token = scanner.NextToken();
      Matches("end", TokenType.Keyword, 11, 0, token);

      token = scanner.NextToken();
      Matches("var", TokenType.Keyword, 12, 0, token);

      token = scanner.NextToken();
      Matches("array", TokenType.Keyword, 13, 0, token);

      token = scanner.NextToken();
      Matches("procedure", TokenType.Keyword, 14, 0, token);

      token = scanner.NextToken();
      Matches("function", TokenType.Keyword, 15, 0, token);

      token = scanner.NextToken();
      Matches("program", TokenType.Keyword, 16, 0, token);

      token = scanner.NextToken();
      Matches("assert", TokenType.Keyword, 17, 0, token);

      token = scanner.NextToken();
      Matches("return", TokenType.Keyword, 18, 0, token);

      Assert.AreEqual(TokenType.EOF, scanner.NextToken().Type);
    }
    [Test]
    public void RecognizesPredefinedIdentifiers()
    {
      string[] lines = {
        "Boolean", "false", "integer", "read", "real", "size", "string", "true",
        "writeln"
      };
      Scanner scanner = InitScanner(lines);

      Token token = scanner.NextToken();
      Matches("Boolean", TokenType.PredefinedIdentifier, 1, 0, token);

      token = scanner.NextToken();
      Matches("false", TokenType.PredefinedIdentifier, 2, 0, token);

      token = scanner.NextToken();
      Matches("integer", TokenType.PredefinedIdentifier, 3, 0, token);

      token = scanner.NextToken();
      Matches("read", TokenType.PredefinedIdentifier, 4, 0, token);

      token = scanner.NextToken();
      Matches("real", TokenType.PredefinedIdentifier, 5, 0, token);

      token = scanner.NextToken();
      Matches("size", TokenType.PredefinedIdentifier, 6, 0, token);

      token = scanner.NextToken();
      Matches("string", TokenType.PredefinedIdentifier, 7, 0, token);

      token = scanner.NextToken();
      Matches("true", TokenType.PredefinedIdentifier, 8, 0, token);

      token = scanner.NextToken();
      Matches("writeln", TokenType.PredefinedIdentifier, 9, 0, token);
    }
  }
}
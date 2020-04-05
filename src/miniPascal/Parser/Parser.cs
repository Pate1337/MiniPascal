using System.Collections.Generic;
using IO;
using Lexer;
using FileHandler;
using Nodes;
using Errors;
using Semantic;

namespace miniPascal
{
  public class Parser
  {
    private IOHandler io;
    private Scanner scanner;
    private Token token;
    private Reader reader;

    public Parser(IOHandler io, Reader reader)
    {
      this.io = io;
      this.scanner = new Scanner(reader);
      this.reader = reader;
    }
    public ProgramNode Parse()
    {
      return Program();
    }
    public ProgramNode Program()
    {
      ProgramNode root = new ProgramNode();
      this.token = this.scanner.NextToken();
      Match(TokenType.Keyword, "program");
      string name = this.token.Value;
      Match(TokenType.Identifier);
      root.Name = name;
      Match(TokenType.SemiColon);
      ProceduresAndFunctions(root);
      root.Block = Block("MainBlock");
      Match(TokenType.Dot);
      return root;
    }
    private Block Block(string style)
    {
      // <block> ::= "begin" <statement> { ";" <statement> } [ ";" ] "end" 
      Match(TokenType.Keyword, "begin");
      Block block = new Block(style);
      block.AddStatement(Statement());
      Statements(block);
      if (NextIs(TokenType.SemiColon)) Match(TokenType.SemiColon);
      Match(TokenType.Keyword, "end");
      return block;
    }
    private void Statements(Block block)
    {
      // { ";" <statement> }
      while (true)
      {
        if (NextIs(TokenType.SemiColon))
        {
          Match(TokenType.SemiColon);
          if (NextIs(TokenType.Keyword, "end")) break; // No need for ";" before end
          block.AddStatement(Statement());
        }
        else break;
      }
    }
    private Statement Statement()
    {
      // <statement> ::= <simple statement> | <structured statement> | <var-declaration>
      if (
        NextIs(TokenType.Identifier) ||
        NextIs(TokenType.PredefinedIdentifier) ||
        NextIs(TokenType.Keyword, "return") ||
        NextIs(TokenType.Keyword, "assert")
        ) return SimpleStatement();
      else if (
        NextIs(TokenType.Keyword, "begin") ||
        NextIs(TokenType.Keyword, "if") ||
        NextIs(TokenType.Keyword, "while")
      ) return StructuredStatement();
      else if (NextIs(TokenType.Keyword, "var")) return Declaration();
      else throw new SyntaxError($"Statement can not start with {this.token.Type} \"{this.token.Value}\".\nExpected Identifier, PredefinedIdentifier or one of the Keywords: \"return\", \"assert\", \"begin\", \"if\", \"while\" or \"var\".", this.token.LineNumber, this.token.Column, this.reader);
    }
    private Statement SimpleStatement()
    {
      // <simple statement> ::= <assignment statement> | <call> | <return statement> |
      // <read statement> | <write statement> | <assert statement>
      switch (this.token.Type)
      {
        case (TokenType.Identifier): return StatementStartingWithIdentifier();
        case (TokenType.PredefinedIdentifier): return StatementStartingWithPredefinedIdentifier();
        case (TokenType.Keyword):
          if (NextIs(TokenType.Keyword, "return")) return ReturnStatement();
          else if (NextIs(TokenType.Keyword, "assert")) return AssertStatement();
          else throw new SyntaxError($"Statement can not start with {this.token.Type} \"{this.token.Value}\".\nExpected Keywords: \"return\" or \"assert\".", this.token.LineNumber, this.token.Column, this.reader);
        default:
          throw new SyntaxError($"Statement can not start with {this.token.Type} \"{this.token.Value}\".", this.token.LineNumber, this.token.Column, this.reader);
      }
    }
    private Statement StatementStartingWithPredefinedIdentifier()
    {
      // <call> ::= <id> "(" <arguments> ")"
      // <assignment statement> ::= <variable> ":=" <expr>
      // ALSO
      // <read statement> ::= "read" "(" <variable> { "," <variable> } ")"
      // <write statement> ::= "writeln" "(" <arguments> ")"
      string id = this.token.Value;
      Match(TokenType.PredefinedIdentifier);
      if (NextIs(TokenType.LeftParenthesis))
      {
        // <call> ::= <id> "(" <arguments> ")"
        // <read statement> ::= "read" "(" <variable> { "," <variable> } ")"
        // <write statement> ::= "writeln" "(" <arguments> ")"
        if (id == "writeln") return WriteStatement();
        else if (id == "read") return ReadStatement();
        else return CallWithHandledIdentifier(id);
      }
      else
      {
        // <assignment statement> ::= <variable> ":=" <expr>
        AssignmentStatement a = new AssignmentStatement();
        a.Variable = VariableWithHandledIdentifier(id);
        Match(TokenType.Assignment);
        a.Expression = Expression();
        return a;
      }
    }
    private Statement StatementStartingWithIdentifier()
    {
      // <call> ::= <id> "(" <arguments> ")"
      // <assignment statement> ::= <variable> ":=" <expr>
      string id = this.token.Value;
      // Match(TokenType.Identifier, TokenType.PredefinedIdentifier);
      Match(TokenType.Identifier);
      if (NextIs(TokenType.LeftParenthesis)) return CallWithHandledIdentifier(id);
      // AssignmentStatement
      AssignmentStatement a = new AssignmentStatement();
      a.Variable = VariableWithHandledIdentifier(id);
      Match(TokenType.Assignment);
      a.Expression = Expression();
      return a;
    }
    private ReadStatement ReadStatement()
    {
      // <read statement> ::= "read" "(" <variable> { "," <variable> } ")" 
      // Match(TokenType.PredefinedIdentifier, "read");
      Match(TokenType.LeftParenthesis);
      ReadStatement r = new ReadStatement();
      r.Variables.Add(Variable());
      while (true)
      {
        if (NextIs(TokenType.Comma))
        {
          r.Variables.Add(Variable());
        }
        else break;
      }
      Match(TokenType.RightParenthesis);
      return r;
    }
    private WriteStatement WriteStatement()
    {
      // <write statement> ::= "writeln" "(" <arguments> ")"
      // Match(TokenType.PredefinedIdentifier, "writeln");
      Match(TokenType.LeftParenthesis);
      WriteStatement w = new WriteStatement();
      if (!NextIs(TokenType.RightParenthesis)) w.Arguments = Arguments();
      Match(TokenType.RightParenthesis);
      return w;
    }
    private Statement StructuredStatement()
    {
      // <structured statement> ::= <block> | <if statement> | <while statement>
      if (NextIs(TokenType.Keyword, "begin")) return Block("StructuredStatementBlock");
      else if (NextIs(TokenType.Keyword, "if")) return IfStatement();
      else if (NextIs(TokenType.Keyword, "while")) return WhileStatement();
      else throw new SyntaxError($"Statement can not start with {this.token.Type} \"{this.token.Value}\".\nExpected Keywords: \"begin\", \"if\" or \"while\".", this.token.LineNumber, this.token.Column, this.reader);
    }
    private IfStatement IfStatement()
    {
      // <if statement> ::= "if" <Boolean expr> "then" <statement> |
      //                "if" <Boolean expr> "then" <statement> "else" <statement>
      IfStatement i = new IfStatement();
      Match(TokenType.Keyword, "if");
      i.BooleanExpression = Expression();
      Match(TokenType.Keyword, "then");
      i.ThenStatement = Statement();
      if (NextIs(TokenType.Keyword, "else"))
      {
        i.ElseStatement = Statement();
      }
      return i;
    }
    private WhileStatement WhileStatement()
    {
      // <while statement> ::= "while" <Boolean expr> "do" <statement> 
      WhileStatement w = new WhileStatement();
      Match(TokenType.Keyword, "while");
      w.BooleanExpression = Expression();
      Match(TokenType.Keyword, "do");
      w.Statement = Statement();
      return w;
    }
    private ReturnStatement ReturnStatement()
    {
      // <return statement> ::= "return" [ expr ]
      ReturnStatement r = new ReturnStatement();
      Match(TokenType.Keyword, "return");
      if (
        NextIs(TokenType.SemiColon) ||
        NextIs(TokenType.Keyword, "end") ||
        NextIs(TokenType.Keyword, "else")
        ) return r; // Empty
      r.Expression = Expression();
      return r;
    }
    private AssertStatement AssertStatement()
    {
      // <assert statement> ::= "assert" "(" <Boolean expr> ")"
      AssertStatement a = new AssertStatement();
      Match(TokenType.Keyword, "assert");
      Match(TokenType.LeftParenthesis);
      a.BooleanExpression = Expression();
      Match(TokenType.RightParenthesis);
      return a;
    }
    private Arguments Arguments()
    {
      // <arguments> ::= expr { "," expr } | <empty>
      Arguments a = new Arguments();
      if (NextIs(TokenType.RightParenthesis)) return a; // Empty
      a.Expressions.Add(Expression());
      while (true)
      {
        if (NextIs(TokenType.Comma))
        {
          Match(TokenType.Comma);
          a.Expressions.Add(Expression());
        }
        else break;
      }
      return a;
    }
    private Variable Variable()
    {
      // <variable> ::= <variable id> [ "[" <integer expr> "]" ]
      string name = this.token.Value;
      Match(TokenType.Identifier, TokenType.PredefinedIdentifier);
      return VariableWithHandledIdentifier(name);
    }
    private Variable VariableWithHandledIdentifier(string name)
    {
      // [ "[" <integer expr> "]" ]
      Variable v = new Variable();
      v.Name = name;
      if (NextIs(TokenType.LeftBracket))
      {
        Match(TokenType.LeftBracket);
        v.IntegerExpression = Expression();
        Match(TokenType.RightBracket);
      }
      return v;
    }
    private Expression Expression()
    {
      // <expr> ::= <simple expr> |
      //       <simple expr> <relational operator> <simple expr>
      SimpleExpression s = SimpleExpression();
      if (NextIs(TokenType.RelationalOperator))
      {
        BooleanExpression b = new BooleanExpression();
        b.Left = s;
        b.RelationalOperator = this.token.Value;
        Match(TokenType.RelationalOperator);
        b.Right = SimpleExpression();
        return b;
      }
      return s;
    }
    private SimpleExpression SimpleExpression()
    {
      // <simple expr> ::= [ <sign> ] <term> { <adding operator> <term> }
      SimpleExpression s = new SimpleExpression();
      if (NextIs(TokenType.AddingOperator, "+") || NextIs(TokenType.AddingOperator, "-"))
      {
        s.Sign = this.token.Value;
        Match(TokenType.AddingOperator);
      }
      s.Term = Term();
      while (true)
      {
        if (NextIs(TokenType.AddingOperator))
        {
          SimpleExpressionAddition a = new SimpleExpressionAddition();
          a.AddingOperator = this.token.Value;
          Match(TokenType.AddingOperator);
          a.Term = Term();
          s.Additions.Add(a);
        }
        else break;
      }
      return s;
    }
    private Term Term()
    {
      // <term> ::= <factor> { <multiplying operator> <factor> }
      Term t = new Term();
      t.Factor = Factor();
      while (true)
      {
        if (NextIs(TokenType.MultiplyingOperator))
        {
          TermMultiplicative m = new TermMultiplicative();
          m.MultiplyingOperator = this.token.Value;
          Match(TokenType.MultiplyingOperator);
          m.Factor = Factor();
          t.Multiplicatives.Add(m);
        }
        else break;
      }
      return t;
    }
    private Factor Factor()
    {
      // <factor> ::= <call> | <variable> | <literal> | "(" <expr> ")" | "not" <factor> | < factor> "." "size"
      // <factor> ::= (<call> | <variable> | <literal> | "(" <expr> ")" | "not" <factor>) ("." "size")*
      // <factor> ::= <call><size> | <variable><size> | <literal><size> | "(" <expr> ")" <size> | "not" <factor> <size>
      // <size> ::= "." "size" <size> | <empty>
      // Elimination of left-recursion
      if (NextIs(TokenType.Identifier) || NextIs(TokenType.PredefinedIdentifier)) return FactorsStartingWithIdentifier();
      else if (
        NextIs(TokenType.IntegerLiteral) ||
        NextIs(TokenType.StringLiteral) ||
        NextIs(TokenType.RealLiteral)
        ) return FactorsStartingWithLiteral();
      else if (NextIs(TokenType.LeftParenthesis)) return FactorsStartingWithLeftParenthesis();
      else if (NextIs(TokenType.Negation)) return FactorsStartingWithNegation();
      else throw new SyntaxError($"Expected Identifier, PredefinedIdentifier, Literal, LeftParenthesis or Keyword \"not\".\nInstead got {this.token.Type} \"{this.token.Value}\".", this.token.LineNumber, this.token.Column, this.reader);
    }
    private Factor FactorsStartingWithNegation()
    {
      // "not" <factor> | <factor> "." "size"
      NegationFactor f = new NegationFactor();
      Match(TokenType.Negation);
      f.Factor = Factor();
      HandlePossibleSize(f);
      return f;
    }
    private Factor FactorsStartingWithLeftParenthesis()
    {
      // "(" expr ")" | <factor> "." "size"
      ClosedExpression f = new ClosedExpression();
      Match(TokenType.LeftParenthesis);
      f.Expression = Expression();
      Match(TokenType.RightParenthesis);
      HandlePossibleSize(f);
      return f;
    }
    private Factor FactorsStartingWithLiteral()
    {
      // <literal> | <factor> "." "size"
      if (NextIs(TokenType.IntegerLiteral))
      {
        IntegerLiteral lit = new IntegerLiteral();
        // lit.Value = System.Int32.Parse(this.token.Value);
        lit.Value = this.token.Value;
        Match(TokenType.IntegerLiteral);
        HandlePossibleSize(lit);
        return lit;
      }
      else if (NextIs(TokenType.StringLiteral))
      {
        StringLiteral lit = new StringLiteral();
        lit.Value = this.token.Value;
        Match(TokenType.StringLiteral);
        HandlePossibleSize(lit);
        return lit;
      }
      else if (NextIs(TokenType.RealLiteral))
      {
        RealLiteral lit = new RealLiteral();
        lit.Value = this.token.Value;
        Match(TokenType.RealLiteral);
        HandlePossibleSize(lit);
        return lit;
      }
      else throw new SyntaxError($"Expected IntegerLiteral.", this.token.LineNumber, this.token.Column, this.reader);
    }
    private Factor FactorsStartingWithIdentifier()
    {
      // <call> | <variable> | <factor> "." "size"
      Factor f;
      string id = this.token.Value;
      Match(TokenType.Identifier, TokenType.PredefinedIdentifier);
      // <call> [ "." "size" ]
      if (NextIs(TokenType.LeftParenthesis)) f = CallWithHandledIdentifier(id);
      // <variable> | <factor> "." "size"
      else f = VariableWithHandledIdentifier(id);
      HandlePossibleSize(f);
      return f;
    }
    private void HandlePossibleSize(Factor f)
    {
      if (NextIs(TokenType.Dot))
      {
        f.Size = true;
        Match(TokenType.Dot);
        Match(TokenType.PredefinedIdentifier, "size");
      }
    }
    private Declaration Declaration()
    {
      // <var-declaration> ::= "var" <id> { "," <id> } ":" <type> 
      Match(TokenType.Keyword, "var");
      string id = this.token.Value;
      Match(TokenType.Identifier, TokenType.PredefinedIdentifier);
      Declaration d = new Declaration();
      d.Identifiers.Add(id);
      while (true)
      {
        if (NextIs(TokenType.Comma))
        {
          Match(TokenType.Comma);
          id = this.token.Value;
          Match(TokenType.Identifier, TokenType.PredefinedIdentifier);
          d.Identifiers.Add(id);
        }
        else break;
      }
      Match(TokenType.Colon);
      Type type = Type();
      d.Type = type;
      return d;
    }
    private Call CallWithHandledIdentifier(string id)
    {
      Call call = new Call();
      call.Name = id;
      Match(TokenType.LeftParenthesis);
      if (!NextIs(TokenType.RightParenthesis)) call.Arguments = Arguments();
      Match(TokenType.RightParenthesis);
      return call;
    }
    private void ProceduresAndFunctions(ProgramNode program)
    {
      while (true)
      {
        if (NextIs(TokenType.Keyword, "procedure")) program.AddProcedure(Procedure());
        else if (NextIs(TokenType.Keyword, "function")) program.AddFunction(Function());
        else break;
      }
    }
    private Procedure Procedure()
    {
      Match(TokenType.Keyword, "procedure");
      string name = this.token.Value;
      Match(TokenType.Identifier, TokenType.PredefinedIdentifier);
      Procedure p = new Procedure();
      p.Name = name;
      Match(TokenType.LeftParenthesis);
      p.Parameters = Parameters();
      Match(TokenType.RightParenthesis);
      Match(TokenType.SemiColon);
      p.Block = Block("ProcedureBlock");
      Match(TokenType.SemiColon);
      return p;
    }
    private Function Function()
    {
      Match(TokenType.Keyword, "function");
      string name = this.token.Value;
      Match(TokenType.Identifier, TokenType.PredefinedIdentifier);
      Function f = new Function();
      f.Name = name;
      Match(TokenType.LeftParenthesis);
      f.Parameters = Parameters();
      Match(TokenType.RightParenthesis);
      Match(TokenType.Colon);
      Type type = Type();
      f.Type = type;
      Match(TokenType.SemiColon);
      f.Block = Block("FunctionBlock");
      Match(TokenType.SemiColon);
      return f;
    }
    private List<Parameter> Parameters()
    {
      // <parameters> ::= [ "var" ] <id> ":" <type> { "," [ "var" ] <id> ":" <type> } | <empty> 
      List<Parameter> parameters = new List<Parameter>();
      bool parenthesisAllowed = true;
      // While will end in RightParenthesis or throws a Syntax error
      while (true)
      {
        if (NextIs(TokenType.RightParenthesis) && parenthesisAllowed) return parameters;
        if (NextIs(TokenType.Keyword, "var"))
        {
          parenthesisAllowed = true;
          parameters.Add(ReferenceParameter());
        }
        else
        {
          parenthesisAllowed = true;
          parameters.Add(ValueParameter());
        }
        if (NextIs(TokenType.Comma))
        {
          Match(TokenType.Comma);
          parenthesisAllowed = false;
        }
      }
    }
    private ReferenceParameter ReferenceParameter()
    {
      Match(TokenType.Keyword, "var");
      string name = this.token.Value;
      Match(TokenType.Identifier, TokenType.PredefinedIdentifier);
      Match(TokenType.Colon);
      Type type = Type();
      ReferenceParameter p = new ReferenceParameter();
      p.Name = name;
      p.Type = type;
      return p;
    }
    private ValueParameter ValueParameter()
    {
      string name = this.token.Value;
      Match(TokenType.Identifier, TokenType.PredefinedIdentifier);
      Match(TokenType.Colon);
      Type type = Type();
      ValueParameter p = new ValueParameter();
      p.Name = name;
      p.Type = type;
      return p;
    }
    private Type Type()
    {
      // <type> ::= <simple type> | <array type> 
      if (
        NextIs(TokenType.PredefinedIdentifier, "Boolean") ||
        NextIs(TokenType.PredefinedIdentifier, "integer") ||
        NextIs(TokenType.PredefinedIdentifier, "real") ||
        NextIs(TokenType.PredefinedIdentifier, "string")
        ) return SimpleType();
      else if (NextIs(TokenType.Keyword, "array")) return ArrayType();
      else throw new SyntaxError($"Type can not be {this.token.Type} {this.token.Value}.\nExpected PredefinedIdentifiers: \"Boolean\", \"integer\", \"real\", \"string\" or Keyword \"array\".", this.token.LineNumber, this.token.Column, this.reader);
    }
    private SimpleType SimpleType()
    {
      string[] allowed = { "Boolean", "integer", "real", "string" };
      string t = this.token.Value;
      Match(TokenType.PredefinedIdentifier, allowed);
      SimpleType type = new SimpleType();
      type.Type = DetermineType(t);
      return type;
    }
    private ArrayType ArrayType()
    {
      // <array type> ::= "array" "[" [<integer expr>] "]" "of" <simple type>
      ArrayType a = new ArrayType();
      Match(TokenType.Keyword, "array");
      Match(TokenType.LeftBracket);
      if (!NextIs(TokenType.RightBracket))
      {
        a.IntegerExpression = Expression();
      }
      Match(TokenType.RightBracket);
      Match(TokenType.Keyword, "of");
      a.Type = DetermineArrayType(SimpleType().Type);
      // a.Type = SimpleType().Type;
      return a;
    }
    private BuiltInType DetermineType(string t)
    {
      switch (t)
      {
        case ("string"): return BuiltInType.String;
        case ("integer"): return BuiltInType.Integer;
        case ("Boolean"): return BuiltInType.Boolean;
        case ("Real"): return BuiltInType.Real;
        default: return BuiltInType.Error;
      }
    }
    private BuiltInType DetermineArrayType(BuiltInType t)
    {
      switch (t)
      {
        case BuiltInType.String: return BuiltInType.StringArray;
        case BuiltInType.Integer: return BuiltInType.IntegerArray;
        case BuiltInType.Real: return BuiltInType.RealArray;
        case BuiltInType.Boolean: return BuiltInType.BooleanArray;
        default: return BuiltInType.Error;
      }
    }
    private bool NextIs(TokenType expectedToken, string expectedValue)
    {
      return (expectedToken == this.token.Type && expectedValue == this.token.Value);
    }
    private bool NextIs(TokenType expected)
    {
      return (expected == this.token.Type);
    }
    private bool NextIs(TokenType[] expectedTypes)
    {
      foreach (TokenType type in expectedTypes)
      {
        if (type == this.token.Type)
        {
          return true;
        }
      }
      return false;
    }
    private void Match(TokenType[] expectedTypes)
    {
      bool success = false;
      foreach (TokenType type in expectedTypes)
      {
        if (type == this.token.Type)
        {
          success = true;
          break;
        }
      }
      if (success) this.token = this.scanner.NextToken();
      else
      {
        // TODO: Put the TokenTypes into a string list
        string expectedString = "";
        throw new SyntaxError($"Expected one of the following: {expectedString}, instead got {this.token.Type} \"{this.token.Value}\".", this.token.LineNumber, this.token.Column, this.reader);
      }
    }
    private void Match(TokenType expectedToken, string[] allowed)
    {
      bool success = false;
      if (expectedToken == this.token.Type)
      {
        foreach (string a in allowed)
        {
          if (a == this.token.Value)
          {
            this.token = this.scanner.NextToken();
            success = true;
            break;
          }
        }
      }
      if (!success)
      {
        string allowedValues = "";
        for (int i = 0; i < allowed.Length; i++)
        {
          if (i == allowed.Length - 2) allowedValues += $"\"{allowed[i]}\" or ";
          else if (i < allowed.Length - 1) allowedValues += $"\"{allowed[i]}\", ";
          else allowedValues += $"\"{allowed[i]}\"";
        }
        throw new SyntaxError($"Expected {expectedToken} {allowedValues}, instead got {this.token.Type} \"{this.token.Value}\"", this.token.LineNumber, this.token.Column, this.reader);
      }
    }
    private void Match(TokenType expectedToken, string expectedValue)
    {
      if (expectedToken == this.token.Type && expectedValue == this.token.Value)
      {
        this.token = this.scanner.NextToken();
      }
      else throw new SyntaxError($"Expected {expectedToken} \"{expectedValue}\", instead got {this.token.Type} \"{this.token.Value}\"", this.token.LineNumber, this.token.Column, this.reader);
    }
    private void Match(TokenType expected)
    {
      if (expected == this.token.Type) this.token = this.scanner.NextToken();
      else throw new SyntaxError($"Expected {expected}, instead got {this.token.Type} \"{this.token.Value}\".", this.token.LineNumber, this.token.Column, this.reader);
    }
    private void Match(TokenType expected1, TokenType expected2)
    {
      // Used for Identifiers. Either Identifier of PredefinedIdentifier
      if (expected1 == this.token.Type || expected2 == this.token.Type) this.token = this.scanner.NextToken();
      else throw new SyntaxError($"Expected {expected1} or {expected2}, instead got {this.token.Type}", this.token.LineNumber, this.token.Column, this.reader);
    }
  }
}
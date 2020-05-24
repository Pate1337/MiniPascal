using System.Collections.Generic;
using IO;
using Nodes;

namespace Semantic
{
  public class PrintVisitor : Visitor
  {
    private IOHandler io;
    private int depth;
    private string IncludeOnNextNewline;
    public PrintVisitor(IOHandler io)
    {
      this.io = io;
      this.depth = 0;
      this.IncludeOnNextNewline = "";
    }
    private void EnterNode(Node n)
    {
      this.io.Write($"\n{HandleDepth(this.depth)}{this.IncludeOnNextNewline}(<{n.Style}>: ");
      this.depth++;
      this.IncludeOnNextNewline = "";
    }
    private void IncludeTextOnNextNewline(string text)
    {
      this.IncludeOnNextNewline = text;
    }
    private void ExitNode()
    {
      this.io.Write(")");
      this.depth--;
    }
    public void VisitProgram(ProgramNode p)
    {
      this.io.Write($"(<{p.Style}>: Name: {p.Name},");
      this.depth++;
      if (p.Procedures.Count > 0)
      {
        IncludeTextOnNextNewline("Procedures: [");
        foreach (Procedure procedure in p.Procedures) procedure.Visit(this);
        this.io.Write("],");
      }
      if (p.Functions.Count > 0)
      {
        IncludeTextOnNextNewline("Functions: [");
        foreach (Function f in p.Functions) f.Visit(this);
        this.io.Write("],");
      }
      IncludeTextOnNextNewline("Block: ");
      p.Block.Visit(this);
      this.depth--;
      this.io.Write(")\n");
    }
    public void VisitProceduresAndFunctions(ProgramNode p){}
    public void VisitProcedure(Procedure p)
    {
      EnterNode(p);
      this.io.Write($"Name: {p.Name},");
      if (p.Parameters.Count > 0)
      {
        IncludeTextOnNextNewline("Parameters: [");
        foreach (Parameter param in p.Parameters) param.Visit(this);
        this.io.Write("],");
      }
      IncludeTextOnNextNewline("Block:");
      p.Block.Visit(this);
      ExitNode();
    }
    public void VisitFunction(Function f)
    {
      EnterNode(f);
      this.io.Write($"Name: {f.Name},");
      if (f.Parameters.Count > 0)
      {
        IncludeTextOnNextNewline("Parameters: [");
        foreach (Parameter param in f.Parameters) param.Visit(this);
        this.io.Write("],");
      }
      IncludeTextOnNextNewline("Type: ");
      f.Type.Visit(this);
      this.io.Write(",");
      IncludeTextOnNextNewline("Block: ");
      f.Block.Visit(this);
      ExitNode();
    }
    public void VisitBlock(Block b, bool needsToReturnValue)
    {
      EnterNode(b);
      if (b.statements.Count > 0)
      {
        IncludeTextOnNextNewline("Statements: [");
        foreach (Statement stmt in b.statements) stmt.Visit(this);
        this.io.Write("]");
      }
      ExitNode();
    }
    public BuiltInType VisitCall(Call c)
    {
      EnterNode(c);
      this.io.Write($"Name: {c.Name}, Size: {c.Size}");
      if (c.Arguments != null)
      {
        this.io.Write(",");
        IncludeTextOnNextNewline("Arguments: [");
        c.Arguments.Visit(this);
        this.io.Write("]");
      }
      ExitNode();
      return BuiltInType.Error;
    }
    public void VisitAssertStatement(AssertStatement s)
    {
      EnterNode(s);
      this.io.Write("BooleanExpression:");
      s.BooleanExpression.Visit(this);
      ExitNode();
    }
    public void VisitAssignmentStatement(AssignmentStatement s)
    {
      EnterNode(s);
      this.io.Write("Variable:");
      s.Variable.Visit(this);
      this.io.Write(", Expression:");
      s.Expression.Visit(this);
      ExitNode();
    }
    public void VisitDeclaration(Declaration s)
    {
      EnterNode(s);
      this.io.Write("Identifiers: [");
      int i = 1;
      foreach (Lexer.Token t in s.Identifiers)
      {
        this.io.Write($"{t.Value}");
        if (i < s.Identifiers.Count) this.io.Write(", ");
        i++;
      }
      this.io.Write("], Type:");
      s.Type.Visit(this);
      ExitNode();
    }
    public void VisitIfStatement(IfStatement s)
    {
      EnterNode(s);
      this.io.Write("BooleanExpression:");
      s.BooleanExpression.Visit(this);
      this.io.Write(", ThenStatement:");
      s.ThenStatement.Visit(this);
      if (s.ElseStatement != null)
      {
        this.io.Write(", ElseStatement:");
        s.ElseStatement.Visit(this);
      }
      ExitNode();
    }
    public void VisitReadStatement(ReadStatement s)
    {
      EnterNode(s);
      this.io.Write("Variables: [");
      foreach (Variable v in s.Variables) v.Visit(this);
      this.io.Write("]");
      ExitNode();
    }
    public void VisitReturnStatement(ReturnStatement s)
    {
      EnterNode(s);
      if (s.Expression != null)
      {
        this.io.Write("Expression:");
        s.Expression.Visit(this);
      }
      ExitNode();
    }
    public void VisitWhileStatement(WhileStatement s)
    {
      EnterNode(s);
      this.io.Write("BooleanExpression:");
      s.BooleanExpression.Visit(this);
      this.io.Write(", Statement:");
      s.Statement.Visit(this);
      ExitNode();
    }
    public void VisitWriteStatement(WriteStatement s)
    {
      EnterNode(s);
      if (s.Arguments != null)
      {
        this.io.Write("Arguments:");
        s.Arguments.Visit(this);
      }
      ExitNode();
    }
    public List<BuiltInType> VisitArguments(Arguments a)
    {
      EnterNode(a);
      if (a.Expressions.Count > 0)
      {
        this.io.Write("Expressions: [");
        foreach (Expression e in a.Expressions) e.Visit(this);
        this.io.Write("]");
      }
      ExitNode();
      return new List<BuiltInType>();
    }
    public SymbolTableEntry VisitReferenceParameter(ReferenceParameter rp)
    {
      EnterNode(rp);
      this.io.Write($"Name: {rp.Name}, Type:");
      rp.Type.Visit(this);
      ExitNode();
      return new SymbolTableEntry("", BuiltInType.Error);
    }
    public SymbolTableEntry VisitValueParameter(ValueParameter vp)
    {
      EnterNode(vp);
      this.io.Write($"Name: {vp.Name}, Type:");
      vp.Type.Visit(this);
      ExitNode();
      return new SymbolTableEntry("", BuiltInType.Error);
    }
    public BuiltInType VisitSimpleType(SimpleType t)
    {
      EnterNode(t);
      this.io.Write($"Type: {t.Type}");
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitArrayType(ArrayType t)
    {
      EnterNode(t);
      this.io.Write($"Type: {t.Type}");
      if (t.IntegerExpression != null)
      {
        this.io.Write(", IntegerExpression:");
        t.IntegerExpression.Visit(this);
      }
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitSimpleExpression(SimpleExpression e)
    {
      EnterNode(e);
      if (e.Sign != null) this.io.Write($"Sign: {e.Sign}, ");
      if (e.Location != null) this.io.Write($"line: {e.Location.Line}, column: {e.Location.Column}, ");
      this.io.Write("Term:");
      e.Term.Visit(this);
      if (e.Additions.Count > 0)
      {
        this.io.Write(", Additions: [");
        foreach (SimpleExpressionAddition a in e.Additions) a.Visit(this);
        this.io.Write("]");
      }
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitBooleanExpression(BooleanExpression e)
    {
      EnterNode(e);
      this.io.Write($"RelationalOperator: {e.RelationalOperator}, Left:");
      e.Left.Visit(this);
      this.io.Write(", Right:");
      e.Right.Visit(this);
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitClosedExpression(ClosedExpression e)
    {
      EnterNode(e);
      this.io.Write($"Size: {e.Size}, Expression:");
      e.Expression.Visit(this);
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitSimpleExpressionAddition(SimpleExpressionAddition e)
    {
      EnterNode(e);
      this.io.Write($"AddingOperator: {e.AddingOperator}, Term:");
      e.Term.Visit(this);
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitTerm(Term t)
    {
      EnterNode(t);
      this.io.Write("Factor:");
      t.Factor.Visit(this);
      if (t.Multiplicatives.Count > 0)
      {
        this.io.Write(", Multiplicatives: [");
        foreach (TermMultiplicative m in t.Multiplicatives) m.Visit(this);
        this.io.Write("]");
      }
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitTermMultiplicative(TermMultiplicative t)
    {
      EnterNode(t);
      this.io.Write($"MultiplyingOperator: {t.MultiplyingOperator}, Factor:");
      t.Factor.Visit(this);
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitIntegerLiteral(IntegerLiteral l)
    {
      EnterNode(l);
      this.io.Write($"Size: {l.Size}, Value: {l.Value}");
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitStringLiteral(StringLiteral l)
    {
      EnterNode(l);
      this.io.Write($"Size: {l.Size}, Value: {l.Value}");
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitRealLiteral(RealLiteral l)
    {
      EnterNode(l);
      this.io.Write($"Size: {l.Size}, Value: {l.Value}");
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitNegationFactor(NegationFactor f)
    {
      EnterNode(f);
      this.io.Write($"Size: {f.Size}, Factor:");
      f.Factor.Visit(this);
      ExitNode();
      return BuiltInType.Error;
    }
    public BuiltInType VisitVariable(Variable v)
    {
      EnterNode(v);
      this.io.Write($"Name: {v.Name}, Size: {v.Size}");
      if (v.IntegerExpression != null)
      {
        this.io.Write(", IntegerExpression:");
        v.IntegerExpression.Visit(this);
      }
      ExitNode();
      return BuiltInType.Error;
    }
    private string HandleDepth(int depth)
    {
      string spaces = "";
      for (int i = 0; i < depth; i++) spaces += " ";
      return spaces;
    }
  }
}
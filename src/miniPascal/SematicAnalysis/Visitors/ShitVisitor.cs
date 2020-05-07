using System.Collections.Generic;
// using Errors;
using Nodes;
using FileHandler;
using IO;

namespace Semantic
{
  public class ShitVisitor : Visitor
  {
    private bool writeOperator;
    private bool lengthOfExpression;
    private bool toString;
    private IOHandler io;
    private Reader reader;
    private FileWriter writer;
    public ShitVisitor(IOHandler io, Reader reader, FileWriter writer)
    {
      this.writeOperator = true;
      this.lengthOfExpression = false;
      this.toString = false;
      this.io = io;
      this.reader = reader;
      this.writer = writer;
    }
    public void VisitProgram(ProgramNode p)
    {
      // foreach (Procedure procedure in p.Procedures) procedure.Visit(this);
      // foreach (Function f in p.Functions) f.Visit(this);
      p.Block.Visit(this);
    }
    public void VisitProcedure(Procedure p){}
    public void VisitFunction(Function f){}
    public SymbolTableEntry VisitReferenceParameter(ReferenceParameter rp)
    {
      return new SymbolTableEntry("", BuiltInType.Error);
    }
    public SymbolTableEntry VisitValueParameter(ValueParameter vp)
    {
      return new SymbolTableEntry("", BuiltInType.Error);
    }
    public BuiltInType VisitArrayType(ArrayType t)
    {
      return BuiltInType.Error;
    }
    public BuiltInType VisitSimpleType(SimpleType t)
    {
      return t.Type;
    }
    private void WriteLengthOfTerm(Term t)
    {
      switch(t.Type)
      {
        case BuiltInType.String:
          this.writer.Write("strlen(");
          t.Visit(this);
          this.writer.Write(")");
          break;
        case BuiltInType.Integer:
          this.writer.Write("floor(log10(abs(");
          t.Visit(this);
          this.writer.Write(")))+1");
          break;
        default: break;
      }
    }
    private string GetCFormat(BuiltInType t)
    {
      switch(t)
      {
        case BuiltInType.String: return "%s";
        case BuiltInType.Integer: return "%d";
        default: return "";
      }
    }
    public BuiltInType VisitSimpleExpression(SimpleExpression e)
    {
      /*
      public string Style { get; set; }
      public string Sign { get; set; }
      public Term Term { get; set; }
      public List<SimpleExpressionAddition> Additions { get; set; }
      // Location of possible Sign
      public Location Location { get; set; }
      public BuiltInType Type { get; set; }
      */
      if (this.lengthOfExpression)
      {
        // Switched to true in StoreStringExpressionToVariable()
        // Create the strlen(a)+log10(b)+1+1, "%s%d", a, b in VisitSimpleExpression
        WriteLengthOfTerm(e.Term);
        string formats = GetCFormat(e.Term.Type);
        foreach(SimpleExpressionAddition a in e.Additions)
        {
          this.writer.Write("+");
          WriteLengthOfTerm(a.Term);
          formats += GetCFormat(a.Term.Type);
        }
        this.writer.Write($"+1,\"{formats}\",");
        this.lengthOfExpression = false;
        // Now print the terms as they were (without lengthOfExpression)
        e.Term.Visit(this);
        this.writeOperator = false;
        foreach(SimpleExpressionAddition a in e.Additions)
        {
          this.writer.Write(",");
          a.Visit(this);
        }
        this.writeOperator = true;
        return BuiltInType.Error;
      }
      if (e.Sign != null) this.writer.Write(e.Sign);
      e.Term.Visit(this);
      foreach(SimpleExpressionAddition a in e.Additions) a.Visit(this);
      return BuiltInType.Error;
    }
    public BuiltInType VisitBooleanExpression(BooleanExpression e)
    {
      return BuiltInType.Error;
    }
    public BuiltInType VisitClosedExpression(ClosedExpression e)
    {
      return BuiltInType.Error;
    }
    public BuiltInType VisitSimpleExpressionAddition(SimpleExpressionAddition e)
    {
      /*
      public string AddingOperator { get; set; }
      public Term Term { get; set; }
      public string Style { get; set; }
      public Location Location { get; set; }
      */
      if (this.writeOperator)
      {
        // TODO: Handle "or". Do not write that ever.
        this.writer.Write(e.AddingOperator);
      }
      e.Term.Visit(this);
      return BuiltInType.Error;
    }
    public BuiltInType VisitTerm(Term t)
    {
      /*
      public string Style { get; set; }
      public Factor Factor { get; set; }
      public List<TermMultiplicative> Multiplicatives { get; set; }
      */
      t.Factor.Visit(this);
      return BuiltInType.Error;
    }
    public BuiltInType VisitTermMultiplicative(TermMultiplicative t)
    {
      return BuiltInType.Error;
    }
    public BuiltInType VisitIntegerLiteral(IntegerLiteral l)
    {
      this.writer.Write(l.Value);
      return BuiltInType.Error;
    }
    public BuiltInType VisitStringLiteral(StringLiteral l)
    {
      /*
      public string Style { get; set; }
      public bool Size { get; set; }
      public string Value { get; set; }
      public Location SizeLocation { get; set; }
      public Location Location { get; set; }
      */
      string val = l.Value;
      // if (this.removeSurroundingQuotes) val = RemoveSurroundingQuotes(l.Value);
      this.writer.Write(val);
      return BuiltInType.Error;
    }
    public BuiltInType VisitRealLiteral(RealLiteral l)
    {
      return BuiltInType.Error;
    }
    public BuiltInType VisitNegationFactor(NegationFactor f)
    {
      return BuiltInType.Error;
    }
    public BuiltInType VisitVariable(Variable v)
    {
      /*
      public string Style { get; set; }
      public string Name { get; set; }
      public bool Size { get; set; }
      // If IntegerExpression is set, means x[IntegerExpression]
      public Expression IntegerExpression { get; set; }
      public Location SizeLocation { get; set; }
      // Location of Name
      public Location Location { get; set; }
      */
      this.writer.Write(v.Name);
      if (v.IntegerExpression != null)
      {
        this.writer.Write("[");
        v.IntegerExpression.Visit(this);
        this.writer.Write("]");
      }
      if (v.Size)
      {
        // Handle the sizeof
      }
      return BuiltInType.Error;
    }
    public BuiltInType VisitCall(Call c)
    {
      return BuiltInType.Error;
    }
    public List<BuiltInType> VisitArguments(Arguments a)
    {
      /*
      public string Style { get; set; }
      public List<Expression> Expressions { get; set; }
      */
      if (this.toString)
      {
        int i = 1;
        foreach(Expression e in a.Expressions)
        {
          if (i == a.Expressions.Count) PrintExpression(e, true); // true indicates last argument
          else PrintExpression(e, false);
          i++;
        }
      }
      else
      {
        int i = 1;
        foreach(Expression e in a.Expressions)
        {
          e.Visit(this);
          if (i < a.Expressions.Count) this.writer.Write(",");
          i++;
        }
      }
      return new List<BuiltInType>();
    }
    public void VisitBlock(Block b, bool needsToReturnValue)
    {
      foreach (Statement stmt in b.statements) stmt.Visit(this);
    }
    public void VisitAssertStatement(AssertStatement s){}
    private void StoreStringExpressionToVariable(Variable v, Expression e)
    {
      // snprintf(a, strlen(a)+1+(log10(b) + 1), "%s%d", a, b)
      this.writer.Write("snprintf(");
      v.Visit(this); // Prints the variable a or a[7]
      this.writer.Write(",");
      // Expression is a + b where a is string and b is integer
      // Create the strlen(a)+log10(b)+1+1, "%s%d", a, b in VisitSimpleExpression
      this.lengthOfExpression = true; // Affects SimpleExpression and BooleanExpression
      e.Visit(this);
      // Switched back to false in SimpleExpression
      this.writer.Write(")");
    }
    public void VisitAssignmentStatement(AssignmentStatement s)
    {
      // TODO: Need to know the type of expression (Or variable or both)
      if (s.Expression.Type == BuiltInType.String)
      {
        // snprintf(a, strlen(a)+1+(log10(b) + 1), "%s%d", a, b)
        StoreStringExpressionToVariable(s.Variable, s.Expression);
        /*this.writer.Write("snprintf(");
        s.Variable.Visit(this);
        this.writer.Write(",");
        this.lengthOfExpression = true;
        s.Expression.Visit(this);
        this.writer.Write("+1,"); // Null

        s.Variable.Visit(this);
        this.writer.Write("=");

        this.writer.Write("malloc(strlen(");
        s.Expression.Visit(this);
        this.writer.Write(")+1);\n");
        this.writer.Write($"strcpy(");
        s.Variable.Visit(this);
        this.writer.Write(",");
        s.Expression.Visit(this);
        this.writer.Write(")");*/
      }
      else
      {
        s.Variable.Visit(this);
        this.writer.Write("=");
        // Integers for now
        s.Expression.Visit(this);
      }
      this.writer.Write(";\n");
    }
    public void VisitDeclaration(Declaration s)
    {
      BuiltInType type = s.Type.Visit(this);
      // TODO: Add a TypeInC attribute to Nodes in AST
      string typeInC = ConvertTypeToC(type);
      foreach (Lexer.Token t in s.Identifiers)
      {
        this.writer.WriteLine($"{typeInC} {t.Value};");
      }
    }
    public void VisitIfStatement(IfStatement s){}
    public void VisitReadStatement(ReadStatement s){}
    public void VisitReturnStatement(ReturnStatement s){}
    public void VisitWhileStatement(WhileStatement s){}
    public void VisitWriteStatement(WriteStatement s)
    {
      /*
      public string Style { get; set; }
      public Arguments Arguments { get; set; }
      public Location Location { get; set; }
      */
      this.toString = true;
      s.Arguments.Visit(this);
      this.toString = false;
    }

    private void PrintExpression(Expression e, bool last)
    {
      this.writer.Write("printf(");
      switch(e.Type)
      {
        case BuiltInType.String:
          e.Visit(this);
          break;
        case BuiltInType.Integer:
          this.writer.Write("\"%d\",");
          e.Visit(this);
          break;
        default: break;
      }
      this.writer.Write(");\n");
      // Print newline char
      if (last) this.writer.Write("printf(\"\\n\");\n");
    }
    private string ConvertTypeToC(BuiltInType type)
    {
      /*
      Integer,
      String,
      Boolean,
      Real,
      IntegerArray,
      StringArray,
      BooleanArray,
      RealArray,
      Error,
      Void, // For Procedures
      None // Indicates that MainBlock can not return
      */
      switch(type)
      {
        case BuiltInType.String: return "char*";
        case BuiltInType.Integer:
        case BuiltInType.Boolean: return "int"; // 0 or 1 if Boolean
        default: return "";
      }
    }
    private string RemoveSurroundingQuotes(string value)
    {
      if (value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"') return value.Substring(1, value.Length - 2);
      return value;
    }
  }
}
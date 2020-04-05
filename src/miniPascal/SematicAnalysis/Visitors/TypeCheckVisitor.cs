using System.Collections.Generic;
using Errors;
using Nodes;

namespace Semantic
{
  public class TypeCheckVisitor : Visitor
  {
    private List<Error> Errors;
    private SymbolTable Table;
    private string ErrorMessage;

    public TypeCheckVisitor()
    {
      this.Errors = new List<Error>();
      this.Table = new SymbolTable();
      this.ErrorMessage = "";
    }
    public void VisitProgram(ProgramNode p)
    {
      /*
      public string Style { get; set; }
      public string Name { get; set; }
      public List<Procedure> Procedures {get; set; }
      public List<Function> Functions { get; set; }
      public Block Block { get; set; }
      */
      foreach (Procedure procedure in p.Procedures) procedure.Visit(this);
      foreach (Function f in p.Functions) f.Visit(this);
      p.Block.Visit(this);
    }
    public void VisitBlock(Block b)
    {
      /*
      public string Style { get; set; }
      public List<Statement> statements;
      */
      foreach (Statement stmt in b.statements) stmt.Visit(this);
    }
    public void VisitDeclaration(Declaration s)
    {
      /*
      public string Style { get; set; }
      public List<string> Identifiers { get; set; }
      public Type Type { get; set; }
      */
      BuiltInType type = s.Type.Visit(this);
      if (type != BuiltInType.Error) 
      {
        foreach (string id in s.Identifiers)
        {
          this.Table.AddEntry(id, type);
        }
      }
      // else there has not been an IntegerExpression inside []. No need to error again.
      this.Table.PrintTable();
    }
    public void VisitProcedure(Procedure p){}
    public void VisitFunction(Function f){}
    public void VisitReferenceParameter(ReferenceParameter rp){}
    public void VisitValueParameter(ValueParameter vp){}
    public BuiltInType VisitArrayType(ArrayType t)
    {
      /*
      public string Style { get; set; }
      public BuiltInType Type { get; set; }
      public Expression IntegerExpression { get; set; } 
      */
      BuiltInType exprType = t.IntegerExpression.Visit(this);
      if (exprType == BuiltInType.Error) return BuiltInType.Error;
      if (exprType != BuiltInType.Integer)
      {
        System.Console.WriteLine($"Expected Integer expression, instead got {exprType}");
        return BuiltInType.Error;
      }
      return t.Type;
    }
    public BuiltInType VisitSimpleType(SimpleType t)
    {
      /*
      public string Style { get; set; }
      public BuiltInType Type { get; set; }
      */
      return t.Type;
    }
    public BuiltInType VisitSimpleExpression(SimpleExpression e)
    {
      /*
      public string Style { get; set; }
      public string Sign { get; set; }
      public Term Term { get; set; }
      public List<SimpleExpressionAddition> Additions { get; set; }
      */
      BuiltInType termType = e.Term.Visit(this);
      if (termType == BuiltInType.Error) return BuiltInType.Error;
      if (e.Sign == "-" && termType != BuiltInType.Integer && termType != BuiltInType.Real)
      {
        System.Console.WriteLine($"Can not have - in front of {termType}");
        return BuiltInType.Error;
      }
      foreach (SimpleExpressionAddition a in e.Additions)
      {
        BuiltInType additionType = a.Visit(this);
        string op = a.AddingOperator;
        if (additionType == BuiltInType.Error)
        {
          this.ErrorMessage = $"Can not do operation \"{op}\" between {termType} and {this.ErrorMessage}";
          System.Console.WriteLine(this.ErrorMessage);
          this.ErrorMessage = "";
          return BuiltInType.Error;
        }
        BuiltInType temp = termType;
        termType = HandleAdditionOperation(op, termType, additionType);
        if (termType == BuiltInType.Error)
        {
          System.Console.WriteLine($"Can not do operation \"{op}\" between {temp} and {additionType}");
          return BuiltInType.Error;
        }
      }
      return termType;
    }
    public BuiltInType VisitBooleanExpression(BooleanExpression e)
    {
      /*
      public string Style { get; set; }
      public SimpleExpression Left { get; set; }
      public string RelationalOperator { get; set; }
      public SimpleExpression Right { get; set; }
      */
      BuiltInType leftType = e.Left.Visit(this);
      BuiltInType rightType = e.Right.Visit(this);
      if (leftType == BuiltInType.Error || rightType == BuiltInType.Error) return BuiltInType.Error;
      if (leftType != rightType)
      {
        System.Console.WriteLine($"Can not do operation \"{e.RelationalOperator}\" between {leftType} and {rightType}");
        return BuiltInType.Error;
      }
      // <relational operator> ::= "=" | "<>" | "<" | "<=" | ">=" | ">"
      return BuiltInType.Boolean;
    }
    public BuiltInType VisitClosedExpression(ClosedExpression e)
    {
      /*
      public string Style { get; set; }
      public bool Size { get; set; }
      public Expression Expression { get; set; }
      */
      BuiltInType exprType = e.Expression.Visit(this);
      if (exprType == BuiltInType.Error) return BuiltInType.Error;
      if (e.Size)
      {
        if (IsArray(exprType)) return BuiltInType.Integer;
        // TODO: If String size possible, add here
        System.Console.WriteLine($"Can not get size of {exprType}");
        return BuiltInType.Error;
      }
      return exprType;
    }
    public BuiltInType VisitSimpleExpressionAddition(SimpleExpressionAddition e)
    {
      /*
      public string AddingOperator { get; set; }
      public Term Term { get; set; }
      public string Style { get; set; }
      */
      BuiltInType termType = e.Term.Visit(this);
      if (termType == BuiltInType.Error) return BuiltInType.Error;
      // <adding operator> ::= "+" | "-" | "or"
      BuiltInType addType = HandleAdditionOperation(e.AddingOperator, termType);
      if (addType == BuiltInType.Error)
      {
        this.ErrorMessage = $"{termType}";
        return BuiltInType.Error;
      }
      return addType;
    }
    public BuiltInType VisitTerm(Term t)
    {
      /*
      public string Style { get; set; }
      public Factor Factor { get; set; }
      public List<TermMultiplicative> Multiplicatives { get; set; }
      */
      BuiltInType factorType = t.Factor.Visit(this);
      if (factorType == BuiltInType.Error) return BuiltInType.Error;
      foreach (TermMultiplicative m in t.Multiplicatives)
      {
        BuiltInType mulType = m.Visit(this);
        string op = m.MultiplyingOperator;
        if (mulType == BuiltInType.Error)
        {
          this.ErrorMessage = $"Can not do operation \"{op}\" between {factorType} and {this.ErrorMessage}";
          System.Console.WriteLine(this.ErrorMessage);
          this.ErrorMessage = "";
          return BuiltInType.Error;
        }
        BuiltInType temp = factorType;
        factorType = HandleMultiplyingOperation(op, factorType, mulType);
        if (factorType == BuiltInType.Error)
        {
          System.Console.WriteLine($"Can not do operation \"{op}\" between {temp} and {mulType}");
          return BuiltInType.Error;
        }
      }
      return factorType;
    }
    public BuiltInType VisitTermMultiplicative(TermMultiplicative t)
    {
      /*
      public string MultiplyingOperator { get; set; }
      public Factor Factor { get; set; }
      public string Style { get; set; }
      */
      // <multiplying operator> ::= "*" | "/" | "%" | "and"
      BuiltInType factorType = t.Factor.Visit(this);
      if (factorType == BuiltInType.Error) return BuiltInType.Error;
      string op = t.MultiplyingOperator;
      BuiltInType mulType = HandleMultiplyingOperation(op, factorType);
      if (mulType == BuiltInType.Error)
      {
        this.ErrorMessage = $"{factorType}";
        return BuiltInType.Error;
      }
      return mulType;
    }
    public BuiltInType VisitIntegerLiteral(IntegerLiteral l)
    {
      /*
      public string Style { get; set; }
      public bool Size { get; set; }
      public string Value { get; set; }
      */
      if (l.Size)
      {
        System.Console.WriteLine("Can not call size method for IntegerLiteral");
        return BuiltInType.Error;
      }
      return BuiltInType.Integer;
    }
    public BuiltInType VisitStringLiteral(StringLiteral l)
    {
      if (l.Size)
      {
        System.Console.WriteLine("Can not call size method for StringLiteral");
        return BuiltInType.Error;
      }
      return BuiltInType.String;
    }
    public BuiltInType VisitRealLiteral(RealLiteral l)
    {
      if (l.Size)
      {
        System.Console.WriteLine("Can not call size method for RealLiteral");
        return BuiltInType.Error;
      }
      return BuiltInType.Real;
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
      */
      return BuiltInType.Error;
    }
    public BuiltInType VisitCall(Call c)
    {
      return BuiltInType.Error;
    }
    public void VisitArguments(Arguments a)
    {
      /*
      public string Style { get; set; }
      public List<Expression> Expressions { get; set; }
      */
      foreach (Expression e in a.Expressions)
      {
        e.Visit(this);
        // No need to give error message here
      }
    }
    public void VisitAssertStatement(AssertStatement s){}
    public void VisitAssignmentStatement(AssignmentStatement s){}
    public void VisitIfStatement(IfStatement s){}
    public void VisitReadStatement(ReadStatement s){}
    public void VisitReturnStatement(ReturnStatement s){}
    public void VisitWhileStatement(WhileStatement s){}
    public void VisitWriteStatement(WriteStatement s)
    {
      /*
      public string Style { get; set; }
      public Arguments Arguments { get; set; }
      */
      s.Arguments.Visit(this);
    }
    private bool IsArray(BuiltInType type)
    {
      return (
        type == BuiltInType.IntegerArray ||
        type == BuiltInType.StringArray ||
        type == BuiltInType.RealArray ||
        type == BuiltInType.BooleanArray
      );
    }
    private BuiltInType HandleMultiplyingOperation(string op, BuiltInType t)
    {
      // <multiplying operator> ::= "*" | "/" | "%" | "and" 
      switch (op)
      {
        case "*":
        case "/":
          if (t != BuiltInType.Integer && t != BuiltInType.Real) return BuiltInType.Error;
          return t;
        case "%":
          if (t != BuiltInType.Integer) return BuiltInType.Error;
          return t;
        case "and": return t;
        default: return BuiltInType.Error;
      }
    }
    private BuiltInType HandleAdditionOperation(string op, BuiltInType t)
    {
      switch (op)
      {
        case "-":
          if (t != BuiltInType.Integer && t != BuiltInType.Real) return BuiltInType.Error;
          return t;
        case "or":
        case "+": return t;
        default: return BuiltInType.Error;
      }
    }
    private BuiltInType HandleMultiplyingOperation(string op, BuiltInType t1, BuiltInType t2)
    {
      switch (op)
      {
        case "*": return HandleMultiplicationOperation(t1, t2);
        case "/": return HandleDivisionOperation(t1, t2);
        case "%": return HandleModuloOperation(t1, t2);
        case "and": return HandleAndOperation(t1, t2);
        default: return BuiltInType.Error;
      }
    }
    private BuiltInType HandleAdditionOperation(string op, BuiltInType t1, BuiltInType t2)
    {
      switch (op)
      {
        case "+": return HandlePlusOperation(t1, t2);
        case "-": return HandleMinusOperation(t1, t2);
        case "or": return HandleOrOperation(t1, t2);
        default: return BuiltInType.Error;
      }
    }
    private BuiltInType HandleMultiplicationOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 != t2) return HandleDifferentTypeMultiplicationOperation(t1, t2);
      if (t1 == BuiltInType.Integer || t1 == BuiltInType.Real) return t1;
      return BuiltInType.Error;
    }
    private BuiltInType HandleDivisionOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 != t2) return HandleDifferentTypeDivisionOperation(t1, t2);
      if (t1 == BuiltInType.Integer || t1 == BuiltInType.Real) return t1;
      return BuiltInType.Error;
    }
    private BuiltInType HandleModuloOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 != t2) return BuiltInType.Error;
      if (t1 == BuiltInType.Integer) return t1;
      return BuiltInType.Error;
    }
    private BuiltInType HandleAndOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 != t2) return HandleDifferentTypeAndOperation(t1, t2);
      if (t1 == BuiltInType.Boolean) return t1;
      return BuiltInType.Error;
    }
    private BuiltInType HandlePlusOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 != t2) return HandleDifferentTypePlusOperation(t1, t2);
      if (t1 != BuiltInType.Boolean) return t1;
      return BuiltInType.Error;
    }
    private BuiltInType HandleMinusOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 != t2) return HandleDifferentTypeMinusOperation(t1, t2);
      if (t1 == BuiltInType.Integer || t1 == BuiltInType.Real) return t1;
      return BuiltInType.Error;
    }
    private BuiltInType HandleOrOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 != t2) return HandleDifferentTypeOrOperation(t1, t2);
      if (t1 == BuiltInType.Boolean) return t1;
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeMultiplicationOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 == BuiltInType.Integer && t2 == BuiltInType.Real) return BuiltInType.Real;
      if (t1 == BuiltInType.Real && t2 == BuiltInType.Integer) return BuiltInType.Real;
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeDivisionOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 == BuiltInType.Integer && t2 == BuiltInType.Real) return BuiltInType.Real;
      if (t1 == BuiltInType.Real && t2 == BuiltInType.Integer) return BuiltInType.Real;
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeAndOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 == BuiltInType.Boolean) return t2; // true and "ok" -> String
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypePlusOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 == BuiltInType.String || t2 == BuiltInType.String) return BuiltInType.String; // Turns Type to String
      // TODO: Handle Real + Integer and Integer + Real
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeMinusOperation(BuiltInType t1, BuiltInType t2)
    {
      // TODO: Handle Integer - Real and Real - Integer
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeOrOperation(BuiltInType t1, BuiltInType t2)
    {
      if (t1 == BuiltInType.Boolean) return t2; // true or "ok" -> String
      return BuiltInType.Error;
    }
  }
}
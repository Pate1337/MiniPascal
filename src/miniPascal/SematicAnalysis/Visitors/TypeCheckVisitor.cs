using System.Collections.Generic;
using Errors;
using Nodes;
using FileHandler;
using IO;

namespace Semantic
{
  public class TypeCheckVisitor : Visitor
  {
    private IOHandler io;
    private List<Error> Errors;
    private SymbolTable Table;
    private Reader reader;

    public TypeCheckVisitor(IOHandler io, Reader reader)
    {
      this.io = io;
      this.Errors = new List<Error>();
      this.Table = new SymbolTable();
      this.reader = reader;
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
      if (t.IntegerExpression == null) return t.Type;
      BuiltInType exprType = t.IntegerExpression.Visit(this);
      if (exprType == BuiltInType.Error) return BuiltInType.Error;
      if (exprType != BuiltInType.Integer)
      {
        new Error($"Expected \"[\" Integer \"]\", instead got \"[\" {exprType} \"]\".", t.Location, this.reader).Print(this.io);
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
      if (e.Sign != null && termType != BuiltInType.Integer && termType != BuiltInType.Real)
      {
        // this.io.WriteLine(new Error($"Can not have \"{e.Sign}\" in front of {termType}.", e.Location, this.reader).ToString());
        new Error($"Can not have \"{e.Sign}\" in front of {termType}.", e.Location, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      foreach (SimpleExpressionAddition a in e.Additions)
      {
        BuiltInType additionType = a.Visit(this);
        if (additionType == BuiltInType.Error) return BuiltInType.Error;
        termType = HandleAdditionOperation(a.AddingOperator, termType, additionType, a.Location);
        if (termType == BuiltInType.Error) return BuiltInType.Error;
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
        new OperationError(e.RelationalOperator, leftType, rightType, e.Location, this.reader).Print(this.io);
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
        new Error($"Can not get size of {exprType}.", e.SizeLocation, this.reader).Print(this.io);
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
      BuiltInType addType = HandleAdditionOperation(e.AddingOperator, termType, e.Location);
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
        if (mulType == BuiltInType.Error) return BuiltInType.Error;
        factorType = HandleMultiplyingOperation(m.MultiplyingOperator, factorType, mulType, m.Location);
        if (factorType == BuiltInType.Error) return BuiltInType.Error;
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
      BuiltInType mulType = HandleMultiplyingOperation(t.MultiplyingOperator, factorType, t.Location);
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
        new Error("Can not call size method for IntegerLiteral", l.SizeLocation, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      return BuiltInType.Integer;
    }
    public BuiltInType VisitStringLiteral(StringLiteral l)
    {
      if (l.Size)
      {
        new Error("Can not call size method for StringLiteral", l.SizeLocation, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      return BuiltInType.String;
    }
    public BuiltInType VisitRealLiteral(RealLiteral l)
    {
      if (l.Size)
      {
        new Error("Can not call size method for RealLiteral", l.SizeLocation, this.reader).Print(this.io);
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
    private BuiltInType HandleMultiplyingOperation(string op, BuiltInType t, Location l)
    {
      // <multiplying operator> ::= "*" | "/" | "%" | "and"
      string errOp = "divide";
      switch (op)
      {
        case "*":
        case "/":
          if (op == "*") errOp = "multiply";
          if (t != BuiltInType.Integer && t != BuiltInType.Real)
          {
            this.io.WriteLine($"Can not {errOp} by {t} [{l}]");
            return BuiltInType.Error;
          }
          return t;
        case "%":
          if (t != BuiltInType.Integer)
          {
            this.io.WriteLine($"Operator \"%\" expects an Integer on the right-hand-side, instead got {t} [{l}]");
            return BuiltInType.Error;
          }
          return t;
        case "and": return t;
        default: return BuiltInType.Error;
      }
    }
    private BuiltInType HandleAdditionOperation(string op, BuiltInType t, Location l)
    {
      switch (op)
      {
        case "-":
          if (t != BuiltInType.Integer && t != BuiltInType.Real)
          {
            this.io.WriteLine($"Can not subtract {t} [{l}]");
            return BuiltInType.Error;
          }
          return t;
        case "or":
        case "+": return t;
        default: return BuiltInType.Error;
      }
    }
    private BuiltInType HandleMultiplyingOperation(string op, BuiltInType t1, BuiltInType t2, Location l)
    {
      switch (op)
      {
        case "*": return HandleMultiplicationOperation(t1, t2, l);
        case "/": return HandleDivisionOperation(t1, t2, l);
        case "%": return HandleModuloOperation(t1, t2, l);
        case "and": return HandleAndOperation(t1, t2, l);
        default: return BuiltInType.Error;
      }
    }
    private BuiltInType HandleAdditionOperation(string op, BuiltInType t1, BuiltInType t2, Location l)
    {
      switch (op)
      {
        case "+": return HandlePlusOperation(t1, t2, l);
        case "-": return HandleMinusOperation(t1, t2, l);
        case "or": return HandleOrOperation(t1, t2, l);
        default: return BuiltInType.Error;
      }
    }
    private BuiltInType HandleMultiplicationOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2) return HandleDifferentTypeMultiplicationOperation(t1, t2, l);
      if (t1 == BuiltInType.Integer || t1 == BuiltInType.Real) return t1;
      this.io.WriteLine(new OperationError("*", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
    private BuiltInType HandleDivisionOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2) return HandleDifferentTypeDivisionOperation(t1, t2, l);
      if (t1 == BuiltInType.Integer || t1 == BuiltInType.Real) return t1;
      this.io.WriteLine(new OperationError("/", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
    private BuiltInType HandleModuloOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2)
      {
        this.io.WriteLine(new OperationError("%", t1, t2, l, this.reader).ToString());
        return BuiltInType.Error;
      }
      if (t1 == BuiltInType.Integer) return t1;
      this.io.WriteLine(new OperationError("%", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
    private BuiltInType HandleAndOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2) return HandleDifferentTypeAndOperation(t1, t2, l);
      if (t1 == BuiltInType.Boolean) return t1;
      this.io.WriteLine(new OperationError("and", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
    private BuiltInType HandlePlusOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2) return HandleDifferentTypePlusOperation(t1, t2, l);
      if (t1 != BuiltInType.Boolean) return t1;
      this.io.WriteLine(new OperationError("+", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
    private BuiltInType HandleMinusOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2) return HandleDifferentTypeMinusOperation(t1, t2, l);
      if (t1 == BuiltInType.Integer || t1 == BuiltInType.Real) return t1;
      this.io.WriteLine(new OperationError("-", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
    private BuiltInType HandleOrOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2) return HandleDifferentTypeOrOperation(t1, t2, l);
      if (t1 == BuiltInType.Boolean) return t1;
      this.io.WriteLine(new OperationError("or", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeMultiplicationOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 == BuiltInType.Integer && t2 == BuiltInType.Real) return BuiltInType.Real;
      if (t1 == BuiltInType.Real && t2 == BuiltInType.Integer) return BuiltInType.Real;
      this.io.WriteLine(new OperationError("*", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeDivisionOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 == BuiltInType.Integer && t2 == BuiltInType.Real) return BuiltInType.Real;
      if (t1 == BuiltInType.Real && t2 == BuiltInType.Integer) return BuiltInType.Real;
      this.io.WriteLine(new OperationError("/", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeAndOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 == BuiltInType.Boolean) return t2; // true and "ok" -> String
      this.io.WriteLine(new OperationError("and", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypePlusOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 == BuiltInType.String || t2 == BuiltInType.String) return BuiltInType.String; // Turns Type to String
      // TODO: Handle Real + Integer and Integer + Real
      this.io.WriteLine(new OperationError("+", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeMinusOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      this.io.WriteLine(new OperationError("-", t1, t2, l, this.reader).ToString());
      // TODO: Handle Integer - Real and Real - Integer
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeOrOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 == BuiltInType.Boolean) return t2; // true or "ok" -> String
      this.io.WriteLine(new OperationError("or", t1, t2, l, this.reader).ToString());
      return BuiltInType.Error;
    }
  }
}
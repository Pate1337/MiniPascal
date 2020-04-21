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
    private SymbolTableHandler Table;
    private Reader reader;

    public TypeCheckVisitor(IOHandler io, Reader reader)
    {
      this.io = io;
      this.Errors = new List<Error>();
      this.Table = new SymbolTableHandler(io, reader);
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
      this.Table.AddEntry(p.Name, new SymbolTableEntry(p.Name, BuiltInType.Void), p.Location);
      foreach (Procedure procedure in p.Procedures) procedure.Visit(this);
      foreach (Function f in p.Functions) f.Visit(this);
      p.Block.Visit(this);
    }
    public BuiltInType VisitBlock(Block b, BuiltInType expectedType)
    {
      // TODO: VisitBlock(Block b, BuiltInType type)
      // If type == BuiltInType.Void, if one of stmt Style is returnStatement. Error
      // If type != BuiltInType.Void, all possible exit points must return type

      /*
      EXIT POINTS:

      Only if the outermost if!
      If then else : if both have return => no need at the end
      If then else : if only one has return => needs return at the end
      If then : needs return at the end
      If no if : needs return at the end
      */

      /*
      if (1 = 1)
        then 
      */

      /*
      public string Style { get; set; }
      public List<Statement> statements;
      */
      this.Table.AddNewBlock();
      foreach (Statement stmt in b.statements) stmt.Visit(this);
      this.Table.RemoveCurrentBlock();
      return expectedType;
    }
    /*public BuiltInType VisitBlock(Block b, BuiltInType expectedType)
    {
      this.Table.AddNewBlock();
      bool allPathsReturnValue = true;
      foreach (Statement stmt in b.statements)
      {
        BuiltInType stmtType = stmt.Visit(this);
        if (IsStructuredStatement(stmt))
        {
          if (stmtType == BuiltInType.Empty) allPathsReturnValue = false;
        }
        // Otherwise no need to do more
      }
      if (expectedType != BuiltInType.Void && !allPathsReturnValue)
      {
        new Error("Not all paths return a value.", b.Location, this.reader).Print(this.io);
      }
      this.Table.RemoveCurrentBlock();
    }*/
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
        foreach (Lexer.Token t in s.Identifiers)
        {
          // Possible declarations to procedures, functions, parameters or program name
          // are handled in SymbolTableHandler
          this.Table.AddEntry(t.Value, new SymbolTableEntry(t.Value, type), t.Location);
        }
      }
      // else there has not been an IntegerExpression inside []. No need to error again.
    }
    public void VisitProcedure(Procedure p)
    {
      /*
      public string Style { get; set; }
      public string Name { get; set; }
      public Block Block { get; set; }
      public List<Parameter> Parameters { get; set; }
      public Location Location { get; set; }
      */

      // TODO: Find out a way to error if block contains a return statement!

      SymbolTableEntry entry = new SymbolTableEntry(p.Name, BuiltInType.Void);
      this.Table.AddEntry(p.Name, entry, p.Location);

      // Create a new scope and add the parameters and Block to that scope
      this.Table.AddNewBlock();

      // Iterate through the Parameters and make a SymbolTableEntry of each
      List<SymbolTableEntry> parameters = new List<SymbolTableEntry>();

      bool invalidParameters = false;
      foreach (Parameter par in p.Parameters)
      {
        // Add the names of parameters to this identifiers SymbolTableEntry
        // parameters.Add(par.Name);

        // The parameter declarations are added to SymbolTable in par.Visit(this)
        SymbolTableEntry e = par.Visit(this);
        if (e.Type == BuiltInType.Error) invalidParameters = true;
        parameters.Add(e);
      }
      // Add to symboltable even if there is an error in parameters, because they are added.
      // Except the ones that are not valid.
      entry.Parameters = parameters;
      if (!invalidParameters)
      {
        // Only if no errors
        // p.Block.Visit(this);
        HandleBlockWithoutReturns(p.Block);
      }

      // Remove the current scope
      this.Table.RemoveCurrentBlock();
    }
    public void VisitFunction(Function f)
    {
      /*
      public string Style { get; set; }
      public string Name { get; set; }
      public Block Block { get; set; }
      public Type Type { get; set; }
      public List<Parameter> Parameters;
      */
      BuiltInType funcType = f.Type.Visit(this);
      if (funcType != BuiltInType.Error)
      {
        SymbolTableEntry entry = new SymbolTableEntry(f.Name, funcType);
        this.Table.AddEntry(f.Name, entry, f.Location);

        this.Table.AddNewBlock();

        List<SymbolTableEntry> parameters = new List<SymbolTableEntry>();
        bool invalidParameters = false;
        foreach (Parameter par in f.Parameters)
        {
          SymbolTableEntry e = par.Visit(this);
          if (e.Type == BuiltInType.Error) invalidParameters = true;
          parameters.Add(e);
        }
        entry.Parameters = parameters;
        if (!invalidParameters)
        {
          // Only if no errors
          // f.Block.Visit(this);
          HandleBlockThatReturns(f.Block, funcType);
        }

        // Remove the current scope
        this.Table.RemoveCurrentBlock();
      }
    }
    public SymbolTableEntry VisitReferenceParameter(ReferenceParameter rp)
    {
      /*
      public string Style { get; set; }
      public string Name { get; set; }
      public Type Type { get; set; }
      public Location Location { get; set; }
      */
      BuiltInType type = rp.Type.Visit(this);
      if (type == BuiltInType.Error) return new SymbolTableEntry(rp.Name, BuiltInType.Error);
      // Only add to symbol table if valid
      SymbolTableEntry entry = new SymbolTableEntry(rp.Name, type, "ref");
      this.Table.AddEntry(rp.Name, entry, rp.Location);
      return entry;
    }
    public SymbolTableEntry VisitValueParameter(ValueParameter vp)
    {
      /*
      public string Style { get; set; }
      public string Name { get; set; }
      public Type Type { get; set; }
      public Location Location { get; set; }
      */
      BuiltInType type = vp.Type.Visit(this);
      if (type == BuiltInType.Error) return new SymbolTableEntry(vp.Name, BuiltInType.Error);
      // Only add to symbol table if valid
      SymbolTableEntry entry = new SymbolTableEntry(vp.Name, type, "val");
      this.Table.AddEntry(vp.Name, entry, vp.Location);
      return entry;
    }
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
        return HandleIllegalSizeCall(exprType, e.SizeLocation);
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
      BuiltInType addType = HandleAdditionOperation(e.AddingOperator, termType, e.Term.Factor.Location);
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
      BuiltInType mulType = HandleMultiplyingOperation(t.MultiplyingOperator, factorType, t.Factor.Location);
      return mulType;
    }
    public BuiltInType VisitIntegerLiteral(IntegerLiteral l)
    {
      /*
      public string Style { get; set; }
      public bool Size { get; set; }
      public string Value { get; set; }
      */
      if (l.Size) return HandleIllegalSizeCall(BuiltInType.Integer, l.SizeLocation);
      return BuiltInType.Integer;
    }
    public BuiltInType VisitStringLiteral(StringLiteral l)
    {
      if (l.Size) return HandleIllegalSizeCall(BuiltInType.String, l.SizeLocation);
      return BuiltInType.String;
    }
    public BuiltInType VisitRealLiteral(RealLiteral l)
    {
      if (l.Size) return HandleIllegalSizeCall(BuiltInType.Real, l.SizeLocation);
      return BuiltInType.Real;
    }
    public BuiltInType VisitNegationFactor(NegationFactor f)
    {
      /*
      public string Style { get; set; }
      public bool Size { get; set; }
      public Factor Factor { get; set; }
      public Location SizeLocation { get; set; }
      // Location of "not"
      public Location Location { get; set; }
      */
      BuiltInType factorType = f.Factor.Visit(this);
      if (factorType == BuiltInType.Error) return BuiltInType.Error;
      if (factorType != BuiltInType.Boolean)
      {
        new Error($"Can not get negation of {factorType}", f.Factor.Location, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      return BuiltInType.Boolean;
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
      SymbolTableEntry e = this.Table.GetEntry(v.Name, v.Location);

      // Error printed in SymbolTable
      if (e.Type == BuiltInType.Error) return BuiltInType.Error;

      // Variable is an array element. For example: arr[3]
      if (VariableIsArrayElement(v))
      {
        BuiltInType arrayElementType = BuiltInType.Error;
        // First check that the variable stored in SymbolTable is an array
        switch (e.Type)
        {
          case BuiltInType.IntegerArray: arrayElementType = BuiltInType.Integer; break;
          case BuiltInType.RealArray: arrayElementType = BuiltInType.Real; break;
          case BuiltInType.StringArray: arrayElementType = BuiltInType.String; break;
          case BuiltInType.BooleanArray: arrayElementType = BuiltInType.Boolean; break;
          default:
            new Error($"Variable {v.Name} is not an array", v.Location, this.reader).Print(this.io);
            return BuiltInType.Error;
        }

        // Then check that the array expression is an integer expression
        BuiltInType exprType = v.IntegerExpression.Visit(this);
        if (exprType == BuiltInType.Error) return BuiltInType.Error;
        if (exprType != BuiltInType.Integer)
        {
          new Error($"Expected {v.Name}[ Integer ], instead got {v.Name}[ {exprType} ].", v.Location, this.reader).Print(this.io);
          return BuiltInType.Error;
        }

        // If the size of variable is called. ( var[1].size not allowed )
        if (v.Size) return HandleIllegalSizeCall(arrayElementType, v.SizeLocation);

        // If everything ok, return the type of which the array consists of
        return arrayElementType;
      }
      
      if (v.Size)
      {
        if (IsArray(e.Type)) return BuiltInType.Integer;
        return HandleIllegalSizeCall(e.Type, v.SizeLocation);
      }
      return e.Type;
    }
    public BuiltInType VisitCall(Call c)
    {
      /*
      public string Style { get; set; }
      public string Name { get; set; }
      public Arguments Arguments { get; set; }
      public bool Size { get; set; }
      public Location SizeLocation { get; set; }
      public Location Location { get; set; }
      */
      SymbolTableEntry e = this.Table.GetEntry(c.Name, c.Location);
      if (e.Type == BuiltInType.Error) return BuiltInType.Error;
      if (e.Parameters == null)
      {
        new Error($"Variable {c.Name} is not a procedure or a function", c.Location, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      List<BuiltInType> argumentTypes = c.Arguments.Visit(this);
      List<BuiltInType> requiredTypes = new List<BuiltInType>();

      foreach (SymbolTableEntry p in e.Parameters)
      {
        // Can be Error if not declared.
        if (p.Type != BuiltInType.Error)
        {
          requiredTypes.Add(p.Type);
        }
        else
        {
          // Might be undeclared if there was an error with the types of parameters.
          new Error($"{(e.Type == BuiltInType.Void ? "Procedure" : "Function")} {c.Name} is not correctly declared!", c.Location, this.reader).Print(this.io);
          return BuiltInType.Error;
        }
      }

      string requiredTypesAsString = Utils.StringHandler.BuiltInTypeListToString(requiredTypes);
      string argumentTypesAsString = Utils.StringHandler.BuiltInTypeListToString(argumentTypes);
      if (argumentTypes.Count != requiredTypes.Count)
      {
        new Error($"{(e.Type == BuiltInType.Void ? "Procedure" : "Function")} {c.Name} expects {requiredTypes.Count} argument(s){(requiredTypes.Count > 0 ? " of type " + requiredTypesAsString : "")}. Instead got {argumentTypes.Count} argument(s){(argumentTypes.Count > 0 ? " of type " + argumentTypesAsString : "")}", c.Location, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      // Check that the argumentTypes match the SymbolTableEntry's parameter types
      bool error = false;
      int i = 0;
      foreach (BuiltInType t in requiredTypes)
      {
        if (t != argumentTypes[i])
        {
          error = true;
          break;
        }
        i++;
      }
      if (error)
      {
        new Error($"{(e.Type == BuiltInType.Void ? "Procedure" : "Function")} {c.Name} expects {requiredTypes.Count} argument(s){(requiredTypes.Count > 0 ? " of type " + requiredTypesAsString : "")}. Instead got {argumentTypes.Count} argument(s){(argumentTypes.Count > 0 ? " of type " + argumentTypesAsString : "")}", c.Location, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      return e.Type; // Void if Procedure call
    }
    public List<BuiltInType> VisitArguments(Arguments a)
    {
      /*
      public string Style { get; set; }
      public List<Expression> Expressions { get; set; }
      */
      List<BuiltInType> types = new List<BuiltInType>();
      foreach (Expression e in a.Expressions)
      {
        types.Add(e.Visit(this));
        // No need to give error message here
      }
      return types;
    }
    public void VisitAssertStatement(AssertStatement s){}
    public void VisitAssignmentStatement(AssignmentStatement s){}
    public void VisitIfStatement(IfStatement s)
    {
      /*
      public string Style { get; set; }
      public Expression BooleanExpression { get; set; }
      public Statement ThenStatement { get; set; }
      public Statement ElseStatement { get; set; }
      */
      BuiltInType exprType = s.BooleanExpression.Visit(this);
      if (exprType != BuiltInType.Boolean && exprType != BuiltInType.Error) new Error($"Expected Boolean expression after keyword \"if\". Instead got {exprType}.", s.Location, this.reader).Print(this.io);
      s.ThenStatement.Visit(this);
      if (s.ElseStatement != null)
      {
        s.ElseStatement.Visit(this);
      }
    }
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
    private void HandleBlockThatReturns(Block b, BuiltInType type)
    {
      // Make sure that all the branches of the block has a return statement
      // that return type
      b.Visit(this, type);
    }
    private void HandleBlockWithoutReturns(Block b)
    {
      // Make sure that none of the statements are return statements
      b.Visit(this);
    }
    private BuiltInType HandleIllegalSizeCall(BuiltInType type, Location loc)
    {
      new Error($"Can not do size-operation for {type}", loc, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    // private BuiltInType GetArrayElementVariableType(Variable v)
    private bool VariableIsArrayElement(Variable v)
    {
      return v.IntegerExpression != null;
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
            // this.io.WriteLine($"Can not {errOp} by {t} [{l}]");
            new Error($"Can not {errOp} by {t}", l, this.reader).Print(this.io);
            return BuiltInType.Error;
          }
          return t;
        case "%":
          if (t != BuiltInType.Integer)
          {
            // this.io.WriteLine($"Operator \"%\" expects an Integer on the right-hand-side, instead got {t} [{l}]");
            new Error($"Operator \"%\" expects an Integer on the right-hand-side, instead got {t}", l, this.reader).Print(this.io);
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
            // this.io.WriteLine($"Can not subtract {t} [{l}]");
            new Error($"Can not subtract {t}", l, this.reader).Print(this.io);
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
      new OperationError("*", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandleDivisionOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2) return HandleDifferentTypeDivisionOperation(t1, t2, l);
      if (t1 == BuiltInType.Integer || t1 == BuiltInType.Real) return t1;
      new OperationError("/", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandleModuloOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2)
      {
        new OperationError("%", t1, t2, l, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      if (t1 == BuiltInType.Integer) return t1;
      new OperationError("%", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandleAndOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2) return HandleDifferentTypeAndOperation(t1, t2, l);
      if (t1 == BuiltInType.Boolean) return t1;
      new OperationError("and", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandlePlusOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2) return HandleDifferentTypePlusOperation(t1, t2, l);
      if (t1 != BuiltInType.Boolean) return t1;
      new OperationError("+", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandleMinusOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2) return HandleDifferentTypeMinusOperation(t1, t2, l);
      if (t1 == BuiltInType.Integer || t1 == BuiltInType.Real) return t1;
      new OperationError("-", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandleOrOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 != t2) return HandleDifferentTypeOrOperation(t1, t2, l);
      if (t1 == BuiltInType.Boolean) return t1;
      new OperationError("or", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeMultiplicationOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 == BuiltInType.Integer && t2 == BuiltInType.Real) return BuiltInType.Real;
      if (t1 == BuiltInType.Real && t2 == BuiltInType.Integer) return BuiltInType.Real;
      new OperationError("*", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeDivisionOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 == BuiltInType.Integer && t2 == BuiltInType.Real) return BuiltInType.Real;
      if (t1 == BuiltInType.Real && t2 == BuiltInType.Integer) return BuiltInType.Real;
      new OperationError("/", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeAndOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 == BuiltInType.Boolean) return t2; // true and "ok" -> String
      new OperationError("and", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypePlusOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 == BuiltInType.String || t2 == BuiltInType.String) return BuiltInType.String; // Turns Type to String
      // TODO: Handle Real + Integer and Integer + Real
      new OperationError("+", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeMinusOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      new OperationError("-", t1, t2, l, this.reader).Print(this.io);
      // TODO: Handle Integer - Real and Real - Integer
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypeOrOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 == BuiltInType.Boolean) return t2; // true or "ok" -> String
      new OperationError("or", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
  }
}
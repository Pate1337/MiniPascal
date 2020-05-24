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
    private SymbolTableHandler Table;
    private Reader reader;
    private BuiltInType expectedReturnType;
    private bool allPathsReturnValue;
    private List<Warning> warnings;
    private CodeGeneration.FunctionCreator fg;

    public TypeCheckVisitor(IOHandler io, Reader reader, CodeGeneration.FunctionCreator fg)
    {
      this.io = io;
      this.Table = new SymbolTableHandler(io, reader);
      this.reader = reader;
      this.allPathsReturnValue = false;
      this.warnings = new List<Warning>();
      this.fg = fg;
    }
    public void VisitProgram(ProgramNode p)
    {
      this.Table.AddEntry(p.Name, new SymbolTableEntry(p.Name, BuiltInType.Void), p.Location);
      foreach (Procedure procedure in p.Procedures) procedure.Visit(this);
      foreach (Function f in p.Functions) f.Visit(this);
      this.expectedReturnType = BuiltInType.None;
      p.Block.Visit(this);
      foreach (Warning w in this.warnings) w.Print(this.io);
    }
    public void VisitProceduresAndFunctions(ProgramNode p){}
    public void VisitBlock(Block b, bool needsToReturnValue)
    {
      bool returnsBeforeEnd = false;
      this.Table.AddNewBlock();
      int i = 1;
      foreach (Statement stmt in b.statements)
      {
        if (returnsBeforeEnd) this.warnings.Add(new Warning($"Unreachable code detected.", stmt.Location));
        // Only if or return statements can switch to true.
        this.allPathsReturnValue = false;
        stmt.Visit(this);
        if (this.allPathsReturnValue && i < b.statements.Count) returnsBeforeEnd = true;
        i++;
      }
      this.Table.RemoveCurrentBlock();
      if (!returnsBeforeEnd && needsToReturnValue && !this.allPathsReturnValue) new Error("Not all paths return a value", b.Location, this.reader).Print(this.io);
    }
    public void VisitDeclaration(Declaration s)
    {
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
      if (type == BuiltInType.StringArray) this.fg.StringArrayInitialization = true;
      s.BuiltInType = type;
      // else there has not been an IntegerExpression inside []. No need to error again.
    }
    public void VisitProcedure(Procedure p)
    {
      this.expectedReturnType = BuiltInType.Void;
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
        p.Block.Visit(this);
      }

      // Remove the current scope
      this.Table.RemoveCurrentBlock();
    }
    public void VisitFunction(Function f)
    {
      /*
      Outer Block: funtion name
      Middle block: parameters
      Inner block: local variables
      */
      BuiltInType funcType = f.Type.Visit(this);
      if (funcType != BuiltInType.Error)
      {
        this.expectedReturnType = funcType;
        SymbolTableEntry entry = new SymbolTableEntry(f.Name, funcType);
        this.Table.AddEntry(f.Name, entry, f.Location);

        // TODO: If AddEntry fails, maybe stop.

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
          f.Block.Visit(this, true);
        }
        // Remove the current scope
        this.Table.RemoveCurrentBlock();
      }
    }
    public SymbolTableEntry VisitReferenceParameter(ReferenceParameter rp)
    {
      return VisitParameter(rp, "ref");
    }
    public SymbolTableEntry VisitValueParameter(ValueParameter vp)
    {
      return VisitParameter(vp, "val");
    }
    private SymbolTableEntry VisitParameter(Parameter p, string parameterType)
    {
      BuiltInType type = p.Type.Visit(this);
      if (type == BuiltInType.Error) return new SymbolTableEntry(p.Name, BuiltInType.Error);
      // Only add to symbol table if valid
      SymbolTableEntry entry = new SymbolTableEntry(p.Name, type, parameterType);
      this.Table.AddEntry(p.Name, entry, p.Location);
      return entry;
    }
    public BuiltInType VisitArrayType(ArrayType t)
    {
      if (t.IntegerExpression == null) return t.Type;
      BuiltInType exprType = t.IntegerExpression.Visit(this);
      if (exprType == BuiltInType.Error) return BuiltInType.Error;
      if (exprType != BuiltInType.Integer)
      {
        new Error($"Expected \"[\" Integer \"]\", instead got \"[\" {exprType} \"]\".", t.Location, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      // Tell the FunctionCreator to write NegativeIndex function
      this.fg.NegativeIndex = true;
      return t.Type;
    }
    public BuiltInType VisitSimpleType(SimpleType t)
    {
      return t.Type;
    }
    public BuiltInType VisitSimpleExpression(SimpleExpression e)
    {
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
      e.Type = termType;
      return termType;
    }
    public BuiltInType VisitBooleanExpression(BooleanExpression e)
    {
      BuiltInType leftType = e.Left.Visit(this);
      BuiltInType rightType = e.Right.Visit(this);
      if (leftType == BuiltInType.Error || rightType == BuiltInType.Error) return BuiltInType.Error;
      if (leftType != rightType)
      {
        if (e.RelationalOperator == "=" || e.RelationalOperator == "<>")
        {
          if (
            (leftType == BuiltInType.Integer && rightType == BuiltInType.Boolean) ||
            (leftType == BuiltInType.Boolean && rightType == BuiltInType.Integer)
            ) return BuiltInType.Boolean;
        }
        new OperationError(e.RelationalOperator, leftType, rightType, e.Location, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      else if (leftType == BuiltInType.String)
      {
        if (e.RelationalOperator == "=" || e.RelationalOperator == "<>") this.fg.CompareStrings = true;
        else
        {
          new OperationError(e.RelationalOperator, leftType, rightType, e.Location, this.reader).Print(this.io);
          return BuiltInType.Error;
        }
      }
      else if (IsArray(leftType))
      {
        if (e.RelationalOperator == "=" || e.RelationalOperator == "<>")
        {
          switch (leftType)
          {
            case BuiltInType.IntegerArray: break; // this.fg.CompareIntegerArrays = true;
            case BuiltInType.StringArray: break;
            default: break;
          }
        }
        else
        {
          new OperationError(e.RelationalOperator, leftType, rightType, e.Location, this.reader).Print(this.io);
          return BuiltInType.Error;
        }
      }
      // <relational operator> ::= "=" | "<>" | "<" | "<=" | ">=" | ">"
      return BuiltInType.Boolean;
    }
    public BuiltInType VisitClosedExpression(ClosedExpression e)
    {
      BuiltInType exprType = e.Expression.Visit(this);
      if (exprType == BuiltInType.Error) return BuiltInType.Error;
      if (e.Size)
      {
        if (IsArray(exprType) || exprType == BuiltInType.String)
        {
          e.Type = BuiltInType.Integer;
          return BuiltInType.Integer;
        }
        // TODO: If String size possible, add here
        return HandleIllegalSizeCall(exprType, e.SizeLocation);
      }
      e.Type = exprType;
      return exprType;
    }
    public BuiltInType VisitSimpleExpressionAddition(SimpleExpressionAddition e)
    {
      BuiltInType termType = e.Term.Visit(this);
      if (termType == BuiltInType.Error) return BuiltInType.Error;
      BuiltInType addType = HandleAdditionOperation(e.AddingOperator, termType, e.Term.Factor.Location);
      return addType;
    }
    public BuiltInType VisitTerm(Term t)
    {
      BuiltInType factorType = t.Factor.Visit(this);
      if (factorType == BuiltInType.Error) return BuiltInType.Error;
      foreach (TermMultiplicative m in t.Multiplicatives)
      {
        BuiltInType mulType = m.Visit(this);
        if (mulType == BuiltInType.Error) return BuiltInType.Error;
        factorType = HandleMultiplyingOperation(m.MultiplyingOperator, factorType, mulType, m.Location);
        if (factorType == BuiltInType.Error) return BuiltInType.Error;
      }
      t.Type = factorType;
      return factorType;
    }
    public BuiltInType VisitTermMultiplicative(TermMultiplicative t)
    {
      // <multiplying operator> ::= "*" | "/" | "%" | "and"
      BuiltInType factorType = t.Factor.Visit(this);
      if (factorType == BuiltInType.Error) return BuiltInType.Error;
      BuiltInType mulType = HandleMultiplyingOperation(t.MultiplyingOperator, factorType, t.Factor.Location);
      return mulType;
    }
    public BuiltInType VisitIntegerLiteral(IntegerLiteral l)
    {
      if (l.Size) return HandleIllegalSizeCall(BuiltInType.Integer, l.SizeLocation);
      return BuiltInType.Integer;
    }
    public BuiltInType VisitStringLiteral(StringLiteral l)
    {
      if (l.Size)
      {
        return BuiltInType.Integer;
        // return HandleIllegalSizeCall(BuiltInType.String, l.SizeLocation);
      }
      this.fg.MakeStringVar = true;
      return BuiltInType.String;
    }
    public BuiltInType VisitRealLiteral(RealLiteral l)
    {
      if (l.Size) return HandleIllegalSizeCall(BuiltInType.Real, l.SizeLocation);
      return BuiltInType.Real;
    }
    public BuiltInType VisitNegationFactor(NegationFactor f)
    {
      BuiltInType factorType = f.Factor.Visit(this);
      if (factorType == BuiltInType.Error) return BuiltInType.Error;
      if (factorType != BuiltInType.Boolean && factorType != BuiltInType.Integer)
      {
        new Error($"Can not get negation of {factorType}", f.Factor.Location, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      return BuiltInType.Boolean;
    }
    public BuiltInType VisitVariable(Variable v)
    {
      SymbolTableEntry e = this.Table.GetEntry(v.Name, v.Location);

      v.Type = e.Type;

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
          case BuiltInType.StringArray:
            arrayElementType = BuiltInType.String;
            this.fg.GetElementFromStringArray = true;
            break;
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
          new Error($"Expected {v.Name}[ <Integer> ], instead got {v.Name}[ <{exprType}> ].", v.Location, this.reader).Print(this.io);
          return BuiltInType.Error;
        }

        // If the size of variable is called. ( var[1].size not allowed, only for strings )
        if (v.Size)
        {
          if (arrayElementType != BuiltInType.String) return HandleIllegalSizeCall(arrayElementType, v.SizeLocation);
          return BuiltInType.Integer;
        }
        // Tell the FunctionCreator to write IndexInBounds function to the C-file
        this.fg.IndexInBounds = true;
        // If everything ok, return the type of which the array consists of
        return arrayElementType;
      }
      if (v.Size)
      {
        if (IsArray(e.Type) || e.Type == BuiltInType.String)
        {
          //v.Type = BuiltInType.Integer;
          return BuiltInType.Integer;
        }
        return HandleIllegalSizeCall(e.Type, v.SizeLocation);
      }
      //v.Type = e.Type;
      return e.Type;
    }
    public BuiltInType VisitCall(Call c)
    {
      SymbolTableEntry e = this.Table.GetEntry(c.Name, c.Location);
      if (e.Type == BuiltInType.Error) return BuiltInType.Error;
      if (e.Parameters == null)
      {
        new Error($"Variable {c.Name} is not a procedure or a function", c.Location, this.reader).Print(this.io);
        return BuiltInType.Error;
      }
      List<BuiltInType> argumentTypes = c.Arguments.Visit(this);
      List<BuiltInType> requiredTypes = new List<BuiltInType>();

      List<int> refParameters = new List<int>();
      int index = 0;
      foreach (SymbolTableEntry p in e.Parameters)
      {
        // Can be Error if not declared.
        if (p.Type != BuiltInType.Error)
        {
          requiredTypes.Add(p.Type);
          if (p.ParameterType == "ref") refParameters.Add(index);
        }
        else
        {
          // Might be undeclared if there was an error with the types of parameters.
          new Error($"{(e.Type == BuiltInType.Void ? "Procedure" : "Function")} {c.Name} is not correctly declared!", c.Location, this.reader).Print(this.io);
          return BuiltInType.Error;
        }
        index++;
      }
      c.Arguments.Refs = refParameters;
      string requiredTypesAsString = Utils.StringHandler.BuiltInTypeListToString(requiredTypes);
      string argumentTypesAsString = Utils.StringHandler.BuiltInTypeListToString(argumentTypes);
      if (argumentTypes.Count != requiredTypes.Count)
      {
        new Error($"{(e.Type == BuiltInType.Void ? "Procedure" : "Function")} {c.Name} expects {requiredTypes.Count} argument(s){(requiredTypes.Count > 0 ? " of type " + requiredTypesAsString : "")}. Instead got {argumentTypes.Count} argument(s){(argumentTypes.Count > 0 ? " of type " + argumentTypesAsString : "")}", c.Location, this.reader).Print(this.io);
        return BuiltInType.Error;
      }

      // Check that ref parameters are passed correctly
      foreach (int indx in refParameters)
      {
        if (!ExpressionCanBeUsedAsReferenceParameter(c.Arguments.Expressions[indx])) new Error($"Reference parameters need to be passed as variables.", c.Location, this.reader).Print(this.io);
        // return BuiltInType.Error;
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
      c.Type = e.Type;
      return e.Type; // Void if Procedure call
    }
    public List<BuiltInType> VisitArguments(Arguments a)
    {
      List<BuiltInType> types = new List<BuiltInType>();
      foreach (Expression e in a.Expressions)
      {
        types.Add(e.Visit(this));
        // No need to give error message here
      }
      a.Types = types;
      return types;
    }
    public void VisitAssertStatement(AssertStatement s)
    {
      BuiltInType exprType = s.BooleanExpression.Visit(this);
      if (exprType != BuiltInType.Error && exprType != BuiltInType.Boolean) new Error($"Expected assert( <Boolean> ). Instead got assert( <{exprType}> ).", s.Location, this.reader).Print(this.io);
    }
    public void VisitAssignmentStatement(AssignmentStatement s)
    {
      BuiltInType varType = s.Variable.Visit(this);
      s.Variable.LHS = true;
      if (varType != BuiltInType.Error)
      {
        BuiltInType exprType = s.Expression.Visit(this);
        if (exprType != BuiltInType.Error)
        {
          // Tell the function creator to write function AssignStringToStringArray
          if (s.Variable.Type == BuiltInType.StringArray && exprType == BuiltInType.String) this.fg.AssignStringToStringArray = true;
          else if (s.Variable.Type == BuiltInType.StringArray && exprType == BuiltInType.StringArray)
          {
            this.fg.CopyCharPointer = true;
            this.fg.CopyIntegerPointer = true;
          }
          else if (s.Variable.Type == BuiltInType.IntegerArray && exprType == BuiltInType.IntegerArray) this.fg.CopyIntegerPointer = true;
          string id = s.Variable.Name;
          if (varType != exprType) new Error($"Can not assign {exprType} into a {varType} variable {id}.", s.Location, this.reader).Print(this.io);
          else this.Table.Assign(id, s.Location); // Does the checks for illegal assignments
        }
      }
      // If everything is ok, decorate the AssignmentStatement Node with the type
    }
    public void VisitIfStatement(IfStatement s)
    {
      this.allPathsReturnValue = false;
      bool allPathsReturnValue = false;
      BuiltInType exprType = s.BooleanExpression.Visit(this);
      if (exprType != BuiltInType.Boolean && exprType != BuiltInType.Error) new Error($"Expected Boolean expression after keyword \"if\". Instead got {exprType}.", s.Location, this.reader).Print(this.io);
      // Also check blocks even if there was an error in the expression
      s.ThenStatement.Visit(this);
      if (this.allPathsReturnValue) allPathsReturnValue = true;
      if (s.ElseStatement != null)
      {
        s.ElseStatement.Visit(this);
        if (this.allPathsReturnValue && allPathsReturnValue) allPathsReturnValue = true;
        else allPathsReturnValue = false;
      }
      else allPathsReturnValue = false; // Only "if then" does not return value on all paths
      this.allPathsReturnValue = allPathsReturnValue;
    }
    public void VisitReadStatement(ReadStatement s)
    {
      foreach (Variable v in s.Variables)
      {
        v.LHS = true;
        BuiltInType varType = v.Visit(this);
        if (IsArray(varType) || varType == BuiltInType.Boolean) new Error($"Can not read into variable \"{v.Name}\" of type {varType}. Can only read to variables of type Integer, String or Real.", v.Location, this.reader).Print(this.io);
        else if (varType != BuiltInType.Error) this.Table.Assign(v.Name, v.Location); // Reports illegal assignments
      }
    }
    public void VisitReturnStatement(ReturnStatement s)
    {
      this.allPathsReturnValue = true;
      if (this.expectedReturnType == BuiltInType.None) new Error("Return statement is not allowed in the main block.", s.Location, this.reader).Print(this.io);
      else
      {
        BuiltInType returnType;
        if (s.Expression == null) returnType = BuiltInType.Void;
        else returnType = s.Expression.Visit(this);
        if (returnType != BuiltInType.Error)
        {
          // Only if no Error in expression
          if (returnType != this.expectedReturnType) new Error($"Expected a return type of {this.expectedReturnType}. Instead got {returnType}.", s.Location, this.reader).Print(this.io);
        }
      }
    }
    public void VisitWhileStatement(WhileStatement s)
    {
      BuiltInType exprType = s.BooleanExpression.Visit(this);
      if (exprType != BuiltInType.Boolean && exprType != BuiltInType.Error) new Error($"Expected Boolean expression after keyword \"while\". Instead got {exprType}.", s.Location, this.reader).Print(this.io);
      s.Statement.Visit(this);
    }
    public void VisitWriteStatement(WriteStatement s)
    {
      if (s.Arguments != null)
      {
        s.Arguments.Visit(this);
        foreach(BuiltInType t in s.Arguments.Types)
        {
          if (t == BuiltInType.IntegerArray) this.fg.IntegerArrayToString = true;
          else if (t == BuiltInType.StringArray) this.fg.StringArrayToString = true;
        }
      }
    }
    private bool ExpressionCanBeUsedAsReferenceParameter(Expression e)
    {
      if (e.Style != "SimpleExpression") return false;
      SimpleExpression simple = (SimpleExpression) e;
      if (simple.Sign != null) return false;
      if (simple.Additions.Count != 0) return false;
      Term term = simple.Term;
      if (term.Multiplicatives.Count != 0) return false;
      Factor factor = term.Factor;
      if (factor.Size == true) return false;
      Node factorNode = (Node) factor;
      if (factorNode.Style != "Variable") return false;
      return true;
    }
    private BuiltInType HandleIllegalSizeCall(BuiltInType type, Location loc)
    {
      new Error($"Can not do size-operation for {type}", loc, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
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
            new Error($"Can not {errOp} by {t}", l, this.reader).Print(this.io);
            return BuiltInType.Error;
          }
          return t;
        case "%":
          if (t != BuiltInType.Integer)
          {
            new Error($"Operator \"%\" expects an Integer on the right-hand-side, instead got {t}", l, this.reader).Print(this.io);
            return BuiltInType.Error;
          }
          return t;
        case "and":
          if (t != BuiltInType.Boolean)
          {
            new Error($"Operator \"and\" expects a Boolean on the right-hand-side, instead got {t}", l, this.reader).Print(this.io);
            return BuiltInType.Error;
          }
          return t;
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
            new Error($"Can not subtract {t}", l, this.reader).Print(this.io);
            return BuiltInType.Error;
          }
          return t;
        case "or":
          if (t != BuiltInType.Boolean && t != BuiltInType.Integer)
          {
            new Error($"Can not use keyword \"or\" with {t}", l, this.reader).Print(this.io);
            return BuiltInType.Error;
          }
          return t;
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
      if (t1 == BuiltInType.String && t2 == BuiltInType.String) this.fg.ConcatStrings = true;
      if (t1 == BuiltInType.IntegerArray && t2 == BuiltInType.IntegerArray) this.fg.ConcatIntegerArrays = true;
      if (t1 == BuiltInType.StringArray && t2 == BuiltInType.StringArray)
      {
        this.fg.CountNewOffsets = true;
        this.fg.ConcatStringArrays = true;
      }
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
      // Only allow for 2 booleans
      // if (t1 == BuiltInType.Boolean) return t2; // true and "ok" -> String
      new OperationError("and", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
    private BuiltInType HandleDifferentTypePlusOperation(BuiltInType t1, BuiltInType t2, Location l)
    {
      if (t1 == BuiltInType.String || t2 == BuiltInType.String)
      {
        this.fg.ConcatStrings = true;
        if (t1 == BuiltInType.Integer || t2 == BuiltInType.Integer || t1 == BuiltInType.Boolean || t2 == BuiltInType.Boolean) this.fg.IntegerToStringWithSizeCalc = true;
        else if (t1 == BuiltInType.IntegerArray || t2 == BuiltInType.IntegerArray) this.fg.IntegerArrayToString = true;
        return BuiltInType.String; // Turns Type to String
      }
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
      // Allows (1=1) or 1
      if (
        (t1 == BuiltInType.Boolean && t2 == BuiltInType.Integer) ||
        (t1 == BuiltInType.Integer && t2 == BuiltInType.Boolean)
        ) return BuiltInType.Boolean;
      // if (t1 == BuiltInType.Boolean) return t2; // true or "ok" -> String
      new OperationError("or", t1, t2, l, this.reader).Print(this.io);
      return BuiltInType.Error;
    }
  }
}
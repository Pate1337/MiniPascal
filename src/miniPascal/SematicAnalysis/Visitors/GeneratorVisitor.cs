using System.Collections.Generic;
using System.Collections;
// using Errors;
using Nodes;
using FileHandler;
using IO;
// using CodeGeneration;

namespace Semantic
{
  public class GeneratorVisitor : Visitor
  {
    private IOHandler io;
    private Reader reader;
    private FileWriter writer;
    private CodeGeneration.VariableHandler varHandler;
    private Stack<CodeGeneration.Variable> stack;
    public GeneratorVisitor(IOHandler io, Reader reader, FileWriter writer)
    {
      this.io = io;
      this.reader = reader;
      this.writer = writer;
      this.varHandler = new CodeGeneration.VariableHandler();
      this.stack = new Stack<CodeGeneration.Variable>();
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
      /*
      public string Style { get; set; }
      public BuiltInType Type { get; set; }
      public Expression IntegerExpression { get; set; }
      // Location of LeftBracket
      public Location Location { get; set; }
      */
      t.IntegerExpression.Visit(this);
      // After visiting expression, there is the IntegerExpression's value at the
      // top of this.stack
      return BuiltInType.Error;
    }
    public BuiltInType VisitSimpleType(SimpleType t)
    {
      return t.Type;
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
      // TODO: Handle Sign
      e.Term.Visit(this);
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
      // Always store in variable
      CodeGeneration.Variable v = this.varHandler.GetFreeVariable(BuiltInType.Integer);
      System.Console.WriteLine("Free variable in itegerLiteral is: " + v.Id + ", " + v.OriginalId);
      if (v.Id == null)
      {
        // Declare new in C
        v = this.varHandler.DeclareVariable(BuiltInType.Integer); // Without a name
        this.writer.Write($"int ");
      }
      this.stack.Push(v); // Push the variable into stack
      this.writer.Write($"{v.Id}={l.Value};\n");
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
    private BuiltInType CorrespondingArray(BuiltInType t)
    {
      switch(t)
      {
        case BuiltInType.Integer: return BuiltInType.IntegerArray;
        case BuiltInType.String: return BuiltInType.StringArray;
        case BuiltInType.Real: return BuiltInType.RealArray;
        case BuiltInType.Boolean: return BuiltInType.BooleanArray;
        default: return BuiltInType.Error;
      }
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
      public BuiltInType Type { get; set; }
      */

      // Can not be a free variable.

      // TODO: Handle size

      CodeGeneration.Variable variable;
      // this.writer.Write(variable.Id);
      /*if (v.IntegerExpression != null)
      {
        // TODO: Store the io[i2] to a new free variable (or declare new and push that to stack)
        // Need to search for variable in declared arrays (by OriginalId)
        // CodeGeneration.Variable variable = this.varHandler.GetFreeVariable(v.Type);
        CodeGeneration.Variable variable = this.varHandler.GetVariable(v.Name, CorrespondingArray(v.Type));
        v.IntegerExpression.Visit(this); // Stores the value on top of stack
        CodeGeneration.Variable exprVar = this.stack.Pop();
        if (exprVar.OriginalId == null) this.varHandler.FreeVariable(exprVar, BuiltInType.Integer);
        // this.writer.Write($"[{exprVar.Id}]");
        // Change the variable Id to a[]
        variable.Id = $"{variable.Id}[{exprVar.Id}]";
        
      }*/
      if (v.IntegerExpression != null)
      {
        // TODO: Store the io[i2] to a new free variable (or declare new and push that to stack)
        // Need to search for variable in declared arrays (by OriginalId)
        // CodeGeneration.Variable variable = this.varHandler.GetFreeVariable(v.Type);
        CodeGeneration.Variable array = this.varHandler.GetVariable(v.Name, CorrespondingArray(v.Type));
        v.IntegerExpression.Visit(this); // Stores the value on top of stack
        CodeGeneration.Variable exprVar = this.stack.Pop();
        if (exprVar.OriginalId == null) this.varHandler.FreeVariable(exprVar, BuiltInType.Integer);
        // this.writer.Write($"[{exprVar.Id}]");
        // Change the variable Id to a[]
        // variable.Id = $"{variable.Id}[{exprVar.Id}]";
        variable = this.varHandler.GetFreePointer(v.Type);
        if (variable.Id == null)
        {
          variable = this.varHandler.DeclarePointer(v.Type);
          this.writer.Write($"{TypeInC(v.Type)} ");
          this.writer.Write($"{variable.Id}=&{array.Id}[{exprVar.Id}];\n");
        }
        else
        {
          // If declared already, remove the *
          this.writer.Write($"{variable.Id.Substring(1)}=&{array.Id}[{exprVar.Id}];\n");
        }
      }
      else
      {
        variable = this.varHandler.GetVariable(v.Name, v.Type);
      }
      this.stack.Push(variable);
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
      if (a.Expressions.Count == 0) return new List<BuiltInType>();
      // Loop the list in reversed order to store the values to stack in correct order
      for (int i = a.Expressions.Count - 1; i >= 0; i--)
      {
        a.Expressions[i].Visit(this);
      }
      return new List<BuiltInType>();
    }
    public void VisitBlock(Block b, bool needsToReturnValue)
    {
      foreach (Statement stmt in b.statements) stmt.Visit(this);
    }
    public void VisitAssertStatement(AssertStatement s){}
    public void VisitAssignmentStatement(AssignmentStatement s)
    {
      /*
      public string Style { get; set; }
      public Variable Variable { get; set; }
      public Expression Expression { get; set; }
      public Location Location { get; set; }
      */
      s.Expression.Visit(this); // stores the value to top most variable in stack
      s.Variable.Visit(this); //  stores on top of stack
      CodeGeneration.Variable varVar = this.stack.Pop();
      CodeGeneration.Variable exprVar = this.stack.Pop();
      this.writer.Write($"{varVar.Id}={exprVar.Id};\n");
      // Can now free the exprVar. Should know the exprVar type aswell..
      if (exprVar.OriginalId == null)
      {
        if (this.varHandler.IsPointer(exprVar)) this.varHandler.FreePointer(exprVar, s.Variable.Type);
        else this.varHandler.FreeVariable(exprVar, s.Variable.Type);
      }
      // Free the left-hand side if it is a pointer (stored in the original location anyways)
      if (this.varHandler.IsPointer(varVar)) this.varHandler.FreePointer(varVar, s.Variable.Type);
    }
    public void VisitDeclaration(Declaration s)
    {
      /*
      public string Style { get; set; }
      // public List<string> Identifiers { get; set; }
      public List<Token> Identifiers { get; set; }
      public Type Type { get; set; }
      public Location Location { get; set; }
      public BuiltInType BuiltInType { get; set; }
      */
      // May not need to declare a new C variable, if there are free ones
      string type = TypeInC(s.BuiltInType);
      if (IsArray(s.BuiltInType))
      {
        // Need to Visit Type and calculate the result of the IntegerExpression
        // After we have r<1> = value
        // int[r1] t.Value;
        s.Type.Visit(this); // Does the C calculation of IntegerExpression, and stores it
        // in the top most variable in the stack
        CodeGeneration.Variable exprVar = this.stack.Pop();
        // Free the variable that holds the value of the [IntegerExpression]
        // NOTE: Unless possible to declare with var a : array[b] of integer;
        this.varHandler.FreeVariable(exprVar, BuiltInType.Integer);
        foreach(Lexer.Token t in s.Identifiers)
        {
          // int* a = malloc(r1 * sizeof(r1));
          CodeGeneration.Variable variable = this.varHandler.GetFreeVariable(s.BuiltInType);
          if (variable.Id == null)
          {
            // No free. Declare new.
            variable = this.varHandler.DeclareVariable(s.BuiltInType, t.Value);
            this.writer.Write($"{type} {variable.Id}=malloc({exprVar.Id}*sizeof({exprVar.Id}));\n");
          }
          else
          {
            // Was already declared. Need to adjust the size.
            // Also free the existing.
            this.writer.Write($"free({variable.Id});\n");
            variable.OriginalId = t.Value;
            this.writer.Write($"{variable.Id}=malloc({exprVar.Id}*sizeof({exprVar.Id}));\n");
          }
        }
      }
      else
      {
        foreach(Lexer.Token t in s.Identifiers)
        {
          CodeGeneration.Variable variable = this.varHandler.GetFreeVariable(s.BuiltInType);
          if (variable.Id == null)
          {
            // Need to declare a new one in C
            variable = this.varHandler.DeclareVariable(s.BuiltInType, t.Value);
            this.writer.Write($"{type} {variable.Id};\n");
          }
          else variable.OriginalId = t.Value; // Change the originalId
        }
      }
    }
    private string TypeInC(BuiltInType t)
    {
      switch(t)
      {
        case BuiltInType.Integer: return "int";
        case BuiltInType.String: return "char*";
        case BuiltInType.IntegerArray: return "int*";
        default: return "";
      }
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
      // s.Arguments.Types
      s.Arguments.Visit(this); // Stores Arguments count amount of values to stack
      foreach(BuiltInType t in s.Arguments.Types)
      {
        PrintType(t);
      }
      // Print newline at the end
      this.writer.Write("printf(\"\\n\");\n");
    }
    private void PrintType(BuiltInType t)
    {
      CodeGeneration.Variable v = this.stack.Pop();
      switch(t)
      {
        case BuiltInType.Integer:
          this.writer.Write($"printf(\"%d\", {v.Id});\n");
          if (v.OriginalId == null)
          {
            if (this.varHandler.IsPointer(v)) this.varHandler.FreePointer(v, t);
            else this.varHandler.FreeVariable(v, t);
          }
          break;
        case BuiltInType.String:
          this.writer.Write($"printf({v.Id});\n");
          if (v.OriginalId == null) this.varHandler.FreeVariable(v, t);
          break;
        default: break;
      }
    }
  }
}
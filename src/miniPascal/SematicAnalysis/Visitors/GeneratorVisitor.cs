using System.Collections.Generic;
using System.Collections;
using Errors;
using Nodes;
using FileHandler;
using IO;

namespace Semantic
{
  public class GeneratorVisitor : Visitor
  {
    private IOHandler io;
    private Reader reader;
    private FileWriter writer;
    private CodeGeneration.VariableHandler varHandler;
    private Stack<CodeGeneration.Variable> stack;
    private CodeGeneration.LabelGenerator labelGenerator;
    private int block;
    private bool ParameterCheck;
    private Stack<int> ParameterValues;
    private CodeGeneration.Variable returnSize; // Used in functions
    private CodeGeneration.Variable returnLengths; // Used in functions
    public GeneratorVisitor(IOHandler io, Reader reader, FileWriter writer)
    {
      this.io = io;
      this.reader = reader;
      this.writer = writer;
      this.varHandler = new CodeGeneration.VariableHandler();
      this.stack = new Stack<CodeGeneration.Variable>();
      this.labelGenerator = new CodeGeneration.LabelGenerator();
      this.block = 0;
      this.ParameterCheck = false;
      this.ParameterValues = new Stack<int>();
      this.returnSize = new CodeGeneration.Variable();
      this.returnLengths = new CodeGeneration.Variable();
    }
    public void VisitProceduresAndFunctions(ProgramNode p)
    {
      foreach (Procedure procedure in p.Procedures) procedure.Visit(this);
      foreach (Function f in p.Functions) f.Visit(this);
    }
    public void VisitProgram(ProgramNode p)
    {
      p.Block.Visit(this);
    }
    public void VisitProcedure(Procedure p)
    {
      List<CodeGeneration.Variable> parameters = HandleParameters(p.Parameters);
      this.writer.Write($"int {p.Name}(");
      int i = 1;
      foreach (CodeGeneration.Variable v in parameters)
      {
        this.writer.Write($"{TypeInC(v.Type)} {v.Id}");
        if (i < parameters.Count) this.writer.Write(",");
        i++;
      }
      this.writer.Write("){\n");
      p.Block.Visit(this);
      this.varHandler.RemoveAll();
      this.writer.Write("return 0;\n");
      this.writer.Write("}\n");
    }
    private List<CodeGeneration.Variable> HandleParameters(List<Parameter> p)
    {
      List<CodeGeneration.Variable> parameters = new List<CodeGeneration.Variable>();
      foreach (Parameter par in p)
      {
        par.Visit(this);
        CodeGeneration.Variable v = this.stack.Pop();
        string original = v.Id;
        bool isRef = original[0] == '*';
        if (isRef)
        {
          original = original.Substring(1);
          v = this.varHandler.DeclareVariable(v.Type, original, 0);
          v.Id = $"*{v.Id}"; // Always use with * in code
        }
        else v = this.varHandler.DeclareVariable(v.Type, original, 0);
        parameters.Add(v);
        // Create additional parameters for array size and string array lengths
        if (IsArray(v.Type))
        {
          // If isRef, make this refs aswell
          CodeGeneration.Variable size = this.varHandler.DeclareVariable(BuiltInType.Integer, null, 0);
          size.OriginalId = size.Id;
          size.IsArrayElement = false;
          if (isRef) size.Id = $"*{size.Id}";
          v.SetSize(size);
          parameters.Add(size);
          if (v.Type == BuiltInType.StringArray)
          {
            CodeGeneration.Variable lengths = this.varHandler.DeclareVariable(BuiltInType.IntegerArray, null, 0);
            lengths.OriginalId = lengths.Id; // OriginalId might also need *
            lengths.IsArrayElement = false;
            if (isRef) lengths.Id = $"*{lengths.Id}";
            v.SetLengths(lengths);
            parameters.Add(lengths);
          }
        }
      }
      return parameters;
    }
    public void VisitFunction(Function f)
    {
      List<CodeGeneration.Variable> parameters = HandleParameters(f.Parameters);
      this.ParameterCheck = true; // disables writing in file
      BuiltInType returnType = f.Type.Visit(this);
      this.ParameterCheck = false;
      int sizeOfArray = 0;
      if (IsArray(returnType))
      {
        // The expression value is stored in ParameterValues
        sizeOfArray = this.ParameterValues.Pop();
        if (sizeOfArray < 0) throw new Error("The size of the array can not be negative!", f.Location, this.reader);
      }
      this.writer.Write($"{TypeInC(returnType)} {f.Name}(");
      int i = 1;
      foreach (CodeGeneration.Variable v in parameters)
      {
        this.writer.Write($"{TypeInC(v.Type)} {v.Id}");
        if (i < parameters.Count) this.writer.Write(",");
        i++;
      }
      if (IsArray(returnType))
      {
        // If the function returns an array, the size and lengths variables are given 
        // to it as parameters
        CodeGeneration.Variable size = this.varHandler.DeclareVariable(BuiltInType.Integer, null, this.block);
        size.OriginalId = size.Id;
        size.Id = $"*{size.Id}";
        size.IsArrayElement = false;
        this.returnSize = size;
        this.writer.Write($"{(parameters.Count > 0 ? "," : "")}int {size.Id}");
        if (returnType == BuiltInType.StringArray)
        {
          CodeGeneration.Variable lengths = this.varHandler.DeclareVariable(BuiltInType.IntegerArray, null, this.block);
          lengths.OriginalId = lengths.Id;
          lengths.Id = $"*{lengths.Id}";
          lengths.IsArrayElement = false;
          this.returnLengths = lengths;
          this.writer.Write($",int* {lengths.Id}");
        }
        // THESE WILL BE ADDED TO THE RETURN VALUE WITH SetSize and SetLengths in VisitReturn
      }
      this.writer.Write("){\n");
      f.Block.Visit(this);
      this.varHandler.RemoveAll();
      this.returnSize = new CodeGeneration.Variable();
      this.returnLengths = new CodeGeneration.Variable();
      this.writer.Write("}\n");
    }
    public void VisitBlock(Block b, bool needsToReturnValue)
    {
      this.block++;
      foreach (Statement stmt in b.statements) stmt.Visit(this);
      // TODO: After block, all the declared variables must be wiped from OriginalId's
      this.varHandler.FreeVariablesDeclaredInBlock(this.block);
      this.block--;
    }
    public void VisitDeclaration(Declaration s)
    {
      // TODO: exprVar could be ArrayElement or Variable
      CodeGeneration.Variable size = new CodeGeneration.Variable(null, null, BuiltInType.Error, this.block);
      if (IsArray(s.BuiltInType))
      {
        s.Type.Visit(this); // Does the C calculation of IntegerExpression, and stores it
        // in the top most variable in the stack
        CodeGeneration.Variable exprVar = this.stack.Pop(); // IntegerExpression might be null
        string value = exprVar.Id;
        if (exprVar.Id == null) value = "0";
        IndexNotNegative(value);
        foreach(Lexer.Token t in s.Identifiers)
        {
          // Create a new temp variable and make that into a ArraySize var
          // TODO: Make this only for IntegerArrays for now
          size = InitializeTempVariable(BuiltInType.Integer);
          // Assign the value of exprVar into the new temp variable
          this.writer.Write($"{size.Id}={value};\n");
          InitializeDeclaredVariable(s.BuiltInType, t.Value, size);
        }
        // After everything, free the exprVar
        this.varHandler.FreeTempVariable(exprVar);
      }
      else
      {
        foreach(Lexer.Token t in s.Identifiers) InitializeDeclaredVariable(s.BuiltInType, t.Value, size);
      }
    }
    private void InitializeDeclaredVariable(BuiltInType type, string originalId, CodeGeneration.Variable size)
    {
      // The declaration. Add the StringLengths (lengths) variable to StringArray variable
      // Use SetSize method for it, it will make the lengths var IsArraySize
      CodeGeneration.Variable v = this.varHandler.GetFreeVariable(type);
      if (v.Id == null)
      {
        // declare new
        v = this.varHandler.DeclareVariable(type, originalId, this.block);
        this.writer.Write($"{TypeInC(type)} ");
        if (type == BuiltInType.IntegerArray)
        {
          v.SetSize(size);
          MallocMemory(v.Id, SizeOf(v));
        }
        else if (type == BuiltInType.StringArray)
        {
          v.SetSize(size);
          MallocMemory(v.Id, SizeOf(v));
          InitializeStringArray(v);
        }
        else if (type == BuiltInType.String)
        {
          // Always malloc memory to a new declared string
          MallocMemory(v.Id, "1");
        }
        else this.writer.Write($"{v.Id};\n");
      }
      else
      {
        if (type == BuiltInType.IntegerArray)
        {
          v.SetSize(size);
          ReallocMemory(v.Id, SizeOf(v));
        }
        else if (type == BuiltInType.StringArray)
        {
          v.SetSize(size);
          ReallocMemory(v.Id, SizeOf(v));
          InitializeStringArray(v);
        }
        v.OriginalId = originalId;
      }
    }
    private void InitializeStringArray(CodeGeneration.Variable arr)
    {
      // Lengths is always malloc'd
      CodeGeneration.Variable lengths = InitializeTempVariable(BuiltInType.IntegerArray, $"sizeof(int)*{arr.Size.Id}");
      this.writer.WriteLine($"InitializeStringArray({arr.Id},{arr.Size.Id},{lengths.Id});");
      arr.SetLengths(lengths);
    }
    public SymbolTableEntry VisitReferenceParameter(ReferenceParameter rp)
    {
      CodeGeneration.Variable v;
      if (IsArray(rp.Type.Type)) v = HandleArrayTypeParameter(rp);
      else v = new CodeGeneration.Variable(rp.Name, rp.Type.Type, 0);
      // Ref adds the * in front of the Id
      v.Id = $"*{v.Id}";
      this.stack.Push(v);
      return new SymbolTableEntry("", BuiltInType.Error);
    }
    public SymbolTableEntry VisitValueParameter(ValueParameter vp)
    {
      CodeGeneration.Variable v;
      if (IsArray(vp.Type.Type)) v = HandleArrayTypeParameter(vp);
      else v = new CodeGeneration.Variable(vp.Name, vp.Type.Type, 0);
      this.stack.Push(v);
      return new SymbolTableEntry("", BuiltInType.Error);
    }
    private CodeGeneration.Variable HandleArrayTypeParameter(Parameter p)
    {
      // During the visit of t.IntegerExpression, there can not be Variables
      this.ParameterCheck = true;
      p.Type.Visit(this); // Value is on top of ParameterValues stack
      this.ParameterCheck = false;
      int i = this.ParameterValues.Pop();
      if (i < 0) throw new Error("The size of the array can not be negative!", p.Location, this.reader);
      // This return Variable is not used. This just contains all the needed values.
      return new CodeGeneration.Variable(p.Name, p.Type.Type, i);
    }
    public BuiltInType VisitArrayType(ArrayType t)
    {
      if (t.IntegerExpression != null) t.IntegerExpression.Visit(this);
      else
      {
        if (this.ParameterCheck) this.ParameterValues.Push(0);
        else this.stack.Push(new CodeGeneration.Variable());
      }
      // After visiting expression, there is the IntegerExpression's value at the
      // top of this.stack
      return t.Type;
    }
    public BuiltInType VisitSimpleType(SimpleType t)
    {
      return t.Type;
    }
    private bool IsOkToAssign(CodeGeneration.Variable v)
    {
      return (v.IsTemporary() && !v.IsArrayElement && !v.IsArraySize);
    }
    public BuiltInType VisitSimpleExpression(SimpleExpression e)
    {
      if (this.ParameterCheck)
      {
        e.Term.Visit(this); // Pushes to ParameterValues stack
        int i = this.ParameterValues.Pop();
        if (e.Sign != null && e.Sign == "-") i = -i;
        foreach (SimpleExpressionAddition a in e.Additions)
        {
          a.Visit(this);
          int j = this.ParameterValues.Pop();
          if (a.AddingOperator == "+") i = i + j;
          if (a.AddingOperator == "-") i = i - j;
        }
        this.ParameterValues.Push(i);
        return BuiltInType.Error;
      }
      // Use the left register for result
      e.Term.Visit(this); // The result is stored on top of stack
    
      if (e.Sign != null && e.Sign == "-")
      {
        CodeGeneration.Variable v = this.stack.Peek();
        // If the variable is temporary (not declared)
        if (IsOkToAssign(v)) this.writer.Write($"{v.Id}=-{v.Id};\n");
        else
        {
          // Otherwise use a new free variable and store result in there
          v = this.stack.Pop();
          // Can not be string
          CodeGeneration.Variable temp = InitializeTempVariable(v.Type);
          this.writer.Write($"{temp.Id}=-{v.Id};\n");
          this.stack.Push(temp);
          // Free the variable or pointer.
          this.varHandler.FreeTempVariable(v);
        }
      }

      foreach(SimpleExpressionAddition a in e.Additions)
      {
        CodeGeneration.Variable termVar = this.stack.Pop();
        a.Visit(this); // Stores the value on top of stack
        // AddingOperator is a.AddingOperator
        CodeGeneration.Variable addVar = this.stack.Pop();
        HandleAddingOperation(termVar, a.AddingOperator, addVar); // Stores result var to stack
      }
      return BuiltInType.Error;
    }
    private void HandleStringAddition(CodeGeneration.Variable v1, CodeGeneration.Variable v2)
    {
      // In this case the TypeChecker has checked that operator is +
      if (v1.Type == BuiltInType.String)
      {
        switch(v2.Type)
        {
          case BuiltInType.String:
          case BuiltInType.IntegerArray:
          case BuiltInType.StringArray: HandleArrayAddition(v1, v2); break;
          case BuiltInType.Boolean:
          case BuiltInType.Integer: HandleStringAndIntegerAddition(v1, v2); break;
          /*case BuiltInType.Real:
          case BuiltInType.Boolean:*/
          default: break;
        }
      }
      else
      {
        // v2.Type is String in this case
        switch(v1.Type)
        {
          case BuiltInType.String:
          case BuiltInType.IntegerArray:
          case BuiltInType.StringArray: HandleArrayAddition(v1, v2); break;
          case BuiltInType.Boolean:
          case BuiltInType.Integer: HandleStringAndIntegerAddition(v1, v2); break;
          /*case BuiltInType.Real:
          case BuiltInType.Boolean:*/
          default: break;
        }
      }
    }
    private CodeGeneration.Variable InitializeTempVariable(BuiltInType type)
    {
      return InitializeTempVariable(type, null);
    }
    private CodeGeneration.Variable InitializeTempVariable(BuiltInType type, string size)
    {
      bool array = IsArray(type);
      CodeGeneration.Variable v = this.varHandler.GetFreeVariable(type);
      if (v.Id == null)
      {
        // declare new
        v = this.varHandler.DeclareVariable(type, this.block);
        this.writer.Write($"{TypeInC(type)} ");
        if (type == BuiltInType.String || array)
        {
          if (size != null) MallocMemory(v.Id, size);
        }
      }
      else if (type == BuiltInType.String || array)
      {
        // When ever assigning a String or array to an existing variable, free the memory
        if (!v.IsArrayElement) this.writer.WriteLine($"free({v.Id});"); // Array elements are not mallocd!
        if (size != null) MallocMemory(v.Id, size);
      }
      return v;
    }
    private CodeGeneration.Variable HandleArrayAddition(CodeGeneration.Variable v1, CodeGeneration.Variable v2)
    {
      // These are also good string sizes
      // This is the only way an array is temporary. They need to be set with Size aswell.
      // Assumes that v1 and v2 have the same type. (Array or String)
      string v1Size = SizeOf(v1);
      string v2Size = SizeOf(v2);
      string nullChar = (v1.Type == BuiltInType.String || v2.Type == BuiltInType.String) ? "+1" : "";

      CodeGeneration.Variable res;
      if (v1.Type == BuiltInType.String && v2.Type == BuiltInType.String)
      {
        res = InitializeTempVariable(BuiltInType.String);
        this.writer.Write($"{res.Id}=ConcatStrings({v1.Id},{v2.Id});\n");
        this.stack.Push(res);
        this.varHandler.FreeTempVariable(v1);
        this.varHandler.FreeTempVariable(v2);
        return res;
      }
      else if (v1.Type == BuiltInType.IntegerArray && v2.Type == BuiltInType.IntegerArray)
      {
        // Calculate the size var
        this.writer.WriteLine("/**** Start of concatenating integerarrays *****/");

        res = InitializeTempVariable(BuiltInType.IntegerArray);
        this.writer.Write($"{res.Id}=ConcatIntegerArrays({v1.Id},{v2.Id},{v1.Size.Id},{v2.Size.Id});\n");
        CodeGeneration.Variable sizeVar = InitializeTempVariable(BuiltInType.Integer);
        this.writer.Write($"{sizeVar.Id}={v1.Size.Id}+{v2.Size.Id};\n");
        res.SetSize(sizeVar);
        this.varHandler.FreeTempVariable(v1);
        this.varHandler.FreeTempVariable(v2);
        this.stack.Push(res);
        this.writer.WriteLine("/**** End of concatenating integerarrays *****/");
        return res;
      }
      else if (v1.Type == BuiltInType.IntegerArray || v2.Type == BuiltInType.IntegerArray)
      {
        // Other one is string in this case
        CodeGeneration.Variable intArr = v1;
        if (v1.Type == BuiltInType.String) intArr = v2;
        CodeGeneration.Variable temp = ConvertIntegerArrayToString(intArr);
        if (v1.Type == BuiltInType.String) HandleArrayAddition(v1, temp);
        else HandleArrayAddition(temp, v2);
        this.varHandler.FreeTempVariable(v1);
        this.varHandler.FreeTempVariable(v2);
        return this.stack.Peek();
      }
      else if (v1.Type == BuiltInType.StringArray && v2.Type == BuiltInType.StringArray)
      {
        res = InitializeTempVariable(BuiltInType.StringArray);
        this.writer.Write($"{res.Id}=ConcatStringArrays({v1.Id},{v2.Id},{v1.Size.Id},{v2.Size.Id},{v1.Lengths.Id},{v2.Lengths.Id});\n");
        CodeGeneration.Variable size = InitializeTempVariable(BuiltInType.Integer);
        this.writer.Write($"{size.Id}={v1.Size.Id}+{v2.Size.Id};\n");
        res.SetSize(size);
        CodeGeneration.Variable lengths = InitializeTempVariable(BuiltInType.IntegerArray);
        // TODO: CONCAT IS NOT ENOUGH BECAUSE THEY ARE NOW OFFSETS
        this.writer.Write($"{lengths.Id}=CountNewOffsets({res.Id},{v1.Lengths.Id},{v2.Lengths.Id},{v1.Size.Id},{v2.Size.Id});\n");
        res.SetLengths(lengths);
        this.stack.Push(res);
        return res;
      }
      else if (v1.Type == BuiltInType.StringArray || v2.Type == BuiltInType.StringArray)
      {
        // Other one is string
        CodeGeneration.Variable strArr = v1;
        if (v1.Type == BuiltInType.String) strArr = v2;
        CodeGeneration.Variable temp = ConvertStringArrayToString(strArr);
        if (v1.Type == BuiltInType.String) res = HandleArrayAddition(v1, temp);
        else res = HandleArrayAddition(temp, v2);
        this.varHandler.FreeTempVariable(v1);
        this.varHandler.FreeTempVariable(v2);
        return res;
      }
      else
      {
        res = new CodeGeneration.Variable(null, null, BuiltInType.Error, this.block);
        return res;
      }
    }
    private CodeGeneration.Variable ConvertStringArrayToString(CodeGeneration.Variable sa)
    {
      // [ "string1", "string2", "string3" ]
      CodeGeneration.Variable v = InitializeTempVariable(BuiltInType.String);
      this.writer.Write($"{v.Id}=StringArrayToString({sa.Id},{sa.Size.Id},{sa.Lengths.Id});\n");
      return v;
    }
    private CodeGeneration.Variable ConvertIntegerArrayToString(CodeGeneration.Variable ia)
    {
      // var arr : array[3] of integer; array[0] = 1; array[1] = 2; array[2] = 3;
      // "string " + arr = "string [1,2,3]"

      CodeGeneration.Variable v = InitializeTempVariable(BuiltInType.String);
      this.writer.Write($"{v.Id}=IntegerArrayToString({ia.Id},{ia.Size.Id});\n");
      return v;
    }
    private string SizeOf(CodeGeneration.Variable v)
    {
      if (v.Type == BuiltInType.String) return $"strlen({v.Id})";
      else if (v.Type == BuiltInType.IntegerArray) return $"sizeof(int)*{v.Size.Id}";
      else if (v.Type == BuiltInType.StringArray)
      {
        // Need to Count the length of StringArray
        return $"sizeof(char)*{v.Size.Id}";
      }
      return $"sizeof({v.Id})";
    }
    private CodeGeneration.Variable ConvertIntegerToString(CodeGeneration.Variable i)
    {
      CodeGeneration.Variable res = InitializeTempVariable(BuiltInType.String, "1");
      this.writer.WriteLine($"IntegerToStringWithSizeCalc({i.Id},&{res.Id});");
      return res;
    }
    private void HandleStringAndIntegerAddition(CodeGeneration.Variable v1, CodeGeneration.Variable v2)
    {
      // These sizes for strings are also good
      // Find out which one is the Integer
      CodeGeneration.Variable strVar = v1;
      CodeGeneration.Variable integer = v2;
      if (v1.Type == BuiltInType.Integer || v1.Type == BuiltInType.Boolean)
      {
        integer = v1;
        strVar = v2;
      }
      CodeGeneration.Variable res = ConvertIntegerToString(integer);

      // Can now free the integer variable
      this.varHandler.FreeTempVariable(integer);
      if (v1.Type == BuiltInType.String) HandleArrayAddition(v1, res);
      else HandleArrayAddition(res, v2);
    }
    private void HandleSimpleAddingOperation(CodeGeneration.Variable v1, string op, CodeGeneration.Variable v2)
    {
      CodeGeneration.Variable dest = v1;
      // Types can be Integer, Real, String or any array
      if (v1.Type == BuiltInType.String || v2.Type == BuiltInType.String)
      {
        // Operation must be + (TypeCheckVisitor)
        HandleStringAddition(v1, v2);
      }
      else if (IsArray(v1.Type))
      {
        // Handle array additions. (Can only be + and same array type)
        HandleArrayAddition(v1, v2);
      }
      else if (v1.Type == BuiltInType.Integer && v2.Type == BuiltInType.Integer)
      {
        HandleIntegerAndIntegerAddition(v1, op, v2);
      }
    }
    private void HandleIntegerAndIntegerAddition(CodeGeneration.Variable v1, string op, CodeGeneration.Variable v2)
    {
      CodeGeneration.Variable dest = v1;
      if (IsOkToAssign(v1))
      // if (v1.IsTemporary() && !v1.IsArrayElement)
      {
        this.writer.Write($"{v1.Id}={v1.Id}{op}{v2.Id};\n");
      }
      else
      {
        dest = InitializeTempVariable(BuiltInType.Integer);
        this.writer.Write($"{dest.Id}={v1.Id}{op}{v2.Id};\n");
        this.varHandler.FreeTempVariable(v1);
      }
      this.stack.Push(dest);
      // Do not free v because it is needed
      this.varHandler.FreeTempVariable(v2);
    }
    private void HandleOr(CodeGeneration.Variable v1, CodeGeneration.Variable v2)
    {
      CodeGeneration.Variable res = InitializeTempVariable(BuiltInType.Boolean);;
      this.writer.Write($"{res.Id}={v1.Id}||{v2.Id};\n");
      this.varHandler.FreeTempVariable(v1);
      this.varHandler.FreeTempVariable(v2);
      this.stack.Push(res);
    }
    public BuiltInType VisitBooleanExpression(BooleanExpression e)
    {
      e.Left.Visit(this); // Stores on stack
      CodeGeneration.Variable left = this.stack.Pop();
      e.Right.Visit(this);
      CodeGeneration.Variable right = this.stack.Pop();
      CodeGeneration.Variable res = HandleRelationalOperation(left, e.RelationalOperator, right);
      this.stack.Push(res);
      this.varHandler.FreeTempVariable(left);
      this.varHandler.FreeTempVariable(right);
      return BuiltInType.Error;
    }
    private CodeGeneration.Variable HandleRelationalOperation(CodeGeneration.Variable l, string o, CodeGeneration.Variable r)
    {
      // <relational operator> ::= "=" | "<>" | "<" | "<=" | ">=" | ">"
      // TODO: Make HandleEqualsOperation() that does operations for strings for example
      switch (o)
      {
        case "=": return HandleEqualsOperation(l, r, false);
        case "<>": return HandleEqualsOperation(l, r, true);
        case "<":
        case ">":
        case "<=":
        case ">=":
          CodeGeneration.Variable v = InitializeTempVariable(BuiltInType.Boolean);
          this.writer.Write($"{v.Id}={l.Id}{o}{r.Id};\n");
          return v;
        default: break;
      }
      return new CodeGeneration.Variable();
    }
    private CodeGeneration.Variable HandleEqualsOperation(CodeGeneration.Variable l, CodeGeneration.Variable r, bool not)
    {
      CodeGeneration.Variable res = InitializeTempVariable(BuiltInType.Boolean);
      switch (l.Type)
      {
        case BuiltInType.Integer:
        case BuiltInType.Boolean:
          this.writer.Write($"{res.Id}={l.Id}{(not ? "!=" : "==")}{r.Id};\n");
          break;
        case BuiltInType.String:
          this.writer.Write($"{res.Id}={(not ? "!" : "")}CompareStrings({l.Id},{r.Id});\n");
          break;
        default: break;
      }
      return res;
    }
    public BuiltInType VisitClosedExpression(ClosedExpression e)
    {
      if (this.ParameterCheck)
      {
        e.Expression.Visit(this);
        return BuiltInType.Error;
      }
      e.Expression.Visit(this);
      if (e.Size)
      {
        CodeGeneration.Variable exprVar = this.stack.Pop();
        CodeGeneration.Variable v = CreateSizeVariable(exprVar);
        // Free exprVar if temporary
        this.varHandler.FreeTempVariable(exprVar);
        this.stack.Push(v);
      }
      return BuiltInType.Error;
    }
    private CodeGeneration.Variable CreateSizeVariable(CodeGeneration.Variable v)
    {
      CodeGeneration.Variable size;
      if (IsArray(v.Type)) size = v.Size;
      else
      {
        // String
        // Create a new temp integer variable and store the size of string to there
        CodeGeneration.Variable temp = InitializeTempVariable(BuiltInType.Integer);
        this.writer.Write($"{temp.Id}=(int)({SizeOf(v)});\n");
        size = temp;
      }
      return size;
    }
    public BuiltInType VisitSimpleExpressionAddition(SimpleExpressionAddition e)
    {
      e.Term.Visit(this);
      return BuiltInType.Error;
    }
    public BuiltInType VisitTerm(Term t)
    {
      t.Factor.Visit(this);
      return BuiltInType.Error;
    }
    public BuiltInType VisitTermMultiplicative(TermMultiplicative t)
    {
      return BuiltInType.Error;
    }
    public BuiltInType VisitIntegerLiteral(IntegerLiteral l)
    {
      if (this.ParameterCheck)
      {
        try
        {
          int i = System.Convert.ToInt32(l.Value);
          this.ParameterValues.Push(i);
        }
        catch (System.Exception)
        {
          throw new Error("Integer is not valid!", l.Location, this.reader);
        }
        return BuiltInType.Error;
      }
      // Always store in variable
      CodeGeneration.Variable v = InitializeTempVariable(BuiltInType.Integer);
      this.stack.Push(v); // Push the variable into stack
      this.writer.Write($"{v.Id}={l.Value};\n");
      return BuiltInType.Error;
    }
    public BuiltInType VisitStringLiteral(StringLiteral l)
    {
      // This is a good size for string (checked)
      CodeGeneration.Variable v;
      if (l.Size)
      {
        // TODO: CreateSizeVariable() ?
        CodeGeneration.Variable temp = InitializeTempVariable(BuiltInType.Integer);
        this.writer.Write($"{temp.Id}={l.Value.Length-2};\n");
        v = temp;
      }
      else
      {
        v = InitializeTempVariable(BuiltInType.String); // Without allocatin memory
        this.writer.Write($"{v.Id}=MakeStringVar({l.Value});\n");
      }
      this.stack.Push(v);
      return BuiltInType.Error;
    }
    public BuiltInType VisitRealLiteral(RealLiteral l)
    {
      return BuiltInType.Error;
    }
    public BuiltInType VisitNegationFactor(NegationFactor f)
    {
      // TODO: Handle Size
      f.Factor.Visit(this);
      CodeGeneration.Variable v = this.stack.Pop();
      CodeGeneration.Variable r = InitializeTempVariable(BuiltInType.Boolean);
      this.writer.Write($"{r.Id}=!{v.Id};\n");
      this.varHandler.FreeTempVariable(v);
      this.stack.Push(r);
      return BuiltInType.Error;
    }
    private BuiltInType ElementTypeOfArray(BuiltInType t)
    {
      switch(t)
      {
        case BuiltInType.IntegerArray: return BuiltInType.Integer;
        case BuiltInType.StringArray: return BuiltInType.String;
        case BuiltInType.BooleanArray: return BuiltInType.Boolean;
        case BuiltInType.RealArray: return BuiltInType.Real;
        default: return BuiltInType.Error;
      }
    }
    private CodeGeneration.Variable GetElementFromStringArray(CodeGeneration.Variable arr, CodeGeneration.Variable i)
    {
      // TODO: Make GetElementFromStringArray return the char* pointer
      CodeGeneration.Variable s = InitializeTempVariable(BuiltInType.String);
      this.writer.Write($"{s.Id}=GetElementFromStringArray({arr.Id},{i.Id},{arr.Lengths.Id});\n");
      return s;
    }
    public BuiltInType VisitVariable(Variable v)
    {
      if (this.ParameterCheck) throw new Error("Variables are not allowed in function/procedure headers' array expressions!", v.Location, this.reader);
      CodeGeneration.Variable variable;
      if (v.IntegerExpression != null)
      {
        CodeGeneration.Variable array = this.varHandler.GetVariable(v.Name, v.Type);
        v.IntegerExpression.Visit(this); // Stores the value on top of stack
        CodeGeneration.Variable exprVar = this.stack.Pop();
        HandleIndexErrors(array, exprVar);
        this.varHandler.FreeTempVariable(exprVar);
        if (v.LHS)
        {
          if (v.Type == BuiltInType.StringArray) variable = new CodeGeneration.Variable($"*({array.Id}+{exprVar.Id})", ElementTypeOfArray(v.Type), array, exprVar.Id, this.block);
          else variable = new CodeGeneration.Variable($"*({array.Id}+{exprVar.Id})", ElementTypeOfArray(v.Type), this.block);
        }
        else
        {
          if (v.Type == BuiltInType.StringArray)
          {
            // Handle getting the value from string array here
            variable = GetElementFromStringArray(array, exprVar);
          }
          else
          {
            CodeGeneration.Variable temp = InitializeTempVariable(ElementTypeOfArray(v.Type));
            this.writer.Write($"{temp.Id}=*({array.Id}+{exprVar.Id});\n");
            variable = temp;
          }
        }
      }
      else variable = this.varHandler.GetVariable(v.Name, v.Type);

      if (v.Size)
      {
        variable = CreateSizeVariable(variable);
      }
      this.stack.Push(variable);
      return BuiltInType.Error;
    }
    private void HandleIndexErrors(CodeGeneration.Variable array, CodeGeneration.Variable index)
    {
      IndexInBounds(index.Id, array.Size.Id);
    }
    private void IndexInBounds(string index, string arraySize)
    {
      this.writer.Write($"IndexInBounds({index},{arraySize});\n");
    }
    private void IndexNotNegative(string index)
    {
      this.writer.Write($"NegativeIndex({index});\n");
    }
    public BuiltInType VisitCall(Call c)
    {
      CodeGeneration.Variable v = new CodeGeneration.Variable();
      c.Arguments.Visit(this);
      List<CodeGeneration.Variable> arguments = new List<CodeGeneration.Variable>();
      int i = 1;
      string argumentsAsString = "";
      foreach(BuiltInType t in c.Arguments.Types)
      {
        bool isRef = c.Arguments.Refs.Contains(i-1);
        // TODO: Use the Arguments.Refs list to check which are refs, adn add & infront of those
        // TODO: Handle ref parameters (add & infront)
        CodeGeneration.Variable a = this.stack.Pop();
        argumentsAsString += $"{(isRef ? "&" : "")}{a.Id}";
        arguments.Add(a);
        if (IsArray(t))
        {
          argumentsAsString += $",{(isRef ? "&" : "")}{a.Size.Id}";
          if (t == BuiltInType.StringArray) argumentsAsString += $",{(isRef ? "&" : "")}{a.Lengths.Id}";
        }
        if (i < c.Arguments.Types.Count) argumentsAsString += ",";
        i++;
      }
      if (c.Type != BuiltInType.Void)
      {
        CodeGeneration.Variable size = new CodeGeneration.Variable();
        CodeGeneration.Variable lengths = new CodeGeneration.Variable();
        if (IsArray(c.Type))
        {
          // Function that returns an array. Need to initialize size (and possibly lengths).
          // And pass them as reference parameters to the function.
          size = InitializeTempVariable(BuiltInType.Integer);
          this.writer.Write($"{size.Id}=0;\n");
          argumentsAsString += $"{(c.Arguments.Types.Count > 0 ? "," : "")}&{size.Id}";
          if (c.Type == BuiltInType.StringArray)
          {
            lengths = InitializeTempVariable(BuiltInType.IntegerArray, "sizeof(int)");
            argumentsAsString += $",&{lengths.Id}";
          }
        }
        // Initialize variable that will store the return value
        v = InitializeTempVariable(c.Type);
        if (size.Id != null) v.SetSize(size);
        if (lengths.Id != null) v.SetLengths(lengths);
        this.writer.Write($"{v.Id}=");
      }
      this.writer.Write($"{c.Name}({argumentsAsString});\n");
      // TODO: Handle Size
      if (c.Size) v = CreateSizeVariable(v);
      foreach (CodeGeneration.Variable a in arguments) this.varHandler.FreeTempVariable(a);
      this.stack.Push(v);
      return BuiltInType.Error;
    }
    public List<BuiltInType> VisitArguments(Arguments a)
    {
      if (a.Expressions.Count == 0) return new List<BuiltInType>();
      // Loop the list in reversed order to store the values to stack in correct order
      for (int i = a.Expressions.Count - 1; i >= 0; i--)
      {
        a.Expressions[i].Visit(this);
      }
      return new List<BuiltInType>();
    }
    public void VisitAssertStatement(AssertStatement s){}
    private void AssignStringToStringArray(CodeGeneration.Variable arrElement, CodeGeneration.Variable str)
    {
      this.writer.Write($"AssignStringToStringArray(&{arrElement.ElementOf.Id},{arrElement.Index},{str.Id},{arrElement.ElementOf.Size.Id},{arrElement.ElementOf.Lengths.Id});\n");
    }
    public void VisitAssignmentStatement(AssignmentStatement s)
    {
      s.Expression.Visit(this); // stores the value to top most variable in stack
      s.Variable.Visit(this); //  stores on top of stack
      CodeGeneration.Variable varVar = this.stack.Pop();
      CodeGeneration.Variable exprVar = this.stack.Pop();
      if (varVar.Type == BuiltInType.String)
      {
        if (varVar.IsArrayElement)
        {
          // Handle Assigning to string arrayElement s[1] := "something"
          AssignStringToStringArray(varVar, exprVar);
        }
        else
        {
          this.writer.WriteLine($"CopyCharPointer(&{varVar.Id},{exprVar.Id},(int)(strlen({exprVar.Id})+1));");
        }
      }
      else if (IsArray(varVar.Type))
      {
        // Assumes that the exprVar is the same type (TypeCheckVisitor)
        CopyArray(varVar, exprVar, null);
      }
      else if (varVar.Type == BuiltInType.Integer)
      {
        if (!varVar.IsArrayElement)
        {
          SimpleAssignment(varVar.Id, exprVar.Id);
        }
        else
        {
          // Handle assigning to integer array element i[1] := 2;
          SimpleAssignment(varVar.Id, exprVar.Id);
        }
      }
      else if (varVar.Type == BuiltInType.Boolean) SimpleAssignment(varVar.Id, exprVar.Id);
      // Can now free the exprVar.
      this.varHandler.FreeTempVariable(exprVar);
      // Free the left-hand side if it is a pointer (stored in the original location anyways)
      this.varHandler.FreeTempVariable(varVar);
    }
    private void CopyArray(CodeGeneration.Variable dest, CodeGeneration.Variable src, string s)
    {
      switch (src.Type)
      {
        case BuiltInType.IntegerArray:
          string used = src.Size.Id;
          if (used == null)
          {
            // This is Lengths array, they have no Size variable. Must send as paramater in that case.
            used = s;
          }
          this.writer.WriteLine($"CopyIntegerPointer(&{dest.Id},{src.Id},{used});");
          break;
        case BuiltInType.StringArray:
          CodeGeneration.Variable size = SizeOfStringArrayInBytes(src);
          this.varHandler.FreeTempVariable(size);
          this.writer.WriteLine($"CopyCharPointer(&{dest.Id},{src.Id},{size.Id});");
          break;
        default: break;
      }
      // Update the Size attribute of lhs array, by assigning it a new value
      if (src.Size.Id != null) this.writer.Write($"{dest.Size.Id}={src.Size.Id};\n"); // Not set for Lengths arrays
      if (src.Type == BuiltInType.StringArray)
      {
        CopyArray(dest.Lengths, src.Lengths, src.Size.Id);
      }
    }
    private CodeGeneration.Variable SizeOfStringArrayInBytes(CodeGeneration.Variable a)
    {
      // int SizeOfStringArrayInBytes(int s,char* a,int* o){
      CodeGeneration.Variable result = InitializeTempVariable(BuiltInType.Integer);
      this.writer.Write($"{result.Id}=SizeOfStringArrayInBytes({a.Size.Id},{a.Id},{a.Lengths.Id});\n");
      // result contains the total length
      return result;
    }
    public void VisitIfStatement(IfStatement s)
    {
      s.BooleanExpression.Visit(this);
      CodeGeneration.Variable b = this.stack.Pop();
      bool elsePresent = s.ElseStatement != null;
      string elseLabel = this.labelGenerator.GenerateLabel();
      string endLabel = this.labelGenerator.GenerateLabel();
      if (elsePresent)
      {
        this.writer.Write($"if(!{b.Id}) goto {elseLabel};\n");
        s.ThenStatement.Visit(this);
        this.writer.Write($"goto {endLabel};\n");
        this.writer.Write($"{elseLabel}:;\n");
        s.ElseStatement.Visit(this);
        this.writer.Write($"{endLabel}:;\n");
      }
      else
      {
        this.writer.Write($"if(!{b.Id}) goto {endLabel};\n");
        s.ThenStatement.Visit(this);
        this.writer.Write($"{endLabel}:;\n");
      }
      this.varHandler.FreeTempVariable(b);
    }
    public void VisitReadStatement(ReadStatement s)
    {
      List<CodeGeneration.Variable> scanned = new List<CodeGeneration.Variable>();
      foreach (Variable v in s.Variables)
      {
        v.Visit(this);
        CodeGeneration.Variable vari = this.stack.Pop();
        // Can only read strings, integers and reals
        string format = CreateFormatString(vari.Type);
        this.writer.Write($"scanf(\"{format}\",&{vari.Id});\n");
        scanned.Add(vari);
      }
      foreach (CodeGeneration.Variable v in scanned) this.varHandler.FreeTempVariable(v);
    }
    public void VisitReturnStatement(ReturnStatement s)
    {
      string retVal = "0"; // In case of Void returns 0
      if (s.Expression != null)
      {
        s.Expression.Visit(this);
        CodeGeneration.Variable expr = this.stack.Pop();
        retVal = expr.Id;
        if (IsArray(expr.Type))
        {
          // Copy the Size variable to this.returnSize
          this.writer.Write($"{this.returnSize.Id}={expr.Size.Id};\n");
          // Copy the Lenghts if StringArray
          if (expr.Type == BuiltInType.StringArray) this.writer.WriteLine($"CopyIntegerPointer(&{this.returnLengths.Id},{expr.Lengths.Id},{expr.Size.Id});");
        }
        this.varHandler.FreeTempVariable(expr);
      }
      this.writer.Write($"return {retVal};\n");
    }
    public void VisitWhileStatement(WhileStatement s)
    {
      string loop = this.labelGenerator.GenerateLabel();
      string end = this.labelGenerator.GenerateLabel();

      this.writer.Write($"{loop}:;\n");
      s.BooleanExpression.Visit(this);
      CodeGeneration.Variable b = this.stack.Pop();
      this.writer.Write($"if(!{b.Id}) goto {end};\n");
      s.Statement.Visit(this);
      this.writer.Write($"goto {loop};\n");
      this.writer.Write($"{end}:;\n");
      this.varHandler.FreeTempVariable(b);
    }
    public void VisitWriteStatement(WriteStatement s)
    {
      List<CodeGeneration.Variable> printedVars = new List<CodeGeneration.Variable>();
      if (s.Arguments != null)
      {
        s.Arguments.Visit(this); // Stores Arguments count amount of values to stack
        string format = "\"";
        string varString = "";
        foreach(BuiltInType t in s.Arguments.Types)
        {
          CodeGeneration.Variable v = this.stack.Pop();
          if (v.Type == BuiltInType.IntegerArray) v = ConvertIntegerArrayToString(v);
          else if (v.Type == BuiltInType.StringArray) v = ConvertStringArrayToString(v);
          format += CreateFormatString(v.Type);
          varString += $",{v.Id}";
          printedVars.Add(v);
        }
        format += "\\n\"";
        // Print newline at the end
        this.writer.Write($"printf({format}{varString});\n");
        // After printing, free the temporary variables
        foreach (CodeGeneration.Variable p in printedVars) this.varHandler.FreeTempVariable(p);
      }
      else this.writer.Write("printf(\"\\n\");\n");
    }

    private void HandleAddingOperation(CodeGeneration.Variable v1, string op, CodeGeneration.Variable v2)
    {
      switch(op)
      {
        case "+":
        case "-": HandleSimpleAddingOperation(v1, op, v2); break;
        case "or": HandleOr(v1, v2); break;
        default: break;
      }
    }
    private void SimpleAssignment(string to, string value)
    {
      this.writer.Write($"{to}={value};\n");
    }
    private void CopyMemory(string dest, string src, string size)
    {
      this.writer.Write($"memcpy({dest},{src},{size});\n");
    }
    private void CopyStringVariable(string to, string from)
    {
      this.writer.Write($"strcpy({to},{from});\n");
    }
    private void ReallocMemory(string target, string value)
    {
      this.writer.Write($"{target}=realloc({target},{value});\n");
    }
    private void MallocMemory(string target, string value)
    {
      this.writer.Write($"{target}=malloc({value});\n");
    }
    private string TypeInC(BuiltInType t)
    {
      switch(t)
      {
        case BuiltInType.Boolean:
        case BuiltInType.Integer: return "int";
        case BuiltInType.StringArray:
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
    private string CreateFormatString(BuiltInType t)
    {
      switch(t)
      {
        case BuiltInType.Boolean:
        case BuiltInType.Integer: return "%d";
        case BuiltInType.String: return "%s";
        default: return "";
      }
    }
  }
}
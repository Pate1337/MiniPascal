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
    private CodeGeneration.LabelGenerator labelGenerator;
    public GeneratorVisitor(IOHandler io, Reader reader, FileWriter writer)
    {
      this.io = io;
      this.reader = reader;
      this.writer = writer;
      this.varHandler = new CodeGeneration.VariableHandler();
      this.stack = new Stack<CodeGeneration.Variable>();
      this.labelGenerator = new CodeGeneration.LabelGenerator();
    }
    public void VisitProgram(ProgramNode p)
    {
      // foreach (Procedure procedure in p.Procedures) procedure.Visit(this);
      // foreach (Function f in p.Functions) f.Visit(this);
      p.Block.Visit(this);
    }
    public void VisitProcedure(Procedure p){}
    public void VisitFunction(Function f){}
    public void VisitBlock(Block b, bool needsToReturnValue)
    {
      foreach (Statement stmt in b.statements) stmt.Visit(this);
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
      // TODO: exprVar could be ArrayElement or Variable
      CodeGeneration.Variable size = new CodeGeneration.Variable(null, null, BuiltInType.Error);
      if (IsArray(s.BuiltInType))
      {
        s.Type.Visit(this); // Does the C calculation of IntegerExpression, and stores it
        // in the top most variable in the stack
        CodeGeneration.Variable exprVar = this.stack.Pop();
        IndexNotNegative(exprVar.Id);
        foreach(Lexer.Token t in s.Identifiers)
        {
          // Create a new temp variable and make that into a ArraySize var
          // TODO: Make this only for IntegerArrays for now
          size = InitializeTempVariable(BuiltInType.Integer);
          // Assign the value of exprVar into the new temp variable
          this.writer.Write($"{size.Id}={exprVar.Id};\n");
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
        v = this.varHandler.DeclareVariable(type, originalId);
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
      return new SymbolTableEntry("", BuiltInType.Error);
    }
    public SymbolTableEntry VisitValueParameter(ValueParameter vp)
    {
      return new SymbolTableEntry("", BuiltInType.Error);
    }
    public BuiltInType VisitArrayType(ArrayType t)
    {
      t.IntegerExpression.Visit(this);
      // After visiting expression, there is the IntegerExpression's value at the
      // top of this.stack
      return BuiltInType.Error;
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
          System.Console.WriteLine("This should not be String: " + v.Type);
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
          case BuiltInType.String: HandleArrayAddition(v1, v2); break;
          case BuiltInType.Integer: HandleStringAndIntegerAddition(v1, v2); break;
          case BuiltInType.IntegerArray: HandleArrayAddition(v1, v2); break;
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
          case BuiltInType.String: HandleArrayAddition(v1, v2); break;
          case BuiltInType.Integer: HandleStringAndIntegerAddition(v1, v2); break;
          case BuiltInType.IntegerArray: HandleArrayAddition(v1, v2); break;
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
      System.Console.WriteLine($"Got the free var {v.Id}");
      if (v.Id == null)
      {
        // declare new
        v = this.varHandler.DeclareVariable(type);
        this.writer.Write($"{TypeInC(type)} ");
        if (type == BuiltInType.String || array)
        {
          if (size != null) MallocMemory(v.Id, size);
        }
      }
      else if (type == BuiltInType.String || array)
      {
        // When ever assigning a String or array to an existing variable, free the memory
        this.writer.WriteLine($"free({v.Id});");
        if (size != null) MallocMemory(v.Id, size);
        // else this.writer.Write($"free({v.Id});\n");
      }
      return v;
    }
    /*
    private void InitializeDeclaredVariable(BuiltInType type, string originalId, CodeGeneration.Variable size)
    {
      // The declaration. Add the StringLengths (lengths) variable to StringArray variable
      // Use SetSize method for it, it will make the lengths var IsArraySize
      CodeGeneration.Variable v = this.varHandler.GetFreeVariable(type);
      if (v.Id == null)
      {
        // declare new
        v = this.varHandler.DeclareVariable(type, originalId);
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
    */
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
        //res = InitializeTempVariable(BuiltInType.String, $"{v1Size}+{v2Size}{nullChar}");
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
        /*CodeGeneration.Variable sizeVar = InitializeTempVariable(BuiltInType.Integer);
        this.writer.Write($"{sizeVar.Id}={v1.Size.Id}+{v2.Size.Id};\n");
        res = InitializeTempVariable(BuiltInType.IntegerArray, $"sizeof(int)*{sizeVar.Id}");
        res.SetSize(sizeVar);

        CopyMemory(res.Id, v1.Id, v1Size);
        CopyMemory($"{res.Id}+{v1.Size.Id}", v2.Id, $"{v2Size}{nullChar}");*/
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
        // char* ConcatStringArrays(char* a1,char* a2,int s1,int s2,int* l1,int* l2){
        res = InitializeTempVariable(BuiltInType.StringArray);
        this.writer.Write($"{res.Id}=ConcatStringArrays({v1.Id},{v2.Id},{v1.Size.Id},{v2.Size.Id},{v1.Lengths.Id},{v2.Lengths.Id});\n");
        CodeGeneration.Variable size = InitializeTempVariable(BuiltInType.Integer);
        this.writer.Write($"{size.Id}={v1.Size.Id}+{v2.Size.Id};\n");
        res.SetSize(size);
        CodeGeneration.Variable lengths = InitializeTempVariable(BuiltInType.IntegerArray);
        // writer.WriteLine("int* ConcatIntegerArrays(int* ia1,int* ia2,int s1,int s2){
        // TODO: CONCAT IS NOT ENOUGH BECAUSE THEY ARE NOW OFFSETS
        this.writer.Write($"{lengths.Id}=CountNewOffsets({res.Id},{v1.Lengths.Id},{v2.Lengths.Id},{v1.Size.Id},{v2.Size.Id});\n");
        // this.writer.Write($"{lengths.Id}=ConcatIntegerArrays({v1.Lengths.Id},{v2.Lengths.Id},{v1.Size.Id},{v2.Size.Id});\n");
        res.SetLengths(lengths);
        // res = new CodeGeneration.Variable(null, null, BuiltInType.Error);
        this.stack.Push(res);
        return res;
      }
      /*else if (v1.Type == BuiltInType.StringArray || v2.Type == BuiltInType.StringArray)
      {
        // Other one is string
        CodeGeneration.Variable strArr = v1;
        if (v1.Type == BuiltInType.String) strArr = v2;
        CodeGeneration.Variable temp = ConvertStringArrayToString(strArr);
        if (v1.Type == BuiltInType.String) HandleArrayAddition(v1, temp);
        else HandleArrayAddition(temp, v2);
        this.varHandler.FreeTempVariable(v1);
        this.varHandler.FreeTempVariable(v2);
        return this.stack.Peek();
      }*/
      else
      {
        res = new CodeGeneration.Variable(null, null, BuiltInType.Error);
        return res;
      }
      
     /* if (res.Type == BuiltInType.String)
      {
        // Both are strings is this case
        this.writer.Write($"sprintf({res.Id},\"%s\",{v1.Id});\n");
        this.writer.Write($"sprintf({res.Id}+{v1Size},\"%s\",{v2.Id});\n");
      }
      else
      {
        // Copy the memory from v1.Id to res
        CopyMemory(res.Id, v1.Id, v1Size);
        CopyMemory($"{res.Id}+{v1.Size.Id}", v2.Id, $"{v2Size}{nullChar}");
        // Copy the memory from v2.Id to res + strlen(v2.Id)
        // if (v1.Type != BuiltInType.String) CopyMemory($"{res.Id}+{v1.Size.Id}", v2.Id, $"{v2Size}{nullChar}");
        // else CopyMemory($"{res.Id}+{v1Size}", v2.Id, $"{v2Size}{nullChar}");
      }

      // Free v1 and v2 (FreeTempVariable only frees them if strings)
      this.varHandler.FreeTempVariable(v1);
      this.varHandler.FreeTempVariable(v2);
      // Push res to stack
      this.stack.Push(res);
      return res;*/
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
      // this.writer.Write("/*********** Start of converting an Integer array to String **********/\n");
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
      if (v1.Type == BuiltInType.Integer)
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
    private void SimpleAddingOperation(string left, string op, string right)
    {
      // The result is stored to the left variable
      this.writer.Write($"{left}={left}{op}{right};\n");
    }
    private void HandleOr(CodeGeneration.Variable v1, CodeGeneration.Variable v2)
    {

    }
    public BuiltInType VisitBooleanExpression(BooleanExpression e)
    {
      return BuiltInType.Error;
    }
    public BuiltInType VisitClosedExpression(ClosedExpression e)
    {
      /*
      public string Style { get; set; }
      public bool Size { get; set; }
      public Expression Expression { get; set; }
      public Location SizeLocation { get; set; }
      // Location of LeftParenthesis
      public Location Location { get; set; }
      public BuiltInType Type { get; set; }
      */
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
      /*
      public string AddingOperator { get; set; }
      public Term Term { get; set; }
      public string Style { get; set; }
      public Location Location { get; set; }
      */
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
      // Always store in variable
      CodeGeneration.Variable v = InitializeTempVariable(BuiltInType.Integer);
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
        // MakeStringVar(string size, string val)
        // v = InitializeTempVariable(BuiltInType.String, $"{l.Value.Length-1}");
        // CopyStringVariable(v.Id, l.Value);
        v = InitializeTempVariable(BuiltInType.String); // Without allocatin memory
        this.writer.Write($"{v.Id}=MakeStringVar({l.Value});\n");
      }
      this.stack.Push(v);
      return BuiltInType.Error;
    }
    /*private CodeGeneration.Variable MakeStringVariable(string value)
    {
      CodeGeneration.Variable s = InitializeTempVariable(BuiltInType.String)
    }*/
    public BuiltInType VisitRealLiteral(RealLiteral l)
    {
      return BuiltInType.Error;
    }
    public BuiltInType VisitNegationFactor(NegationFactor f)
    {
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
      // this.writer.Write($"GetElementFromStringArray(&{s.Id},{arr.Id},{i.Id},{arr.Lengths.Id});\n");
      return s;
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
      CodeGeneration.Variable variable;
      if (v.IntegerExpression != null)
      {
        // Returns a variable with Id of i0[i2] for example.
        // ArrayElements are handled differently than normal variables.
        // CodeGeneration.Variable array = this.varHandler.GetVariable(v.Name, CorrespondingArray(v.Type));
        CodeGeneration.Variable array = this.varHandler.GetVariable(v.Name, v.Type);
        v.IntegerExpression.Visit(this); // Stores the value on top of stack
        CodeGeneration.Variable exprVar = this.stack.Pop();
        HandleIndexErrors(array, exprVar);
        this.varHandler.FreeTempVariable(exprVar);
        // This should be different for left-hand side and right-hand sides
        // For left hand sides, this is needed
        // For rhs tho, can assign to temp var and return that
        // Make undeclared var
        if (v.LHS)
        {
          if (v.Type == BuiltInType.StringArray) variable = new CodeGeneration.Variable($"{array.Id}[{exprVar.Id}]", ElementTypeOfArray(v.Type), array, exprVar.Id);
          else variable = new CodeGeneration.Variable($"{array.Id}[{exprVar.Id}]", ElementTypeOfArray(v.Type));
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
            this.writer.Write($"{temp.Id}={array.Id}[{exprVar.Id}];\n");
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
    public void VisitAssertStatement(AssertStatement s){}
    private void AssignStringToStringArray(CodeGeneration.Variable arrElement, CodeGeneration.Variable str)
    {
      this.writer.Write($"AssignStringToStringArray(&{arrElement.ElementOf.Id},{arrElement.Index},{str.Id},{arrElement.ElementOf.Size.Id},{arrElement.ElementOf.Lengths.Id});\n");
    }
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
      if (varVar.Type == BuiltInType.String)
      {
        if (varVar.IsArrayElement)
        {
          // Handle Assigning to string arrayElement s[1] := "something"
          AssignStringToStringArray(varVar, exprVar);
        }
        else
        {
          if (IsOkToAssign(exprVar))
          {
            // Small optimization
            exprVar.OriginalId = varVar.OriginalId;
            varVar.OriginalId = null;
          }
          else
          {
            // TODO: CopyCharPointer(varVar.Id, exprVar.Id, $"strlen({exprVar.Id})+1")
            System.Console.WriteLine("This is done");
            this.writer.WriteLine("/******* Assing string var to another *********/");
            // ReallocMemory(varVar.Id, $"{SizeOf(exprVar)}+1");
            // CopyStringVariable(varVar.Id, exprVar.Id);
            this.writer.WriteLine($"CopyCharPointer(&{varVar.Id},{exprVar.Id},(int)(strlen({exprVar.Id})+1));");
            this.writer.WriteLine("/******* end Assing string var to another *********/");
          }
        }
      }
      else if (IsArray(varVar.Type))
      {
        // Assumes that the exprVar is the same type (TypeCheckVisitor)
        this.writer.Write($"/******* Start of assigning array {exprVar.Id} to {varVar.Id} *********/\n");
        CopyArray(varVar, exprVar, null);
        this.writer.Write($"/******* End of assigning array {exprVar.Id} to {varVar.Id} *********/\n");
      }
      else if (varVar.Type == BuiltInType.Integer)
      {
        if (!varVar.IsArrayElement)
        {
          if (IsOkToAssign(exprVar))
          // if (exprVar.IsTemporary() && !exprVar.IsArrayElement)
          {
            exprVar.OriginalId = varVar.OriginalId;
            varVar.OriginalId = null;
          }
          else
          {
            SimpleAssignment(varVar.Id, exprVar.Id);
          }
        }
        else
        {
          // Handle assigning to integer array element i[1] := 2;
          SimpleAssignment(varVar.Id, exprVar.Id);
        }
      }
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
        System.Console.WriteLine("Also copying the Lengths");
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
      // s1=realloc(s1, sizeof(s2)); // sizeof(string) returns also the null char
      // while strlen(string) would need the + 1 for the null char
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
        case BuiltInType.Integer: return "%d";
        case BuiltInType.String: return "%s";
        default: return "";
      }
    }
  }
}
using System.Collections;
using System.Collections.Generic;

namespace CodeGeneration
{
  public class VariableHandler
  {
    private List<Variable> declaredIntegers;
    private List<Variable> declaredIntegerArrays;
    private List<Variable> declaredStrings;
    private List<Variable> declaredStringArrays;
    private List<Variable> declaredBooleans;
    private List<Variable> declaredBooleanArrays;

    private Stack<Variable> freeIntegers;
    private Stack<Variable> freeIntegerArrays;
    private Stack<Variable> freeStrings;
    private Stack<Variable> freeStringArrays;
    private Stack<Variable> freeBooleans;
    private Stack<Variable> freeBooleanArrays;

    private VariableGenerator generator;

    public VariableHandler()
    {
      this.declaredIntegers = new List<Variable>();
      this.declaredIntegerArrays = new List<Variable>();
      this.declaredStrings = new List<Variable>();
      this.declaredStringArrays = new List<Variable>();
      this.declaredBooleans = new List<Variable>();
      this.declaredBooleanArrays = new List<Variable>();
      this.freeIntegers = new Stack<Variable>();
      this.freeIntegerArrays = new Stack<Variable>();
      this.freeStrings = new Stack<Variable>();
      this.freeStringArrays = new Stack<Variable>();
      this.freeBooleans = new Stack<Variable>();
      this.freeBooleanArrays = new Stack<Variable>();
      this.generator = new VariableGenerator();
    }
    public Variable GetFreeVariable(Semantic.BuiltInType type)
    {
      switch(type)
      {
        case Semantic.BuiltInType.Integer: return GetFreeVariable(this.freeIntegers);
        case Semantic.BuiltInType.String: return GetFreeVariable(this.freeStrings);
        case Semantic.BuiltInType.IntegerArray: return GetFreeVariable(this.freeIntegerArrays);
        case Semantic.BuiltInType.StringArray: return GetFreeVariable(this.freeStringArrays);
        case Semantic.BuiltInType.Boolean: return GetFreeVariable(this.freeBooleans);
        case Semantic.BuiltInType.BooleanArray: return GetFreeVariable(this.freeBooleanArrays);
        default: return EmptyVariable();
      }
    }
    public Variable GetFreeVariable(Stack<Variable> stack)
    {
      try
      {
        return stack.Pop();
      }
      catch (System.InvalidOperationException)
      {
        return EmptyVariable();
      }
    }
    public Variable DeclareVariable(Semantic.BuiltInType type, string originalId, int block)
    {
      Variable v = DeclareVariable(type, block);
      v.OriginalId = originalId;
      return v;
    }
    public Variable DeclareVariable(Semantic.BuiltInType type, int block)
    {
      Variable v;
      switch(type)
      {
        case Semantic.BuiltInType.Integer:
          v = new Variable(this.generator.GenerateIntegerVariable(), null, type, block);
          this.declaredIntegers.Add(v);
          return v;
        case Semantic.BuiltInType.String:
          v = new Variable(this.generator.GenerateStringVariable(), null, type, block);
          this.declaredStrings.Add(v);
          return v;
        case Semantic.BuiltInType.IntegerArray:
          v = new Variable(this.generator.GenerateIntegerVariable(), null, type, block);
          this.declaredIntegerArrays.Add(v);
          return v;
        case Semantic.BuiltInType.StringArray:
          v = new Variable(this.generator.GenerateStringVariable(), null, type, block);
          this.declaredStringArrays.Add(v);
          return v;
        case Semantic.BuiltInType.Boolean:
          v = new Variable(this.generator.GenerateBooleanVariable(), null, type, block);
          this.declaredBooleans.Add(v);
          return v;
        case Semantic.BuiltInType.BooleanArray:
          v = new Variable(this.generator.GenerateBooleanVariable(), null, type, block);
          this.declaredBooleanArrays.Add(v);
          return v;
        default: return EmptyVariable();
      }
    }
    public void FreeVariable(Variable v)
    {
      ResetVariable(v);
      // TODO: Size, Lengths and ElementOf would be nice to also null
      switch(v.Type)
      {
        case Semantic.BuiltInType.Integer:
          if (!AlreadyFree(v, this.freeIntegers)) this.freeIntegers.Push(v);
          if (v.ElementOf.Id != null) FreeVariable(v.ElementOf);
          break;
        case Semantic.BuiltInType.String:
          if (!AlreadyFree(v, this.freeStrings)) this.freeStrings.Push(v);
          if (v.ElementOf.Id != null) FreeVariable(v.ElementOf);
          break;
        case Semantic.BuiltInType.IntegerArray:
          if (!AlreadyFree(v, this.freeIntegerArrays)) this.freeIntegerArrays.Push(v);
          // Also free the Size variable
          if (v.Size.Id != null) FreeVariable(v.Size);
          break;
        case Semantic.BuiltInType.StringArray:
          if (!AlreadyFree(v, this.freeStringArrays)) this.freeStringArrays.Push(v);
          // TODO: These need to be freed. Fix in GeneratorVisitor, to always set these
          // for StringArrays
          FreeVariable(v.Size);
          FreeVariable(v.Lengths);
          break;
        case Semantic.BuiltInType.Boolean:
          if (!AlreadyFree(v, this.freeBooleans)) this.freeBooleans.Push(v);
          if (v.ElementOf.Id != null) FreeVariable(v.ElementOf);
          break;
        case Semantic.BuiltInType.BooleanArray:
          if (!AlreadyFree(v, this.freeBooleanArrays)) this.freeBooleanArrays.Push(v);
          FreeVariable(v.Size);
          break;
        default: break;
      }
    }
    private void ResetVariable(Variable v)
    {
      v.OriginalId = null; // null the original reference
      v.IsArraySize = false;
      v.IsArrayElement = false;
      v.Index = null;
      v.Block = 0;
    }
    public void FreeVariablesDeclaredInBlock(int block)
    {
      FreeVariablesDeclaredInBlock(block, this.declaredIntegers, this.freeIntegers);
      FreeVariablesDeclaredInBlock(block, this.declaredStrings, this.freeStrings);
      FreeVariablesDeclaredInBlock(block, this.declaredBooleans, this.freeBooleans);
      FreeVariablesDeclaredInBlock(block, this.declaredIntegerArrays, this.freeIntegerArrays);
      FreeVariablesDeclaredInBlock(block, this.declaredStringArrays, this.freeStringArrays);
      FreeVariablesDeclaredInBlock(block, this.declaredBooleanArrays, this.freeBooleanArrays);
    }
    private void FreeVariablesDeclaredInBlock(int block, List<Variable> list, Stack<Variable> stack)
    {
      for (int i = list.Count - 1; i >= 0; i--)
      {
        Variable v = list[i];
        if (v.Block == block)
        {
          ResetVariable(v);
          if (!AlreadyFree(v, stack)) stack.Push(v);
        }
        else break;
      }
    }
    private bool AlreadyFree(Variable v, Stack<Variable> stack)
    {
      foreach(Variable e in stack) if (e.Id == v.Id) return true;
      return false;
    }
    public Variable GetVariable(string name, Semantic.BuiltInType type)
    {
      switch(type)
      {
        case Semantic.BuiltInType.Integer: return SearchDeclared(name, this.declaredIntegers);
        case Semantic.BuiltInType.String: return SearchDeclared(name, this.declaredStrings);
        case Semantic.BuiltInType.IntegerArray: return SearchDeclared(name, this.declaredIntegerArrays);
        case Semantic.BuiltInType.StringArray: return SearchDeclared(name, this.declaredStringArrays);
        case Semantic.BuiltInType.Boolean: return SearchDeclared(name, this.declaredBooleans);
        case Semantic.BuiltInType.BooleanArray: return SearchDeclared(name, this.declaredBooleanArrays);
        default: return EmptyVariable();
      }
    }
    public void RemoveAll()
    {
      this.declaredIntegers.Clear();
      this.declaredIntegerArrays.Clear();
      this.declaredStrings.Clear();
      this.declaredStringArrays.Clear();
      this.declaredBooleans.Clear();
      this.declaredBooleanArrays.Clear();
      this.freeIntegers.Clear();
      this.freeIntegerArrays.Clear();
      this.freeStrings.Clear();
      this.freeStringArrays.Clear();
      this.freeBooleans.Clear();
      this.freeBooleanArrays.Clear();
      this.generator.Reset();
    }
    private Variable SearchDeclared(string name, List<Variable> list)
    {
      foreach(Variable v in list)
      {
        if (v.OriginalId == name) return v;
      }
      return EmptyVariable();
    }
    public void FreeTempVariable(Variable v)
    {
      if (v.Id != null && v.IsTemporary() && !v.IsArrayElement && !v.IsArraySize) FreeVariable(v);
    }
    private Variable EmptyVariable()
    {
      return new Variable();
    }
  }
}
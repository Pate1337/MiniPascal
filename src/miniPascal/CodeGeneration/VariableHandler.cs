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

    private Stack<Variable> freeIntegers;
    private Stack<Variable> freeIntegerArrays;
    private Stack<Variable> freeStrings;
    private Stack<Variable> freeStringArrays;

    private VariableGenerator generator;

    public VariableHandler()
    {
      this.declaredIntegers = new List<Variable>();
      this.declaredIntegerArrays = new List<Variable>();
      this.declaredStrings = new List<Variable>();
      this.declaredStringArrays = new List<Variable>();
      this.freeIntegers = new Stack<Variable>();
      this.freeIntegerArrays = new Stack<Variable>();
      this.freeStrings = new Stack<Variable>();
      this.freeStringArrays = new Stack<Variable>();
      this.generator = new VariableGenerator();
    }
    public Variable GetFreeVariable(Semantic.BuiltInType type)
    {
      switch(type)
      {
        case Semantic.BuiltInType.Integer: return GetFreeInteger();
        case Semantic.BuiltInType.String: return GetFreeString();
        case Semantic.BuiltInType.IntegerArray: return GetFreeIntegerArray();
        case Semantic.BuiltInType.StringArray: return GetFreeStringArray();
        default: return EmptyVariable();
      }
    }
    public Variable GetFreeStringArray()
    {
      try
      {
        return this.freeStringArrays.Pop();
      }
      catch (System.InvalidOperationException)
      {
        return EmptyVariable();
      }
    }
    public Variable GetFreeIntegerArray()
    {
      try
      {
        return this.freeIntegerArrays.Pop();
      }
      catch (System.InvalidOperationException)
      {
        return EmptyVariable();
      }
    }
    public Variable GetFreeInteger()
    {
      try
      {
        return this.freeIntegers.Pop();
      }
      catch (System.InvalidOperationException)
      {
        return EmptyVariable();
      }
    }
    public Variable GetFreeString()
    {
      try
      {
        return this.freeStrings.Pop();
      }
      catch (System.InvalidOperationException)
      {
        return EmptyVariable();
      }
    }
    public Variable DeclareVariable(Semantic.BuiltInType type, string originalId)
    {
      Variable v = DeclareVariable(type);
      v.OriginalId = originalId;
      return v;
    }
    public Variable DeclareVariable(Semantic.BuiltInType type)
    {
      Variable v;
      switch(type)
      {
        case Semantic.BuiltInType.Integer:
          v = new Variable(this.generator.GenerateIntegerVariable(), null, type);
          this.declaredIntegers.Add(v);
          return v;
        case Semantic.BuiltInType.String:
          v = new Variable(this.generator.GenerateStringVariable(), null, type);
          this.declaredStrings.Add(v);
          return v;
        case Semantic.BuiltInType.IntegerArray:
          v = new Variable(this.generator.GenerateIntegerVariable(), null, type);
          this.declaredIntegerArrays.Add(v);
          return v;
        case Semantic.BuiltInType.StringArray:
          v = new Variable(this.generator.GenerateStringVariable(), null, type);
          this.declaredStringArrays.Add(v);
          return v;
        default: return EmptyVariable();
      }
    }
    public void FreeVariable(Variable v)
    {
      v.OriginalId = null; // null the original reference
      v.IsArraySize = false;
      v.IsArrayElement = false;
      v.Index = null;
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
        default: break;
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
        default: return EmptyVariable();
      }
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
      if (v.IsTemporary() && !v.IsArrayElement && !v.IsArraySize) FreeVariable(v);
    }
    private Variable EmptyVariable()
    {
      return new Variable(null, null, Semantic.BuiltInType.Error);
    }
  }
}
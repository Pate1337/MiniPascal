using System.Collections;
using System.Collections.Generic;

namespace CodeGeneration
{
  public class VariableHandler
  {
    private List<Variable> declaredIntegers;
    private List<Variable> declaredIntegerArrays;
    private List<Variable> declaredStrings;
    // private List<Variable> declaredIntegerPointers;

    private Stack<Variable> freeIntegers;
    private Stack<Variable> freeIntegerArrays;
    private Stack<Variable> freeStrings;
    // private Stack<Variable> freeIntegerPointers;

    private VariableGenerator generator;

    public VariableHandler()
    {
      this.declaredIntegers = new List<Variable>();
      this.declaredIntegerArrays = new List<Variable>();
      this.declaredStrings = new List<Variable>();
      // this.declaredIntegerPointers = new List<Variable>();
      this.freeIntegers = new Stack<Variable>();
      this.freeIntegerArrays = new Stack<Variable>();
      this.freeStrings = new Stack<Variable>();
      // this.freeIntegerPointers = new Stack<Variable>();
      this.generator = new VariableGenerator();
    }
    /*public Variable GetFreePointer(Semantic.BuiltInType type)
    {
      switch(type)
      {
        case Semantic.BuiltInType.Integer: return GetFreeIntegerPointer();
        default: return EmptyVariable();
      }
    }*/
    public Variable GetFreeVariable(Semantic.BuiltInType type)
    {
      switch(type)
      {
        case Semantic.BuiltInType.Integer: return GetFreeInteger();
        case Semantic.BuiltInType.String: return GetFreeString();
        case Semantic.BuiltInType.IntegerArray: return GetFreeIntegerArray();
        default: return EmptyVariable();
      }
    }
    /*public Variable GetFreeIntegerPointer()
    {
      try
      {
        return this.freeIntegerPointers.Pop();
      }
      catch (System.InvalidOperationException)
      {
        return EmptyVariable();
      }
    }*/
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
    /*public Variable DeclarePointer(Semantic.BuiltInType type)
    {
      Variable v;
      switch(type)
      {
        case Semantic.BuiltInType.Integer:
          v = new Variable($"*{this.generator.GenerateIntegerVariable()}", null, type);
          this.declaredIntegerPointers.Add(v);
          return v;
        default: return EmptyVariable();
      }
    }*/
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
        default: return EmptyVariable();
      }
    }
    public void FreeVariable(Variable v)
    {
      v.OriginalId = null; // null the original reference
      v.IsArraySize = false;
      v.IsArrayElement = false;
      switch(v.Type)
      {
        case Semantic.BuiltInType.Integer:
          if (!AlreadyFree(v, this.freeIntegers)) this.freeIntegers.Push(v);
          break;
        case Semantic.BuiltInType.String:
          if (!AlreadyFree(v, this.freeStrings)) this.freeStrings.Push(v);
          break;
        case Semantic.BuiltInType.IntegerArray:
          if (!AlreadyFree(v, this.freeIntegerArrays)) this.freeIntegerArrays.Push(v);
          // Also free the Size variable
          FreeVariable(v.Size);
          break;
        default: break;
      }
    }
    private bool AlreadyFree(Variable v, Stack<Variable> stack)
    {
      foreach(Variable e in stack) if (e.Id == v.Id) return true;
      return false;
    }
    /*public void FreePointer(Variable v)
    {
      switch(v.Type)
      {
        case Semantic.BuiltInType.Integer:
          this.freeIntegerPointers.Push(v);
          break;
        default: break;
      }
    }*/
    public Variable GetVariable(string name, Semantic.BuiltInType type)
    {
      switch(type)
      {
        case Semantic.BuiltInType.Integer: return SearchDeclared(name, this.declaredIntegers);
        case Semantic.BuiltInType.String: return SearchDeclared(name, this.declaredStrings);
        case Semantic.BuiltInType.IntegerArray: return SearchDeclared(name, this.declaredIntegerArrays);
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
    /*public bool IsPointer(Variable v)
    {
      foreach(Variable vari in this.declaredIntegerPointers)
      {
        if (vari.Id == v.Id) return true;
      }
      return false;
    }*/
    public void FreeTempVariable(Variable v)
    {
      if (v.IsTemporary() && !v.IsArrayElement && !v.IsArraySize) FreeVariable(v);
      /*{
        if (IsPointer(v)) FreePointer(v);
        else FreeVariable(v);
      }*/
    }
    private Variable EmptyVariable()
    {
      return new Variable(null, null, Semantic.BuiltInType.Error);
    }
  }
}
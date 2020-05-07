using System.Collections.Generic;
using System;
using Errors;
using FileHandler;
using IO;
using Nodes;

namespace Semantic
{
  /*
  * This class is used to interact with the SymbolTable.
  * All the errors are handled here.
  * The blocks are also handled here.
  */
  public class SymbolTableHandler
  {
    private SymbolTable CurrentBlock;
    private Reader reader;
    private IOHandler io;
    public SymbolTableHandler(IOHandler io, Reader reader)
    {
      this.CurrentBlock = new SymbolTable();
      this.io = io;
      this.reader = reader;
    }
    public void AddNewBlock()
    {
      // this.io.WriteLine("Before adding a block:");
      // PrintTable();
      SymbolTable block = new SymbolTable();
      block.Parent = this.CurrentBlock;
      this.CurrentBlock = block;
      // this.io.WriteLine("After adding a block:");
      // PrintTable();
    }
    public void RemoveCurrentBlock()
    {
      // this.io.WriteLine("Before removing block:");
      // PrintTable();
      this.CurrentBlock = this.CurrentBlock.Parent;
      // this.io.WriteLine("After removing block:");
      // PrintTable();
    }
    public void AddEntry(string id, SymbolTableEntry e, Location loc)
    {
      // Need to check if entry by id is a procedure/function or parameter of this block
      // Can not declare those again
      SymbolTableEntry existingEntry = FindEntry(id);
      if (existingEntry.Type != BuiltInType.Error)
      {
        // Was found
        if (IsParameter(existingEntry)) new Error($"Variable {id} has already been declared. Can not re-declare a parameter.", loc, this.reader).Print(this.io);
        else if (IsProcedureOrFunction(existingEntry)) new Error($"Variable {id} has already been declared. Can not re-declare a procedure or function.", loc, this.reader).Print(this.io);
        else if (IsProgramName(existingEntry)) new Error($"Variable {id} has already been declared. Can not re-declare the name of the program.", loc, this.reader).Print(this.io);
        else new Error($"Variable {id} has already been declared!", loc, this.reader).Print(this.io);
      }
      else
      {
        // try - catch even tho redundant. (already checked that entry does not exist)
        try
        {
          this.CurrentBlock.AddEntry(id, e);
        }
        catch (ArgumentException)
        {
          new Error($"Variable {id} has already been declared!", loc, this.reader).Print(this.io);
        }
      }
    }
    public SymbolTableEntry GetEntry(string id, Location loc)
    {
      SymbolTableEntry e = FindEntry(id);
      if (e.Type == BuiltInType.Error) new Error($"Variable {id} has not been declared!", loc, this.reader).Print(this.io);
      return e;
    }
    /*
    * No Error message here
    */
    public SymbolTableEntry GetEntry(string id)
    {
      return FindEntry(id);
    }
    /*
    * Type checking is done in TypeCheckVisitor.
    */
    public void Assign(string id, Location loc)
    {
      SymbolTableEntry e = FindEntry(id);
      if (e.Type != BuiltInType.Error)
      {
        if (IsParameter(e))
        {
          if (!IsReferenceParameter(e)) new Error($"Can not assign a value to variable {id}, because it is a parameter.", loc, this.reader).Print(this.io);
        }
        else if (IsProcedureOrFunction(e)) new Error($"Can not assign a value to variable {id}, because it is a procedure or a function.", loc, this.reader).Print(this.io);
        else if (IsProgramName(e)) new Error($"Can not assign a value to variable {id}, because it is the program's name.", loc, this.reader).Print(this.io);
      }
      else new Error($"Can not assign a value to variable {id}, because it has not been declared.", loc, this.reader).Print(this.io);

      /*if (e.Type == BuiltInType.Error)
      {
        new Error($"Can not assign a value to variable {id}, because it has not been declared.", loc, this.reader).Print(this.io);
        return false;
      }
      if (IsParameter(e))
      {
        if (!IsReferenceParameter(e))
        {
          new Error($"Can not assign a value to variable {id}, because it is a parameter.", loc, this.reader).Print(this.io);
          return false;
        }
      }
      else if (IsProcedureOrFunction(e))
      {
        new Error($"Can not assign a value to variable {id}, because it is a procedure or a function.", loc, this.reader).Print(this.io);
        return false;
      }
      else if (IsProgramName(e))
      {
        new Error($"Can not assign a value to variable {id}, because it is the program's name.", loc, this.reader).Print(this.io);
        return false;
      }
      return true;*/
    }
    private bool IsReferenceParameter(SymbolTableEntry e)
    {
      if (e.ParameterType == "ref") return true;
      return false;
    }
    private bool IsParameter(SymbolTableEntry e)
    {
      if (e.ParameterType != null) return true;
      return false;
    }
    private bool IsProcedureOrFunction(SymbolTableEntry e)
    {
      if (e.Parameters != null) return true;
      return false;
    }
    private bool IsProgramName(SymbolTableEntry e)
    {
      if (e.Parameters == null && e.Type == BuiltInType.Void) return true;
      return false;
    }
    private SymbolTableEntry FindEntryFromBlock(string id, SymbolTable block)
    {
      try
      {
        return block.GetEntry(id);
      }
      catch (KeyNotFoundException)
      {
        return new SymbolTableEntry("", BuiltInType.Error);
      }
      catch (NullReferenceException)
      {
        // If block is null
        return new SymbolTableEntry("", BuiltInType.Error);
      }
    }
    private SymbolTableEntry FindEntry(string id)
    {
      SymbolTable block = this.CurrentBlock;
      SymbolTableEntry e = new SymbolTableEntry("", BuiltInType.Error);
      while (block != null)
      {
        e = FindEntryFromBlock(id, block);
        if (e.Type != BuiltInType.Error) return e;
        block = block.Parent;
      }
      // Was not found
      return e;
    }
    public void PrintTable()
    {
      this.io.WriteLine("SYMBOLTABLE:");
      SymbolTable block = this.CurrentBlock;
      while (block != null)
      {
        this.io.WriteLine("Block");
        block.PrintTable();
        block = block.Parent;
      }
    }
  }
}
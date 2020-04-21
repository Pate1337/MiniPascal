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
      this.io.WriteLine("Before adding a block:");
      PrintTable();
      SymbolTable block = new SymbolTable();
      block.Parent = this.CurrentBlock;
      this.CurrentBlock = block;
      this.io.WriteLine("After adding a block:");
      PrintTable();
    }
    public void RemoveCurrentBlock()
    {
      this.io.WriteLine("Before removing block:");
      PrintTable();
      this.CurrentBlock = this.CurrentBlock.Parent;
      this.io.WriteLine("After removing block:");
      PrintTable();
    }
    public void AddEntry(string id, SymbolTableEntry e, Location loc)
    {
      // Need to check if entry by id is a procedure/function or parameter of this block
      // Can not declare those again
      if (IsParameter(id)) new Error($"Variable {id} has already been declared. Can not re-declare a parameter.", loc, this.reader).Print(this.io);
      else if (IsProcedureOrFunction(id)) new Error($"Variable {id} has already been declared. Can not re-declare a procedure or function.", loc, this.reader).Print(this.io);
      else if (IsProgramName(id)) new Error($"Variable {id} has already been declared. Can not re-declare the name of the program.", loc, this.reader).Print(this.io);
      else
      {
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
    private bool IsParameter(string id)
    {
      // Parameters are located in Parent block.
      // SymbolTable block = this.CurrentBlock.Parent;
      // SymbolTableEntry e = FindEntryFromBlock(id, block);
      SymbolTableEntry e = FindEntry(id);
      if (e.Type != BuiltInType.Error)
      {
        // Was found. Check if parameter.
        if (e.ParameterType != null) return true;
      }
      return false;
    }
    private bool IsProcedureOrFunction(string id)
    {
      // Procedures and functions are declared in the global scope.
      SymbolTable block = GetGlobalScope();
      SymbolTableEntry e = FindEntryFromBlock(id, block);
      if (e.Type != BuiltInType.Error)
      {
        // If entry has Parameters, is a function or procedure.
        if (e.Parameters != null) return true;
      }
      return false;
    }
    private bool IsProgramName(string id)
    {
      // Program name is declared in the global scope.
      SymbolTable block = GetGlobalScope();
      SymbolTableEntry e = FindEntryFromBlock(id, block);
      if (e.Type != BuiltInType.Error)
      {
        // If entry has no parameters and Type is Void => Program name
        if (e.Parameters == null && e.Type == BuiltInType.Void) return true;
      }
      return false;
    }
    private SymbolTable GetGlobalScope()
    {
      SymbolTable block = this.CurrentBlock;
      while (block.Parent != null)
      {
        block = block.Parent;
      }
      return block;
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
      /*while (block != null)
      {
        try
        {
          return block.GetEntry(id);
        }
        catch (KeyNotFoundException)
        {
          // Not found in the current block. Find from the Parent.
          block = block.Parent;
        }
      }
      // Was not found in any of the blocks
      return new SymbolTableEntry("", BuiltInType.Error);*/
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
using System.Collections.Generic;
using System;

namespace Semantic
{
  public class SymbolTable
  {
    private Dictionary<string, SymbolTableEntry> Table;

    public SymbolTable()
    {
      this.Table = new Dictionary<string, SymbolTableEntry>();
    }
    public void AddEntry(string id, BuiltInType type)
    {
      try
      {
        this.Table.Add(id, new SymbolTableEntry(id, type));
      }
      catch (ArgumentException)
      {
        Console.WriteLine($"Identifier {id} has already been declared!");
      }
    }
    public SymbolTableEntry GetEntry(string id)
    {
      return this.Table[id];
    }
    public void PrintTable()
    {
      Console.WriteLine("SYMBOLTABLE:");
      foreach (KeyValuePair<string, SymbolTableEntry> pair in this.Table)
      {
        Console.WriteLine($"<{pair.Key}, ({pair.Value.Identifier}, {pair.Value.Type})>");
      }
    }
  }
}
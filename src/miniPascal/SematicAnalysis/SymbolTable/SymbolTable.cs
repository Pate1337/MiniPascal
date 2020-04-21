using System.Collections.Generic;
using System;

namespace Semantic
{
  public class SymbolTable
  {
    private Dictionary<string, SymbolTableEntry> Table;
    public SymbolTable Parent { get; set; }

    public SymbolTable()
    {
      this.Table = new Dictionary<string, SymbolTableEntry>();
      this.Parent = null;
    }
    public void AddEntry(string id, SymbolTableEntry e)
    {
      this.Table.Add(id, e);
    }
    public SymbolTableEntry GetEntry(string id)
    {
      return this.Table[id];
    }
    public void PrintTable()
    {
      foreach (KeyValuePair<string, SymbolTableEntry> pair in this.Table)
      {
        Console.WriteLine(pair.Value);
        /*Console.Write($"<{pair.Key}, ({pair.Value.Identifier}, {pair.Value.Type}, {pair.Value.Reference}");
        if (pair.Value.Parameters != null)
        {
          Console.Write(", [ ");
          foreach (string p in pair.Value.Parameters) Console.Write($"{p},");
          Console.Write(" ]");
        }
        Console.Write(")>\n");*/
      }
    }
  }
}
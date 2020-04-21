using System.Collections.Generic;

namespace Semantic
{
  public class SymbolTableEntry
  {
    public string Identifier { get; set; }
    public BuiltInType Type { get; set; }
    public List<SymbolTableEntry> Parameters { get; set; }
    public string ParameterType { get; set; }

    public SymbolTableEntry(string identifier, BuiltInType type)
    {
      this.Identifier = identifier;
      this.Type = type;
      // Important to default at null. null means not a function or procedure.
      this.Parameters = null;
      // Set to null by default. null means not a parameter.
      this.ParameterType = null;
    }
    public SymbolTableEntry(string identifier, BuiltInType type, string reference)
    {
      this.Identifier = identifier;
      this.Type = type;
      this.Parameters = null;
      this.ParameterType = reference;
    }
    public override string ToString()
    {
      string entry = $"<Identifier: {this.Identifier}, Type: {this.Type}";
      if (this.ParameterType != null) entry += $", ParameterType: {this.ParameterType}";
      if (this.Parameters != null)
      {
        List<string> pars = new List<string>();
        foreach (SymbolTableEntry e in this.Parameters)
        {
          pars.Add(e.Identifier);
        }
        entry += ", Parameters: [ ";
        entry += Utils.StringHandler.StringListToString(pars);
        entry += " ]";
      }
      entry += ">";
      return entry;
    }
  }
}
namespace Semantic
{
  public class SymbolTableEntry
  {
    public string Identifier { get; set; }
    public BuiltInType Type { get; set; }

    public SymbolTableEntry(string identifier, BuiltInType type)
    {
      this.Identifier = identifier;
      this.Type = type;
    }
  }
}
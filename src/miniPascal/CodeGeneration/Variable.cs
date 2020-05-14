namespace CodeGeneration
{
  public class Variable
  {
    public string Id { get; set; }
    public string OriginalId { get; set; }
    public Semantic.BuiltInType Type { get; set; }
    public bool IsArrayElement { get; set; }
    public bool IsArraySize { get; set; }
    // public string Size { get; set; }
    public Variable Size { get; set; } // For integer, real and boolean arrays

    public Variable(string id, Semantic.BuiltInType type)
    {
      this.Id = id;
      this.OriginalId = null;
      this.Type = type;
      this.IsArrayElement = true;
      this.IsArraySize = false;
      // this.Size = null;
      // this.Size = new Variable(null, null, Semantic.BuiltInType.Error);
    }

    public Variable(string id, string originalId, Semantic.BuiltInType type)
    {
      this.Id = id;
      this.OriginalId = originalId;
      this.Type = type;
      this.IsArrayElement = false;
      this.IsArraySize = false;
      // this.Size = null;
      // this.Size = new Variable(null, null, Semantic.BuiltInType.Error);
    }
    public void SetSize(Variable v)
    {
      this.Size = v;
      v.IsArraySize = true;
      System.Console.WriteLine("SetSize");
    }
    public bool IsTemporary()
    {
      return this.OriginalId == null;
    }
  }
}
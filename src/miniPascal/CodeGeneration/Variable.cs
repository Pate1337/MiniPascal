using System.Collections.Generic;

namespace CodeGeneration
{
  public class Variable
  {
    public string Id { get; set; }
    public string OriginalId { get; set; }
    public Semantic.BuiltInType Type { get; set; }
    public bool IsArrayElement { get; set; }
    public bool IsArraySize { get; set; }
    public Variable Size { get; set; } // For integer, real and boolean arrays
    public Variable Lengths { get; set; } // For StringArrays. TODO: Change to Offsets
    public Variable ElementOf { get; set; } // An array variable
    public string Index { get; set; }
    public int Block { get; set; }

    /*
    * Creates an empty variable
    */
    public Variable()
    {
      this.Id = null;
      this.OriginalId = null;
      this.Type = Semantic.BuiltInType.Error;
      this.IsArrayElement = false;
      this.IsArraySize = false;
      this.Index = null;
      this.Block = 0;
    }

    /*
    * Creates an ArrayElement variable
    */
    public Variable(string id, Semantic.BuiltInType type, Variable arr, string index, int block)
    {
      this.Id = id;
      this.OriginalId = null;
      this.Type = type;
      this.IsArrayElement = true;
      this.IsArraySize = false;
      this.ElementOf = arr;
      this.Index = index;
      this.Size = new Variable();
      this.Lengths = new Variable();
      this.Block = block;
    }

    /*
    * Also creates an ArrayElement variable
    */
    public Variable(string id, Semantic.BuiltInType type, int block)
    {
      this.Id = id;
      this.OriginalId = null;
      this.Type = type;
      this.IsArrayElement = true;
      this.IsArraySize = false;
      this.Index = null;
      this.Size = new Variable();
      this.Lengths = new Variable();
      this.ElementOf = new Variable();
      this.Block = block;
    }

    /*
    * Creates a variable that has been declared in MiniPascal.
    */
    public Variable(string id, string originalId, Semantic.BuiltInType type, int block)
    {
      this.Id = id;
      this.OriginalId = originalId;
      this.Type = type;
      this.IsArrayElement = false;
      this.IsArraySize = false;
      this.Index = null;
      this.Size = new Variable();
      this.Lengths = new Variable();
      this.ElementOf = new Variable();
      this.Block = block;
    }
    public void SetSize(Variable v)
    {
      this.Size = v;
      v.IsArraySize = true;
    }
    public void SetLengths(Variable v)
    {
      this.Lengths = v;
      v.IsArraySize = true;
    }
    public bool IsTemporary()
    {
      return this.OriginalId == null;
    }
  }
}
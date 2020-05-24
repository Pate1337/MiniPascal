namespace CodeGeneration
{
  public class VariableGenerator
  {
    private int intCount;
    private int stringCount;
    private int booleanCount;
    public VariableGenerator()
    {
      this.intCount = 0;
      this.stringCount = 0;
      this.booleanCount = 0;
    }
    public string GenerateIntegerVariable()
    {
      this.intCount++;
      return $"i{this.intCount-1}";
    }
    public string GenerateStringVariable()
    {
      this.stringCount++;
      return $"s{this.stringCount-1}";
    }
    public string GenerateBooleanVariable()
    {
      this.booleanCount++;
      return $"b{this.booleanCount-1}";
    }
    public void Reset()
    {
      this.intCount = 0;
      this.stringCount = 0;
      this.booleanCount = 0;
    }
  }
}
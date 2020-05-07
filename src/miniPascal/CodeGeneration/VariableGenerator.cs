namespace CodeGeneration
{
  public class VariableGenerator
  {
    private int intCount;
    private int stringCount;
    public VariableGenerator()
    {
      this.intCount = 0;
      this.stringCount = 0;
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
  }
}
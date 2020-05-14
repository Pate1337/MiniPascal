namespace CodeGeneration
{
  public class LabelGenerator
  {
    private int count;
    public LabelGenerator()
    {
      this.count = 0;
    }
    public string GenerateLabel()
    {
      this.count++;
      return $"L{this.count-1}";
    }
  }
}
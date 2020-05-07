namespace CodeGeneration
{
  public class Variable
  {
    public string Id { get; set; }
    public string OriginalId { get; set; }

    public Variable(string id, string originalId)
    {
      this.Id = id;
      this.OriginalId = originalId;
    }
  }
}
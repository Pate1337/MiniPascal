using System.Collections.Generic;

namespace IO
{
  public class TestIO : IOHandler
  {
    private string[] inputs;
    private int index;
    List<string> outputs;
    List<string> warnings;

    public TestIO(string[] inputs)
    {
      this.inputs = inputs;
      this.index = 0;
      this.outputs = new List<string>();
      this.warnings = new List<string>();
    }
    public string ReadLine()
    {
      return this.inputs[this.index++];
    }
    public void WriteLine(string line)
    {
      this.outputs.Add(line.ToString());
    }
    public void Write(string text)
    {
      // Not yet implemented
    }
    public List<string> GetOutput()
    {
      return this.outputs;
    }
  }
}
using Errors;

namespace Utils
{
  public abstract class File
  {
    public static bool Exists(string file)
    {
      return System.IO.File.Exists(file);
    }
    public static System.IO.StreamReader CreateStreamReader(string file)
    {
      try
      {
        return new System.IO.StreamReader(file);
      }
      catch (System.Exception)
      {
        throw new Error($"Could not read file {file}.");
      }
    }
    /*public static string ReadAllText(string file)
    {
      try
      {
        return System.IO.File.ReadAllText(file);
      }
      catch (Exception)
      {
        throw new Error($"Could not read file {file}");
      }
    }*/
  }
}
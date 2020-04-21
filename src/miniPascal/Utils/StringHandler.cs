using System.Collections.Generic;

namespace Utils
{
  public abstract class StringHandler
  {
    public static string BuiltInTypeListToString(List<Semantic.BuiltInType> list)
    {
      int i = 1;
      string s = "";
      foreach (Semantic.BuiltInType t in list)
      {
        s += $"{t}";
        if (i < list.Count) s += ", ";
        i++;
      }
      return s;
    }
    public static string StringListToString(List<string> list)
    {
      int i = 1;
      string s = "";
      foreach (string t in list)
      {
        s += t;
        if (i < list.Count) s += ", ";
        i++;
      }
      return s;
    }
  }
}
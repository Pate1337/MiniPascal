/*using System;
using System.Runtime;
using System.Reflection;
using System.Reflection.Emit;*/
using Utils;
using Errors;

namespace Assembler
{
  public static class Assembler
  {
    public static void Start()
    {
      string currPath = System.AppDomain.CurrentDomain.BaseDirectory;
      System.Console.WriteLine(currPath);
      System.IO.StreamWriter file;
      try
      {
        file = File.CreateStreamWriter($"{currPath}/test.c");
        //byte[] info = new System.Text.UTF8Encoding(true).GetBytes("This is some text in the file.");
        // Add some information to the file.
        //file.Write(info, 0, info.Length);
        file.WriteLine("morjens");
        file.WriteLine("jep");
        file.Dispose();
      }
      catch (Error e)
      {
        System.Console.WriteLine(e);
      }
      /*
      //AppDomain ad = AppDomain.CurrentDomain;
      //AssemblyName am = new AssemblyName();
      //am.Name = "TestAsm";
      // AssemblyBuilder ab = ad.DefineDynamicAssembly(am, AssemblyBuilderAccess.Run);
      AssemblyBuilder ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()),AssemblyBuilderAccess.RunAndSave);
      ModuleBuilder mb = ab.DefineDynamicModule("testmod");
      TypeBuilder tb = mb.DefineType("mytype", TypeAttributes.Public);
      MethodBuilder metb = tb.DefineMethod("hi", MethodAttributes.Public |
      MethodAttributes.Static);
      // ab.SetEntryPoint(metb);

      ILGenerator il = metb.GetILGenerator();

      //LocalBuilder result = il.DeclareLocal(typeof(int));
      //il.Emit(OpCodes.Stloc, result);

      il.EmitWriteLine("Hello World");
      il.Emit(OpCodes.Ret);
      tb.CreateType();
      // ab.Save("TestAsm.exe");

      // empty argument array
      object[] noArgs = new object[0];

      // call the method we wrote (on a null object because we don't
      // need one for static method)
      ab.GetType("mytype").GetMethod("hi").Invoke(null, noArgs);*/
    }
  }
}
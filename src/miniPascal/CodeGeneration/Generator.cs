using FileHandler;
using Nodes;
using IO;
using Semantic;

namespace CodeGeneration
{
  public class Generator
  {
    private FileWriter writer;
    private ProgramNode ast;
    private IOHandler io;
    private Visitor visitor;
    private FunctionCreator fc;
    private string FileToCompile;
    private string PathOfExe;
    private string NameOfExe;
    public Generator(FileWriter writer, ProgramNode ast, IOHandler io, Visitor visitor, FunctionCreator fc)
    {
      this.writer = writer;
      this.ast = ast;
      this.io = io;
      this.visitor = visitor;
      this.fc = fc;
      this.FileToCompile = this.writer.FileName;
      this.PathOfExe = System.IO.Path.ChangeExtension(this.FileToCompile, ".exe");
      this.NameOfExe = System.IO.Path.GetFileName(this.PathOfExe);
    }
    public void GenerateCode()
    {
      this.writer.WriteLine("#include <stdio.h>");
      this.writer.WriteLine("#include <stdlib.h>");
      this.writer.WriteLine("#include <string.h>");
      this.writer.WriteLine("#include <math.h>");
      this.fc.WriteFunctions(this.writer);
      this.visitor.VisitProceduresAndFunctions(this.ast);
      this.writer.WriteLine("int main() {");
      // this.writer.WriteLine("setvbuf(stdout, 0, _IONBF, 0);");
      this.visitor.VisitProgram(this.ast);
      /*this.writer.WriteLine("goto END;");
      this.writer.WriteLine("ERROR:;");
      this.writer.WriteLine("printf(\"Error occurred! Stopped execution.\\n\");");
      this.writer.WriteLine("END:;");*/
      this.writer.WriteLine("return 0;");
      this.writer.WriteLine("}");
      this.writer.Close();
    }
    public void CreateExecutable()
    {
      // TODO: Add check that file test.c exists
      System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
      // TODO: On windows this is different
      pProcess.StartInfo.FileName = @"/usr/bin/gcc";
      pProcess.StartInfo.Arguments = $"-Wall {this.FileToCompile} -o {this.PathOfExe}"; //argument

      // Redirect all the output and errors from compilation of C program
      pProcess.StartInfo.UseShellExecute = false;
      pProcess.StartInfo.RedirectStandardOutput = true;
      string eOut = null;
      pProcess.StartInfo.RedirectStandardError = true;
      pProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler((sender, e) => 
                                 { eOut += e.Data; });
      pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
      pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
      pProcess.Start();

      // To avoid deadlocks, use an asynchronous read operation on at least one of the streams.  
      // pProcess.BeginErrorReadLine();

      while (!pProcess.WaitForExit(1000));
      HandleErrors(pProcess, "c", eOut);
    }
    public void RunExecutable()
    {
      if (!Utils.File.Exists(this.PathOfExe)) throw new Errors.Error($"Could not execute file {this.PathOfExe} because it does not exist!");
      System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
      pProcess.StartInfo.FileName = this.PathOfExe;
      // pProcess.StartInfo.UseShellExecute = false;
      // pProcess.StartInfo.RedirectStandardOutput = true;
      pProcess.Start();

      // string output = pProcess.StandardOutput.ReadToEnd();

      // System.Console.Write(output);

      while (!pProcess.WaitForExit(1000));
      HandleErrors(pProcess, "e", null);
    }
    private void HandleErrors(System.Diagnostics.Process p, string t, string e)
    {
      int code = 5;
      try
      {
        code = p.ExitCode;
      }
      catch (System.Exception)
      {
        code = 100;
      }
      string gccErrors = e != null ? $"\ngcc returned errors:\n{e}" : "";
      switch(code)
      {
        case 0: break; // No error
        case (int)ErrorCode.NegativeIndex:
          throw new Errors.CompileError(t, $"Negative indexes are not allowed!{gccErrors}");
        case (int)ErrorCode.OutOfBoundsIndex:
          throw new Errors.CompileError(t, $"Index was out of bounds!{gccErrors}");
        /*case (int)ErrorCode.InvalidBoolean:
          throw new Errors.CompileError(t, $"Boolean type was assigned an invalid integer value! Only 0 and 1 are allowed.{gccErrors}");*/
        case 100: throw new Errors.CompileError(t, "Could not get the exitCode of process...");
        default:
          throw new Errors.CompileError(t, $"The process that executes {(t == "e" ? this.NameOfExe : "The .c file")} returned error code {code}!{gccErrors}");
      }
    }
  }
}
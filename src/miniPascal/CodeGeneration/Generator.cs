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
    public Generator(FileWriter writer, ProgramNode ast, IOHandler io, Visitor visitor)
    {
      this.writer = writer;
      this.ast = ast;
      this.io = io;
      this.visitor = visitor;
    }
    public void GenerateCode()
    {
      this.writer.WriteLine("#include <stdio.h>");
      this.writer.WriteLine("#include <stdlib.h>");
      this.writer.WriteLine("#include <string.h>");
      this.writer.WriteLine("#include <math.h>");
      this.writer.WriteLine("int main() {");
      this.visitor.VisitProgram(this.ast);
      // this.writer.WriteLine("printf(\"Hello, World!\\n\");");
      this.writer.WriteLine("goto END;");
      this.writer.WriteLine("ERROR:");
      this.writer.WriteLine("printf(\"Error occurred!\\n\");");
      this.writer.WriteLine("END:");
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
      string currPath = System.AppDomain.CurrentDomain.BaseDirectory;
      pProcess.StartInfo.Arguments = $"-Wall {this.writer.FileName} -o {currPath}/test.exe"; //argument

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
      int code = 5;
      try
      {
        code = pProcess.ExitCode;
      }
      catch (System.Exception)
      {
        code = 1;
      }
      if (code == 0) System.Console.WriteLine("C compiled succesfully!");
      else
      {
        System.Console.WriteLine("Error compiling C!");
        System.Console.WriteLine("Errors: " + eOut);
      }
    }
    public void RunExecutable()
    {
      // TODO: Add check that test.exe exists
      System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
      pProcess.StartInfo.FileName = @"test.exe";
      pProcess.StartInfo.UseShellExecute = false;
      pProcess.StartInfo.RedirectStandardOutput = true;
      pProcess.Start();

      string output = pProcess.StandardOutput.ReadToEnd();

      System.Console.Write(output);

      while (!pProcess.WaitForExit(1000));
    }
  }
}
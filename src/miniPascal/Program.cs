using Lexer;
using Errors;
using IO;
using FileHandler;
using Semantic;
using CodeGeneration;

namespace miniPascal
{
    class Program
    {
        static void Main(string[] args)
        {
            IOHandler io = new SystemIO();
            try
            {
                ArgHandler argHandler = new ArgHandler(args);
                string fullPath = Utils.File.GetFullPath(argHandler.FileName);
                Reader reader = new FileReader(fullPath);
                Parser p = new Parser(io, reader);
                Nodes.ProgramNode ast = p.Parse();
                
                if (argHandler.PrintAst)
                {
                    PrintVisitor visitor = new PrintVisitor(io);
                    visitor.VisitProgram(ast);
                }

                FileWriter writer = new FileWriter(fullPath);

                FunctionCreator fg = new FunctionCreator();
                TypeCheckVisitor v = new TypeCheckVisitor(io, reader, fg);
                v.VisitProgram(ast);


                GeneratorVisitor gv = new GeneratorVisitor(io, reader, writer);
                Generator generator = new Generator(writer, ast, io, gv, fg);
                generator.GenerateCode();
                generator.CreateExecutable();
                generator.RunExecutable();
            }
            catch (Error e)
            {
                e.Print(io);
            }
        }
    }
}

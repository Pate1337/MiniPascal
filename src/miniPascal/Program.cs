using Lexer;
using Errors;
using IO;
using FileHandler;
using Semantic;

namespace miniPascal
{
    class Program
    {
        static void Main(string[] args)
        {
            IOHandler io = new SystemIO();
            try
            {
                // Tee parserista rivi kerrallaan 
                ArgHandler argHandler = new ArgHandler(args);
                Reader reader = new FileReader(argHandler.FileName);
                Parser p = new Parser(io, reader);
                Nodes.ProgramNode ast = p.Parse();

                PrintVisitor visitor = new PrintVisitor(io);
                visitor.VisitProgram(ast);

                TypeCheckVisitor v = new TypeCheckVisitor(io, reader);
                v.VisitProgram(ast);

                /*Scanner scanner = new Scanner(reader);

                Token token;
                while ((token = scanner.NextToken()).Type != TokenType.EOF)
                {
                    io.WriteLine(token.ToString());
                }*/
                
                // Generic literal = new IntLiteral(34);
                
                // ScanBuffer buffer = new ScanBuffer(reader);
                
                /*int i = 0;
                while (!buffer.IsEmpty())
                {
                    io.WriteLine(buffer.ReadChar().ToString());
                }*/


                /*string line;
                while ((line = reader.ReadNextLine()) != null)
                {
                    io.WriteLine(line);
                }
                io.WriteLine(reader.ReadNextLine());
                io.WriteLine(reader.Lines.Count.ToString());*/


                // bool exists = Utils.File.Exists(argHandler.FileName);
                // io.WriteLine("Exists: " + exists);
                //Console.WriteLine(argHandler.FileName);
                //Console.WriteLine(argHandler.PrintAst);
            }
            catch (Error e)
            {
                e.Print(io);
            }
            /*bool fileDefined = false;
            bool printAst = false;
            string fileName = "";
            foreach (string arg in args)
            {
                if (arg == "-ast") printAst = true;
                if (arg[0] != '-' && fileDefined)
                {
                    Console.WriteLine("Only filename can be defined without '-'. Other arguments must have '-' in front!");
                    fileDefined = false;
                    break;
                }
                if (arg[0] != '-')
                {
                    fileDefined = true;
                    fileName = arg;
                }
            }
            if (!fileDefined)
            {
                Console.WriteLine("No file specified. Create a commandline tool here.");
            }
            else
            {
                if (System.IO.File.Exists(fileName))
                {
                    FileReader.SetFile(fileName);
                    string text = FileReader.ReadAllText();
                    try
                    {
                        BlockNode ast = BuildAST(text);
                        if (printAst) PrintAST(ast);
                        RunInterpreter(ast);
                    }
                    catch (Error e)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e);
                        Console.ResetColor();
                    }
                    FileReader.ClearInput();
                }
                else
                {
                    Console.WriteLine($"File {fileName} can not be found! Use absolute path.");
                }
            }*/
        }
    }
}

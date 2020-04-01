using IO;
using Nodes;

namespace Semantic
{
  public class PrintVisitor : Visitor
  {
    private IOHandler io;
    private int depth;
    public PrintVisitor(IOHandler io)
    {
      this.io = io;
      this.depth = 0;
    }
    public void VisitProgram(ProgramNode p)
    {
      this.io.Write($"(<{p.Style}>: Name: {p.Name}, Procedures:");
      this.depth++;
      foreach (Procedure procedure in p.Procedures)
      {
        procedure.Visit(this);
      }
      this.depth--;
      this.io.Write(")\n");
    }
    public void VisitProcedure(Procedure p)
    {
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{p.Style}>: Name: {p.Name}, Parameters:");
      this.depth++;
      foreach (Node param in p.Parameters)
      {
        param.Visit(this);
      }
      this.depth--;
      this.io.Write(")");
    }
    public void VisitReferenceParameter(ReferenceParameter rp)
    {
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{rp.Style}>: Name: {rp.Name}, Type:");
      this.depth++;
      Node n = (Node) rp.Type;
      n.Visit(this);
      this.depth--;
      this.io.Write(")");
    }
    public void VisitValueParameter(ValueParameter vp)
    {
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{vp.Style}>: Name: {vp.Name}, Type:");
      this.depth++;
      Node n = (Node) vp.Type;
      n.Visit(this);
      this.depth--;
      this.io.Write(")");
    }
    public void VisitSimpleType(SimpleType t)
    {
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{t.Style}>: Type: {t.Type}");
      this.io.Write(")");
    }
    public void VisitArrayType(ArrayType t)
    {
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{t.Style}>: Type: {t.Type}, IntegerExpression:");
      this.depth++;
      Node n = (Node) t.IntegerExpression;
      n.Visit(this);
      this.depth--;
      this.io.Write(")");
    }
    public void VisitSimpleExpression(SimpleExpression e)
    {
      /*
      public string Style { get; set; }
      public string Sign { get; set; }
      public Term Term { get; set; }
      public List<SimpleExpressionAddition> Additions { get; set; }
      */
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{e.Style}>: Sign: {e.Sign}, Term:");
      this.depth++;
      Node n = (Node) e.Term;
      n.Visit(this);
      this.io.Write(", Additions:");
      foreach (Node a in e.Additions)
      {
        a.Visit(this);
      }
      this.depth--;
      this.io.Write(")");
    }
    public void VisitBinaryExpression(BinaryExpression e)
    {
      /*
      public string Style { get; set; }
      public SimpleExpression Left { get; set; }
      public string RelationalOperator { get; set; }
      public SimpleExpression Right { get; set; }
      */
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{e.Style}>: RelationalOperator: {e.RelationalOperator}, Left:");
      this.depth++;
      Node n = (Node) e.Left;
      n.Visit(this);
      this.io.Write(", Right:");
      n = (Node) e.Right;
      n.Visit(this);
      this.depth--;
      this.io.Write(")");
    }
    public void VisitClosedExpression(ClosedExpression e)
    {
      /*
      public string Style { get; set; }
      public bool Size { get; set; }
      public Expression Expression { get; set; }
      */
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{e.Style}>: Size: {e.Size}, Expression:");
      this.depth++;
      Node n = (Node) e.Expression;
      n.Visit(this);
      this.depth--;
      this.io.Write(")");
    }
    public void VisitSimpleExpressionAddition(SimpleExpressionAddition e)
    {
      /*
      public string AddingOperator { get; set; }
      public Term Term { get; set; }
      public string Style { get; set; }
      */
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{e.Style}>: AddingOperator: {e.AddingOperator}, Term:");
      Node n = (Node) e.Term;
      this.depth++;
      n.Visit(this);
      this.depth--;
      this.io.Write(")");
    }
    public void VisitTerm(Term t)
    {
      /*
      public string Style { get; set; }
      public Factor Factor { get; set; }
      public List<TermMultiplicative> Multiplicatives { get; set; }
      */
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{t.Style}>: Factor:");
      this.depth++;
      Node n = (Node) t.Factor;
      n.Visit(this);
      this.io.Write(", Multiplicatives:");
      foreach (Node m in t.Multiplicatives)
      {
        m.Visit(this);
      }
      this.depth--;
      this.io.Write(")");
    }
    public void VisitTermMultiplicative(TermMultiplicative t)
    {
      /*
      public string MultiplyingOperator { get; set; }
      public Factor Factor { get; set; }
      public string Style { get; set; }
      */
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{t.Style}>: MultiplyingOperator: {t.MultiplyingOperator}, Factor:");
      this.depth++;
      Node n = (Node) t.Factor;
      n.Visit(this);
      this.depth--;
      this.io.Write(")");
    }
    public void VisitIntegerLiteral(IntegerLiteral l)
    {
      /*
      public string Style { get; set; }
      public bool Size { get; set; }
      public int Value { get; set; }
      */
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{l.Style}>: Size: {l.Size}, Value: {l.Value}");
      this.io.Write(")");
    }
    public void VisitStringLiteral(StringLiteral l)
    {
      /*
      public string Style { get; set; }
      public bool Size { get; set; }
      public string Value { get; set; }
      */
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{l.Style}>: Size: {l.Size}, Value: {l.Value}");
      this.io.Write(")");
    }
    public void VisitRealLiteral(RealLiteral l)
    {
      /*
      public string Style { get; set; }
      public bool Size { get; set; }
      public string Value { get; set; }
      */
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{l.Style}>: Size: {l.Size}, Value: {l.Value}");
      this.io.Write(")");
    }
    public void VisitNegationFactor(NegationFactor f)
    {
      /*
      public string Style { get; set; }
      public bool Size { get; set; }
      public Factor Factor { get; set; }
      */
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{f.Style}>: Size: {f.Size}, Factor:");
      Node n = (Node) f.Factor;
      this.depth++;
      n.Visit(this);
      this.depth--;
      this.io.Write(")");
    }
    public void VisitVariable(Variable v)
    {
      /*
      public string Style { get; set; }
      public string Name { get; set; }
      public bool Size { get; set; }
      public Expression IntegerExpression { get; set; }
      */
      string spaces = HandleDepth(this.depth);
      this.io.Write($"\n{spaces}(<{v.Style}>: Name: {v.Name}, Size: {v.Size}, IntegerExpression:");
      if (v.IntegerExpression != null)
      {
        this.depth++;
        Node n = (Node) v.IntegerExpression;
        n.Visit(this);
        this.depth--;
      }
      this.io.Write(")");
    }
    private string HandleDepth(int depth)
    {
      string spaces = "";
      for (int i = 0; i < depth; i++)
      {
        spaces += " ";
      }
      return spaces;
    }
  }
}
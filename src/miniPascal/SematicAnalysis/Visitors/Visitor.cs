using Nodes;

namespace Semantic
{
  public interface Visitor
  {
    void VisitProgram(ProgramNode p);
    void VisitProcedure(Procedure p);
    void VisitReferenceParameter(ReferenceParameter rp);
    void VisitValueParameter(ValueParameter vp);
    void VisitArrayType(ArrayType t);
    void VisitSimpleType(SimpleType t);
    void VisitSimpleExpression(SimpleExpression e);
    void VisitBinaryExpression(BinaryExpression e);
    void VisitClosedExpression(ClosedExpression e);
    void VisitSimpleExpressionAddition(SimpleExpressionAddition e);
    void VisitTerm(Term t);
    void VisitTermMultiplicative(TermMultiplicative t);
    void VisitIntegerLiteral(IntegerLiteral l);
    void VisitStringLiteral(StringLiteral l);
    void VisitRealLiteral(RealLiteral l);
    void VisitNegationFactor(NegationFactor f);
    void VisitVariable(Variable v);
  }
}
using System.Collections.Generic;
using Nodes;

namespace Semantic
{
  public interface Visitor
  {
    void VisitProgram(ProgramNode p);
    void VisitProcedure(Procedure p);
    void VisitFunction(Function f);
    SymbolTableEntry VisitReferenceParameter(ReferenceParameter rp);
    SymbolTableEntry VisitValueParameter(ValueParameter vp);
    BuiltInType VisitArrayType(ArrayType t);
    BuiltInType VisitSimpleType(SimpleType t);
    BuiltInType VisitSimpleExpression(SimpleExpression e);
    BuiltInType VisitBooleanExpression(BooleanExpression e);
    BuiltInType VisitClosedExpression(ClosedExpression e);
    BuiltInType VisitSimpleExpressionAddition(SimpleExpressionAddition e);
    BuiltInType VisitTerm(Term t);
    BuiltInType VisitTermMultiplicative(TermMultiplicative t);
    BuiltInType VisitIntegerLiteral(IntegerLiteral l);
    BuiltInType VisitStringLiteral(StringLiteral l);
    BuiltInType VisitRealLiteral(RealLiteral l);
    BuiltInType VisitNegationFactor(NegationFactor f);
    BuiltInType VisitVariable(Variable v);
    BuiltInType VisitCall(Call c);
    List<BuiltInType> VisitArguments(Arguments a);
    void VisitBlock(Block b, bool needsToReturnValue);
    void VisitAssertStatement(AssertStatement s);
    void VisitAssignmentStatement(AssignmentStatement s);
    void VisitDeclaration(Declaration s);
    void VisitIfStatement(IfStatement s);
    void VisitReadStatement(ReadStatement s);
    void VisitReturnStatement(ReturnStatement s);
    void VisitWhileStatement(WhileStatement s);
    void VisitWriteStatement(WriteStatement s);
  }
}
using System.Linq.Expressions;

namespace MSaver.Application.Common.Specifications;

internal sealed class ReplaceExpressionVisitor(
    Expression oldValue,
    Expression newValue) : ExpressionVisitor
{
    public override Expression? Visit(Expression? node)
    {
        return node == oldValue ? newValue : base.Visit(node);
    }
}
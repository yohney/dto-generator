using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.Logic.Generated.Verification
{
    public abstract class MapperBase<TEntity, TDto>
    {
        public abstract Expression<Func<TEntity, TDto>> SelectorExpression { get; }

        public abstract void MapToModel(TDto dto, TEntity model);
    }

    public static class MapperExtensions
    {
        public static Expression<Func<TEntity, TDto>> MergeWith<TEntity, TDto, TBaseEntity, TBaseDto>(
            this Expression<Func<TEntity, TDto>> expression,
            Expression<Func<TBaseEntity, TBaseDto>> baseExpression)
        {
            var body = ((MemberInitExpression)expression.Body);
            var param = expression.Parameters[0];
            List<MemberBinding> bindings = new List<MemberBinding>(body.Bindings.OfType<MemberAssignment>());

            var baseExpressionBody = (MemberInitExpression)baseExpression.Body;
            var replace = new ParameterReplaceVisitor(baseExpression.Parameters[0], param);
            foreach (var binding in baseExpressionBody.Bindings.OfType<MemberAssignment>())
            {
                bindings.Add(Expression.Bind(binding.Member,
                    replace.VisitAndConvert(binding.Expression, "MergeWith")));
            }

            return Expression.Lambda<Func<TEntity, TDto>>(
                Expression.MemberInit(body.NewExpression, bindings), param);
        }

        class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression from, to;

            public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
            {
                this.from = from;
                this.to = to;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == from ? to : base.VisitParameter(node);
            }
        }
    }
}

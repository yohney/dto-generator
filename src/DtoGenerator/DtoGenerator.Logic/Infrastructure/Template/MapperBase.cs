using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace #Namespace#
{
    public abstract class MapperBase<TEntity, TDto>
    {
        public abstract Expression<Func<TEntity, TDto>> SelectorExpression { get; }

        public abstract void MapToModel(TDto dto, TEntity model);
    }
}

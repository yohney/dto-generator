using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.TestSolution.DAL.Infrastructure
{
    public abstract class MapperBase<TModel, TDto>
    {
        public abstract void MapToModel(TDto dto, TModel model);

        public abstract Expression<Func<TModel, TDto>> SelectorExpression { get; }
    }
}

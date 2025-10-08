using Maman.Core.Entities;
using Maman.Core.Specifications;

namespace Maman.Infrastructure.Specifications;

public class SpecificationEvaluator<T> where T : BaseEntity
{
	public static IFindFluent<T, T> GetQuery(IFindFluent<T, T> inputQuery, ISpecification<T> spec)
	{
		var query = inputQuery;


		if (spec.OrderBy != null)
        {
            query = query.SortBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending != null)
        {
            query = query.SortByDescending(spec.OrderByDescending);
        }
        

        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Limit(spec.Take);
        }

		return query;
	}
}

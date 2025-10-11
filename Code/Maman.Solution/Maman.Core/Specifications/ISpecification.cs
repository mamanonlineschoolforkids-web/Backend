using Maman.Core.Entities;

namespace Maman.Core.Specifications;

public interface ISpecification<T> where T : BaseEntity
{
	Expression<Func<T, bool>> Criteria { get; }
	Expression<Func<T, object>> OrderBy { get; }
	Expression<Func<T, object>> OrderByDescending { get; }
	public int? Skip { get;}
	public int? Take { get; }
	bool IsPagingEnabled { get; }

	//List<Expression<Func<T, object>>> Includes { get; }
}

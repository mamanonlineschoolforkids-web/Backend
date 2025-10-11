using Maman.Core.Entities;

namespace Maman.Core.Specifications;

public class BaseSpecification<T> : ISpecification<T> where T : BaseEntity
{
	public BaseSpecification()
	{
		
	}
	public BaseSpecification(Expression<Func<T, bool>> criteria)
	{
		Criteria = criteria;
	}

	public Expression<Func<T, bool>> Criteria { get; }
	public Expression<Func<T, object>> OrderBy { get; private set; }
	public Expression<Func<T, object>> OrderByDescending { get; private set; }
	public int? Skip { get; private set; }
	public int? Take { get ; private set; }

	public bool IsPagingEnabled { get; private set; }

	//public List<Expression<Func<T, object>>> Includes { get; } = new();
}

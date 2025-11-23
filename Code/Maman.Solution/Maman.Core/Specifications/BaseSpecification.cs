
using System.Linq.Expressions;

namespace Maman.Core.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T>
{
	protected BaseSpecification(Expression<Func<T, bool>> criteria)
	{
		Criteria = criteria;
	}

	public Expression<Func<T, bool>> Criteria { get; }
	public Expression<Func<T, object>>? OrderBy { get; private set; }
	public Expression<Func<T, object>>? OrderByDescending { get; private set; }
	public int Take { get; private set; }
	public int Skip { get; private set; }
	public bool IsPagingEnabled { get; private set; }


	protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
	{
		OrderBy = orderByExpression;
	}
	protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
	{
		OrderByDescending = orderByDescExpression;
	}

	protected void ApplyPaging(int pageNumber, int pageSize)
	{
		Skip =  (pageNumber - 1) * pageSize;
		Take = pageSize;
		IsPagingEnabled = true;
	}

	protected virtual void ApplySorting(string? sort)
	{

	}
}
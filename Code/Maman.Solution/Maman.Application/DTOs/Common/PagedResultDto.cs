using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maman.Application.DTOs.Common
{
	public class PagedResultDto<T>
	{
		public long TotalCount { get; set; }
		public int PageSize { get; set; }

		public int PageNumber { get; set; }
		public List<T> Items { get; set; }

	}
}

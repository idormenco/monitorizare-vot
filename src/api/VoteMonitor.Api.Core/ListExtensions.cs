using System.Collections.Generic;
using System.Linq;

namespace VoteMonitor.Api.Core
{
	public static class ListExtensions
	{
		public static List<T> Paginate<T>(this List<T> list, int page, int pageSize)
		{
			return list
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();
		}
	}

}

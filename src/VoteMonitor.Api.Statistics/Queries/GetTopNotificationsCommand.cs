using MediatR;
using VoteMonitor.Api.Core;
namespace VoteMonitor.Api.Statistics.Queries
{
	internal class GetTopNotificationsCommand : PagingModel, IRequest<object>
	{
		public int CacheHours { get; }

		public int CacheMinutes { get; }

		public int CacheSeconds { get; }

		public GetTopNotificationsCommand(int page, int pageSize, int cacheHours, int cacheMinutes, int cacheSeconds)
		{
			CacheHours = cacheHours;
			CacheMinutes = cacheMinutes;
			CacheSeconds = cacheSeconds;
			PageSize = pageSize;
			Page = page;
		}
	}
}
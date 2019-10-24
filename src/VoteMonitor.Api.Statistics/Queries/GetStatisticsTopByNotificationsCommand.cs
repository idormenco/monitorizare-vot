using MediatR;
using VoteMonitor.Api.Core;
using VoteMonitor.Api.Statistics.Models;

namespace VoteMonitor.Api.Statistics.Queries
{
	public class GetStatisticsTopByNotificationsCommand : PagingModel, IRequest<SimpleStatisticsModel>
	{
		public int CacheHours { get; set; }
		public int CacheMinutes { get; set; }
		public int CacheSeconds { get; set; }
		public bool IsOrganizer { get; set; }
		public int IdNgo { get; set; }
		public StatisticsGroupingType GroupBy { get; set; }
	}
}
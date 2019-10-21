using VoteMonitor.Api.Core;

namespace VoteMonitor.Api.Statistics.Models
{
	public class NotificationsStatisticsFilterModel : PagingModel
	{
		public StatisticsGroupingType GroupBy { get; set; }
	}
}
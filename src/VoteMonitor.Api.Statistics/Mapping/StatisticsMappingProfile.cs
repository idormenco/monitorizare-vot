using AutoMapper;
using VoteMonitor.Api.Statistics.Models;
using VoteMonitor.Entities;

namespace VoteMonitor.Api.Statistics.Mapping
{
	public class StatisticsMappingProfile: Profile
	{
		public StatisticsMappingProfile()
		{
			CreateMap<SimpleStatistics, SimpleStatisticsModel>(MemberList.Source);
			CreateMap<SimpleStatisticsModel, SimpleStatistics>(MemberList.Source);
		}
	}
}

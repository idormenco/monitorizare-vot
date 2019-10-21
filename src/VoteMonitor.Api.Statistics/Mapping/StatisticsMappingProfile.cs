using AutoMapper;
using VoteMonitor.Api.Statistics.Dtos;
using VoteMonitor.Api.Statistics.Models;

namespace VoteMonitor.Api.Statistics.Mapping
{
	public class StatisticsMappingProfile: Profile
	{
		public StatisticsMappingProfile()
		{
			CreateMap<LabeledResponseDto, LabeledResponseModel>(MemberList.Source);
			CreateMap<LabeledResponseModel, LabeledResponseDto>(MemberList.Source);
		}
	}
}

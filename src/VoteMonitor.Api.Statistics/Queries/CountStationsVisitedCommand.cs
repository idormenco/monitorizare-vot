using MediatR;
using VoteMonitor.Api.Statistics.Models;

namespace VoteMonitor.Api.Statistics.Queries
{
	public class CountStationsVisitedCommand : IRequest<LabeledResponseModel>
	{
	}
}
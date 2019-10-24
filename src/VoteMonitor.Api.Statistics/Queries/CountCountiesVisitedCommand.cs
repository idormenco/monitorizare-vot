using MediatR;
using VoteMonitor.Api.Statistics.Models;

namespace VoteMonitor.Api.Statistics.Queries
{
	public class CountCountiesVisitedCommand : IRequest<LabeledResponseModel>
	{
	}
}
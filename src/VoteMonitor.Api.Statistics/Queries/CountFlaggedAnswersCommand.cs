﻿using MediatR;
using VoteMonitor.Api.Statistics.Dtos;

namespace VoteMonitor.Api.Statistics.Queries
{
	public class CountFlaggedAnswersCommand : IRequest<LabeledResponseDto>
	{
	}
}
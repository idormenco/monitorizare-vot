using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoteMonitor.Api.Statistics.Dtos;
using VoteMonitor.Api.Statistics.Queries;
using VoteMonitor.Entities;

namespace VoteMonitor.Api.Statistics.QueriesHandlers
{
	public class VotingStatisticsQueriesHanler :
			IRequestHandler<CountAnswersCommand, LabeledResponseDto>,
			IRequestHandler<CountStationsVisitedCommand, LabeledResponseDto>,
			IRequestHandler<CountFlaggedAnswersCommand, LabeledResponseDto>,
			IRequestHandler<CountCountiesVisitedCommand, LabeledResponseDto>,
			IRequestHandler<CountNotesUploadedCommand, LabeledResponseDto>,
			IRequestHandler<CountLoggedInObserversCommand, LabeledResponseDto>
	{
		private readonly VoteMonitorContext _context;

		public VotingStatisticsQueriesHanler(VoteMonitorContext context)
		{
			_context = context;
		}

		public async Task<LabeledResponseDto> Handle(CountAnswersCommand message, CancellationToken token)
		{
			var number = await _context.Answers.CountAsync();
			return new LabeledResponseDto
			{
				Label = "Number of answers submitted",
				Value = number.ToString()
			};
		}
		public async Task<LabeledResponseDto> Handle(CountStationsVisitedCommand message, CancellationToken token)
		{
			var number = await _context.Answers.Select(r => r.IdPollingStation).Distinct().CountAsync();
			return new LabeledResponseDto
			{
				Label = "Number of Polling Stations visited",
				Value = number.ToString()
			};
		}
		public async Task<LabeledResponseDto> Handle(CountCountiesVisitedCommand message, CancellationToken token)
		{
			var number = await _context.Answers.Select(r => r.CountyCode).Distinct().CountAsync();
			return new LabeledResponseDto
			{
				Label = "Number of Counties visited",
				Value = number.ToString()
			};
		}
		public async Task<LabeledResponseDto> Handle(CountNotesUploadedCommand message, CancellationToken token)
		{
			var number = await _context.Notes.CountAsync();
			return new LabeledResponseDto
			{
				Label = "Number of notes submitted",
				Value = number.ToString()
			};
		}
		public async Task<LabeledResponseDto> Handle(CountLoggedInObserversCommand message, CancellationToken token)
		{
			var number = await _context.PollingStationInfos.Select(pi => pi.IdObserver).Distinct().CountAsync(token);
			return new LabeledResponseDto
			{
				Label = "Number of logged in Observers",
				Value = number.ToString()
			};
		}
		public async Task<LabeledResponseDto> Handle(CountFlaggedAnswersCommand message, CancellationToken token)
		{
			var number = await _context.Answers.Include(r => r.OptionAnswered).CountAsync(r => r.OptionAnswered.Flagged);
			return new LabeledResponseDto
			{
				Label = "Number of flagged answers submitted",
				Value = number.ToString()
			};
		}
	}
}

using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoteMonitor.Api.Core;
using VoteMonitor.Api.Core.Services;
using VoteMonitor.Api.Statistics.Models;
using VoteMonitor.Api.Statistics.Queries;
using VoteMonitor.Entities;

namespace VoteMonitor.Api.Statistics.QueriesHandlers
{
	public class VotingStatisticsQueriesHanler :
			IRequestHandler<CountAnswersCommand, LabeledResponseModel>,
			IRequestHandler<CountStationsVisitedCommand, LabeledResponseModel>,
			IRequestHandler<CountFlaggedAnswersCommand, LabeledResponseModel>,
			IRequestHandler<CountCountiesVisitedCommand, LabeledResponseModel>,
			IRequestHandler<CountNotesUploadedCommand, LabeledResponseModel>,
			IRequestHandler<CountLoggedInObserversCommand, LabeledResponseModel>,
			IRequestHandler<CountNumberOfObserversCommand, ApiListResponse<SimpleStatisticsModel>>

	{
		private readonly VoteMonitorContext _context;
		private readonly ICacheService _cacheService;
		private readonly IMapper _mapper;

		public VotingStatisticsQueriesHanler(ICacheService cacheService, VoteMonitorContext context, IMapper mapper)
		{
			_context = context;
			_cacheService = cacheService;
			_mapper = mapper;
		}

		public async Task<LabeledResponseModel> Handle(CountAnswersCommand message, CancellationToken token)
		{
			var number = await _context.Answers.CountAsync();
			return new LabeledResponseModel
			{
				Label = "Number of answers submitted",
				Value = number.ToString()
			};
		}
		public async Task<LabeledResponseModel> Handle(CountStationsVisitedCommand message, CancellationToken token)
		{
			var number = await _context.Answers.Select(r => r.IdPollingStation).Distinct().CountAsync();
			return new LabeledResponseModel
			{
				Label = "Number of Polling Stations visited",
				Value = number.ToString()
			};
		}
		public async Task<LabeledResponseModel> Handle(CountCountiesVisitedCommand message, CancellationToken token)
		{
			var number = await _context.Answers.Select(r => r.CountyCode).Distinct().CountAsync();
			return new LabeledResponseModel
			{
				Label = "Number of Counties visited",
				Value = number.ToString()
			};
		}
		public async Task<LabeledResponseModel> Handle(CountNotesUploadedCommand message, CancellationToken token)
		{
			var number = await _context.Notes.CountAsync();
			return new LabeledResponseModel
			{
				Label = "Number of notes submitted",
				Value = number.ToString()
			};
		}
		public async Task<LabeledResponseModel> Handle(CountLoggedInObserversCommand message, CancellationToken token)
		{
			var number = await _context.PollingStationInfos.Select(pi => pi.IdObserver).Distinct().CountAsync(token);
			return new LabeledResponseModel
			{
				Label = "Number of logged in Observers",
				Value = number.ToString()
			};
		}
		public async Task<LabeledResponseModel> Handle(CountFlaggedAnswersCommand message, CancellationToken token)
		{
			var number = await _context.Answers.Include(r => r.OptionAnswered).CountAsync(r => r.OptionAnswered.Flagged);
			return new LabeledResponseModel
			{
				Label = "Number of flagged answers submitted",
				Value = number.ToString()
			};
		}

		public async Task<ApiListResponse<SimpleStatisticsModel>> Handle(CountNumberOfObserversCommand request, CancellationToken cancellationToken)
		{
			var cacheKey = "statistici-observatori";

			var query = @"select count(distinct a.IdObserver) as [Value], CountyCode as Label
						  FROM Answers a (NOLOCK)
						  INNER JOIN Observers o on a.IdObserver = o.Id
						  WHERE @isOrganizer = 0 OR O.IdNgo = @idNgo
						  GROUP BY CountyCode 
						  ORDER BY [Value] DESC";

			if (!request.IsOrganizer)
			{
				cacheKey = $"{cacheKey}-{request.IdNgo}";
			}
			else
			{
				cacheKey = $"{cacheKey}-Organizer";
			}

			// get or save all records in cache
			var records = await _cacheService.GetOrSaveDataInCacheAsync(cacheKey,
				async () =>
				{
					return await _context.Answers
					.FromSql(query, new SqlParameter("@isOrganizer", request.IsOrganizer), new SqlParameter("@idNgo", request.IdNgo))
					.ToListAsync();
				},
				new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = new TimeSpan(request.CacheHours, request.CacheMinutes, request.CacheMinutes)
				}
			);

			// perform count and pagination on the records retrieved from the cache 
			var pagedList = records.Paginate(request.Page, request.PageSize);

			return new ApiListResponse<SimpleStatisticsModel>
			{
				Data = pagedList.Select(x => _mapper.Map<SimpleStatisticsModel>(x)).ToList(),
				Page = request.Page,
				PageSize = request.PageSize,
				TotalItems = records.Count()
			};
		}
	}
}

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
			var cacheKey = "statistics-observers";

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
					return await _context.SimpleStatistics
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

		public async Task<ApiListResponse<SimpleStatisticsModel>> Handle(GetStatisticsTopByNotificationsCommand request, CancellationToken token)
		{
			return request.GroupBy == StatisticsGroupingType.County
				? await GetTopCountiesByNumberOfNotifications(request, token)
				: await GetSesizariSectii(request, token);
		}

		private async Task<ApiListResponse<SimpleStatisticsModel>> GetTopCountiesByNumberOfNotifications(GetStatisticsTopByNotificationsCommand request, CancellationToken token)
		{
			var cacheKey = "statistics-conties";
			var query = @"SELECT R.CountyCode AS Label, COUNT(*) as Value
                  FROM Answers AS R 
                  INNER JOIN OptionsToQuestions AS RD ON RD.Id = R.IdOptionToQuestion
                  INNER JOIN Observers O ON O.Id = R.IdObserver
                  INNER JOIN Questions Q ON Q.Id = RD.IdQuestion
                  WHERE RD.Flagged = 1 AND (@isOrganiser = 1 OR O.IdNgo = @idNgo) AND Q.FormCode = @formCode
				  GROUP BY R.CountyCode ORDER BY Value DESC";

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
				async () => await _context.SimpleStatistics
					.FromSql(query)
					.ToListAsync(),
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

		private async Task<ApiListResponse<SimpleStatisticsModel>> GetSesizariSectii(GetStatisticsTopByNotificationsCommand request, CancellationToken token)
		{
			var query = $@"SELECT R.CountyCode AS Label, R.PollingStationNumber AS Cod, COUNT(*) as Value
                  FROM Answers AS R 
                  INNER JOIN OptionsToQuestions AS RD ON RD.Id = R.IdOptionToQuestion
                  INNER JOIN Observers O ON O.Id = R.IdObserver
                  INNER JOIN Questions I ON I.Id = RD.IdQuestion
                  WHERE RD.Flagged = 1 AND (@isOrganiser = 1 OR O.IdNgo = @idNgo) AND Q.FormCode = @formCode
                  GROUP BY R.CountyCode, R.PollingStationNumber";

			var paginatedQuery = $"{query} ORDER BY Value DESC OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

			var cacheKey = "statistics-polling-stations";

			if (!request.IsOrganizer)
			{
				cacheKey = $"{cacheKey}-{request.IdNgo}";
			}
			else
			{
				cacheKey = $"{cacheKey}-Organizer";
			}
			// get or save paginated response in cache
			return await _cacheService.GetOrSaveDataInCacheAsync($"{cacheKey}-{request.Page}",
				async () =>
				{
					var records = await _context.PollingStationsStatistics
						.FromSql(paginatedQuery, new SqlParameter("@id", 2))
						.ToListAsync();

					return new ApiListResponse<SimpleStatisticsModel>
					{
						Data = records.Select(x => _mapper.Map<SimpleStatisticsModel>(x)).ToList(),
						Page = request.Page,
						PageSize = request.PageSize,
						TotalItems = await _context.PollingStationsStatistics.FromSql(query, new SqlParameter("@id", 2)).CountAsync()
					};
				},
				new DistributedCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = new TimeSpan(request.CacheHours, request.CacheMinutes, request.CacheMinutes)
				}
			);
		}
	}
}

using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoteMonitor.Api.Statistics.Dtos;
using VoteMonitor.Api.Statistics.Models;
using VoteMonitor.Api.Statistics.Options;
using VoteMonitor.Api.Statistics.Queries;

namespace VoteMonitor.Api.Statistics.Controllers
{
	[Route("api/v1/statistics")]
	public class StatisticsController : Controller
	{
		private readonly IMediator _mediator;
		private readonly StatisticsCachingOptions _options;
		private readonly ILogger _logger;
		private readonly IMapper _mapper;

		public StatisticsController(IMediator mediator
			, IOptions<StatisticsCachingOptions> options
			, ILogger logger
			, IMapper mapper)
		{
			_mediator = mediator;
			_options = options.Value;
			_logger = logger;
			_mapper = mapper;
		}


		[HttpGet]
		[Route("notifications")]
		public async Task<object> GetNotifications(NotificationsStatisticsFilterModel model)
		{
			var command = new GetTopNotificationsCommand(
				model.Page,
				model.PageSize,
				_options.CacheHours,
				_options.CacheMinutes,
				_options.CacheSeconds
				);


			var result = await _mediator.Send(command);


			return null;
		}

		[HttpGet]
		[AllowAnonymous]
		[Route("mini/answers")]
		public async Task<LabeledResponseModel> Answers()
		{
			var response = await _mediator.Send(new CountAnswersCommand());

			return _mapper.Map<LabeledResponseModel>(response);
		}
		[HttpGet]
		[AllowAnonymous]
		[Route("mini/stations")]
		public async Task<LabeledResponseModel> StationsVisited()
		{
			var response = await _mediator.Send(new CountStationsVisitedCommand());
			
			return _mapper.Map<LabeledResponseModel>(response);
		}
		[HttpGet]
		[AllowAnonymous]
		[Route("mini/counties")]
		public async Task<LabeledResponseModel> Counties()
		{
			var response = await _mediator.Send(new CountCountiesVisitedCommand());

			return _mapper.Map<LabeledResponseModel>(response);
		}
		[HttpGet]
		[AllowAnonymous]
		[Route("mini/notes")]
		public async Task<LabeledResponseModel> Notes()
		{
			var response = await _mediator.Send(new CountNotesUploadedCommand());

			return _mapper.Map<LabeledResponseModel>(response);
		}
		[HttpGet]
		[AllowAnonymous]
		[Route("mini/loggedinobservers")]
		public async Task<LabeledResponseModel> LoggedInObservers()
		{
			var response = await _mediator.Send(new CountLoggedInObserversCommand());

			return _mapper.Map<LabeledResponseModel>(response);
		}
		[HttpGet]
		[AllowAnonymous]
		[Route("mini/flaggedanswers")]
		public async Task<LabeledResponseModel> FlaggedAnswers()
		{
			var response =  await _mediator.Send(new CountFlaggedAnswersCommand());

			return _mapper.Map<LabeledResponseModel>(response);
		}

		[HttpGet]
		[AllowAnonymous]
		[Route("mini/all")]
		public async Task<List<LabeledResponseModel>> All()
		{
			var list = new List<LabeledResponseDto>
			{
				await _mediator.Send(new CountAnswersCommand()),
				await _mediator.Send(new CountStationsVisitedCommand()),
				await _mediator.Send(new CountFlaggedAnswersCommand()),
				await _mediator.Send(new CountLoggedInObserversCommand()),
				await _mediator.Send(new CountNotesUploadedCommand()),
				await _mediator.Send(new CountCountiesVisitedCommand())
			};
			var mappedList = _mapper.Map<List<LabeledResponseModel>>(list);

			return mappedList;
		}
	}
}

using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoteMonitor.Api.Core;
using VoteMonitor.Api.Statistics.Models;
using VoteMonitor.Api.Statistics.Options;
using VoteMonitor.Api.Statistics.Queries;

namespace VoteMonitor.Api.Statistics.Controllers
{
	[Route("api/v1/statistics")]
	public class StatisticsController : Controller
	{
		private readonly IMediator _mediator;
		private readonly StatisticsOptions _options;
		private readonly ILogger _logger;
		private readonly IMapper _mapper;
		private readonly IConfigurationRoot _configuration;

		public StatisticsController(IMediator mediator
			, IOptions<StatisticsOptions> options
			, ILogger logger
			, IMapper mapper
			, IConfigurationRoot configuration)
		{
			_mediator = mediator;
			_options = options.Value;
			_logger = logger;
			_mapper = mapper;
			_configuration = configuration;
		}

		[HttpGet]
		[Route("NumberOfObservers")]
		public async Task<ApiListResponse<SimpleStatisticsModel>> NumberOfObservers(PagingModel model)
		{
			var idNgo = this.GetIdOngOrDefault(_configuration.GetValue<int>("DefaultIdOng"));
			var isOrganiser= this.GetOrganizatorOrDefault(_configuration.GetValue<bool>("DefaultOrganizator"));

			return await _mediator.Send(new CountNumberOfObserversCommand
			{
				IdNgo = idNgo,
				IsOrganizer = isOrganiser,
				PageSize = model.PageSize,
				Page = model.Page,
				CacheHours = _options.CacheHours,
				CacheMinutes = _options.CacheMinutes,
				CacheSeconds = _options.CacheSeconds
			});
		}

		[HttpGet]
		[Route("notifications")]
		public async Task<object> GetNotifications(NotificationsStatisticsFilterModel model)
		{
			var idNgo = this.GetIdOngOrDefault(_configuration.GetValue<int>("DefaultIdOng"));
			var isOrganiser = this.GetOrganizatorOrDefault(_configuration.GetValue<bool>("DefaultOrganizator"));

			var command = new GetStatisticsTopByNotificationsCommand()
			{
				IdNgo = idNgo,
				IsOrganizer = isOrganiser,
				Page = model.Page,
				PageSize = model.PageSize,
				GroupBy =model.GroupBy,
				CacheHours = _options.CacheHours,
				CacheMinutes =_options.CacheMinutes,
				CacheSeconds = _options.CacheSeconds,
			};


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
			var list = new List<LabeledResponseModel>
			{
				await _mediator.Send(new CountAnswersCommand()),
				await _mediator.Send(new CountStationsVisitedCommand()),
				await _mediator.Send(new CountFlaggedAnswersCommand()),
				await _mediator.Send(new CountLoggedInObserversCommand()),
				await _mediator.Send(new CountNotesUploadedCommand()),
				await _mediator.Send(new CountCountiesVisitedCommand())
			};
			
			return list;
		}
	}
}

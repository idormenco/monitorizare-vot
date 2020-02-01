using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VoteMonitor.Api.Core.Models;
using VoteMonitor.Api.Ngo.Commands;
using VoteMonitor.Api.Ngo.Models;
using VoteMonitor.Api.Ngo.Queries;

namespace VoteMonitor.Api.Ngo.Controllers
{
    [Route("api/v1/ngo")]
    public class NgoAdminController : Controller
    {
        private readonly IMediator _mediator;

        public NgoAdminController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        [Route("{idNgo}/ngoadmin/")]
        //[Authorize("NgoAdmin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<NgoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllNgosAsync(int idNgo)
        {
            var ngosListResult = await _mediator.Send(new GetAllNgoAdmins(idNgo));

            if (ngosListResult.IsFailure)
            {
                return BadRequest(new ErrorModel { Message = ngosListResult.Error });
            }

            return Ok(ngosListResult.Value);
        }


        [HttpGet]
        [Route("{idNgo}/ngoadmin/{ngoAdminId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(NgoModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNgoByIdAsync([FromRoute]int idNgo, [FromRoute]int ngoAdminId)
        {
            var ngoDetailsResult = await _mediator.Send(new GetNgoAdminDetails(idNgo, ngoAdminId));

            if (ngoDetailsResult.IsFailure)
            {
                return BadRequest(new ErrorModel { Message = ngoDetailsResult.Error });
            }

            return Ok(ngoDetailsResult.Value);
        }

        [HttpPost]
        [Route("{idNgo}/ngoadmin/{ngoAdminId}")]
        //[Authorize("NgoAdmin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<NgoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateNgoAdminAsync([FromRoute]int idNgo, [FromRoute]int ngoAdminId, [FromBody]CreateUpdateNgoAdminModel model)
        {
            var result = await _mediator.Send(new UpdateNgoAdmin(idNgo, ngoAdminId, model));

            if (result.IsFailure)
            {
                return BadRequest(new ErrorModel { Message = result.Error });
            }

            return Ok();
        }

        [HttpPost]
        [Route("{idNgo}/ngoadmin")]
        //[Authorize("NgoAdmin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<NgoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateNgoAdminAsync([FromRoute]int idNgo, [FromBody]CreateUpdateNgoAdminModel model)
        {
            var result = await _mediator.Send(new CreateNgoAdmin(idNgo, model));

            if (result.IsFailure)
            {
                return BadRequest(new ErrorModel { Message = result.Error });
            }

            return Ok();
        }

        [HttpDelete]
        [Route("{idNgo}/ngoadmin/{ngoAdminId}")]
        //[Authorize("NgoAdmin")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<NgoModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteNgoAdminAsync([FromRoute]int idNgo, [FromRoute]int ngoAdminId)
        {
            var result = await _mediator.Send(new DeleteNgoAdmin(idNgo, ngoAdminId));

            if (result.IsFailure)
            {
                return BadRequest(new ErrorModel { Message = result.Error });
            }

            return Ok();
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoteMonitor.Api.Ngo.Models;
using VoteMonitor.Api.Ngo.Queries;
using VoteMonitor.Entities;

namespace VoteMonitor.Api.Ngo.Handlers
{
    public class NgoQueryHandler : IRequestHandler<GetAllNgos, Result<List<NgoModel>>>,
        IRequestHandler<GetNgoDetails, Result<NgoModel>>
    {
        private readonly VoteMonitorContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public NgoQueryHandler(VoteMonitorContext context, IMapper mapper, ILogger logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<NgoModel>>> Handle(GetAllNgos request, CancellationToken cancellationToken)
        {
            try
            {
                var listAsync = await _context.Ngos.Select(x => _mapper.Map<NgoModel>(x)).ToListAsync(cancellationToken);
                return Result.Ok(listAsync);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when loading all ngos");
                return Result.Failure<List<NgoModel>>("Could not load ngos.");
            }
        }

        public async Task<Result<NgoModel>> Handle(GetNgoDetails request, CancellationToken cancellationToken)
        {
            try
            {
                var ngo = await _context.Ngos.FirstOrDefaultAsync(x => x.Id == request.NgoId, cancellationToken);
                if (ngo == null)
                {
                    return Result.Failure<NgoModel>($"Could not find ngo with id {request.NgoId}");

                }

                var mappedNgo = _mapper.Map<NgoModel>(ngo);
                return Result.Ok(mappedNgo);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when loading all ngos");
                return Result.Failure<NgoModel>($"Could not load info for ngo with id {request.NgoId}");
            }
        }
    }
}
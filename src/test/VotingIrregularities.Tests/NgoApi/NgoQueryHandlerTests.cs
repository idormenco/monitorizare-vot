using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using VoteMonitor.Entities;
using Microsoft.Extensions.Logging;
using Shouldly;
using VoteMonitor.Api.Ngo.Handlers;
using VoteMonitor.Api.Ngo.Mappers;
using VoteMonitor.Api.Ngo.Queries;
using Xunit;

namespace VotingIrregularities.Tests.NgoApi
{
    public class NgoQueryHandlerTests
    {

        Mock<ILogger> _fakeLogger = new Mock<ILogger>();
        private readonly DbContextOptions<VoteMonitorContext> _dbContextOptions;
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _configuration = new MapperConfiguration(cfg =>
            cfg.AddProfile(new NgoMapping()));

        public NgoQueryHandlerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<VoteMonitorContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _mapper = new Mapper(_configuration);
        }


        [Fact]
        public async Task Should_fetch_all_ngos()
        {
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                context.Ngos.Add(new Ngo { Id = 3, IsActive = false, Name = "Name 3", NgoAdmins = new List<NgoAdmin>(), Observers = new List<Observer>(), Organizer = false, ShortName = "ShortName3" });
                context.Ngos.Add(new Ngo { Id = 2, IsActive = true, Name = "Name 2", NgoAdmins = new List<NgoAdmin>(), Observers = new List<Observer>(), Organizer = true, ShortName = "ShortName2" });
                context.Ngos.Add(new Ngo { Id = 1, IsActive = true, Name = "Name 1", NgoAdmins = new List<NgoAdmin>(), Observers = new List<Observer>(), Organizer = false, ShortName = "ShortName1" });
                context.SaveChanges();
            }

            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var ngoQueryHandler = new NgoQueryHandler(context, _mapper, _fakeLogger.Object);
                var ngos = await ngoQueryHandler.Handle(new GetAllNgos(), new CancellationToken(false));

                ngos.IsSuccess.ShouldBeTrue();
                ngos.Value.Count.ShouldBe(3);

                var ngo3 = ngos.Value[0];
                ngo3.Id.ShouldBe(3);
                ngo3.IsActive.ShouldBeFalse();
                ngo3.Name.ShouldBe("Name 3");
                ngo3.Organizer.ShouldBeFalse();
                ngo3.ShortName.ShouldBe("ShortName3");


                var ngo2 = ngos.Value[1];
                ngo2.Id.ShouldBe(2);
                ngo2.IsActive.ShouldBeTrue();
                ngo2.Name.ShouldBe("Name 2");
                ngo2.Organizer.ShouldBeTrue();
                ngo2.ShortName.ShouldBe("ShortName2");

                var ngo1 = ngos.Value[2];
                ngo1.Id.ShouldBe(1);
                ngo1.IsActive.ShouldBeTrue();
                ngo1.Name.ShouldBe("Name 1");
                ngo1.Organizer.ShouldBeFalse();
                ngo1.ShortName.ShouldBe("ShortName1");

            }
        }


        [Fact]
        public async Task Should_return_failed_result_when_somenthing_went_wrong()
        {
            var ngoQueryHandler = new NgoQueryHandler(null, _mapper, _fakeLogger.Object);
            var ngos = await ngoQueryHandler.Handle(new GetAllNgos(), new CancellationToken(false));

            ngos.IsSuccess.ShouldBeFalse();
            ngos.Error.ShouldBe("Could not load ngos.");
        }

        [Fact]
        public async Task GetNgoDetails_should_fetch_ngo_by_id_if_exists()
        {
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                context.Ngos.Add(new Ngo { Id = 3, IsActive = false, Name = "Name 3", NgoAdmins = new List<NgoAdmin>(), Observers = new List<Observer>(), Organizer = false, ShortName = "ShortName3" });
                context.Ngos.Add(new Ngo { Id = 2, IsActive = true, Name = "Name 2", NgoAdmins = new List<NgoAdmin>(), Observers = new List<Observer>(), Organizer = true, ShortName = "ShortName2" });
                context.Ngos.Add(new Ngo { Id = 1, IsActive = true, Name = "Name 1", NgoAdmins = new List<NgoAdmin>(), Observers = new List<Observer>(), Organizer = false, ShortName = "ShortName1" });
                context.SaveChanges();
            }

            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var ngoQueryHandler = new NgoQueryHandler(context, _mapper, _fakeLogger.Object);
                var ngo = await ngoQueryHandler.Handle(new GetNgoDetails(2), new CancellationToken(false));

                ngo.IsSuccess.ShouldBeTrue();

                ngo.Value.Id.ShouldBe(2);
                ngo.Value.IsActive.ShouldBeTrue();
                ngo.Value.Name.ShouldBe("Name 2");
                ngo.Value.Organizer.ShouldBeTrue();
                ngo.Value.ShortName.ShouldBe("ShortName2");
            }
        }

        [Fact]
        public async Task GetNgoDetails_should_return_failed_result_when_request_with_inexistent_id()
        {
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                context.Ngos.Add(new Ngo { Id = 3, IsActive = false, Name = "Name 3", NgoAdmins = new List<NgoAdmin>(), Observers = new List<Observer>(), Organizer = false, ShortName = "ShortName3" });
                context.Ngos.Add(new Ngo { Id = 2, IsActive = true, Name = "Name 2", NgoAdmins = new List<NgoAdmin>(), Observers = new List<Observer>(), Organizer = true, ShortName = "ShortName2" });
                context.Ngos.Add(new Ngo { Id = 1, IsActive = true, Name = "Name 1", NgoAdmins = new List<NgoAdmin>(), Observers = new List<Observer>(), Organizer = false, ShortName = "ShortName1" });
                context.SaveChanges();
            }

            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var ngoQueryHandler = new NgoQueryHandler(context, _mapper, _fakeLogger.Object);
                var ngo = await ngoQueryHandler.Handle(new GetNgoDetails(55), new CancellationToken(false));

                ngo.IsSuccess.ShouldBeFalse();

                ngo.Error.ShouldBe("Could not find ngo with id 55");
            }
        }

        [Fact]
        public async Task GetNgoDetails_should_return_failed_result_when_something_goes_wrong()
        {
            var ngoQueryHandler = new NgoQueryHandler(null, _mapper, _fakeLogger.Object);
            var ngo = await ngoQueryHandler.Handle(new GetNgoDetails(55), new CancellationToken(false));

            ngo.IsSuccess.ShouldBeFalse();

            ngo.Error.ShouldBe("Error when loading info for ngo with id 55");
        }

    }
}
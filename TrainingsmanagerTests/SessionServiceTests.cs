using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Moq;
using Trainingsmanager.Database.Enums;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Helper;
using Trainingsmanager.Mappers;
using Trainingsmanager.Models;
using Trainingsmanager.Options;
using Trainingsmanager.Repositories;
using Trainingsmanager.Services;

public class SessionServiceTests
{
    private readonly Mock<ISessionRepository> _repositoryMock = new();
    private readonly Mock<ISessionMapper> _mapperMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock = new();
    private readonly Mock<ISessionGroupRepository> _sessionGroupRepoMock = new();
    private readonly Mock<ISessionHelper> _helperMock = new();
    private readonly Mock<ISubscriptionMapper> _subscriptionMapperMock = new();
    private readonly Mock<ISubscriptionService> _subscriptionServiceMock = new();
    private readonly Mock<ILogger<ISessionService>> _loggerMock = new();
    private readonly IOptions<FixedSubsOptions> _fixedSubsOptions = Options.Create(new FixedSubsOptions
    {
        FixedSubs = new List<string> { "Alice", "Bob" }
    });

    public SessionServiceTests()
    {
        
    }

    [Fact]
    public async Task GetAllSessionsAsync_ShouldCallRepositoryAndReturnMappedResponse()
    {
        // Arrange
        var service = new SessionService(
            _repositoryMock.Object,
            _mapperMock.Object,
            _userServiceMock.Object,
            _subscriptionRepoMock.Object,
            _fixedSubsOptions,
            _helperMock.Object,
            _sessionGroupRepoMock.Object,
            _subscriptionMapperMock.Object,
            _subscriptionServiceMock.Object,
            _loggerMock.Object
        );
        var sessionList = new List<Session> { new Session { Id = Guid.NewGuid(), Teamname = "Test Team" } };
        _repositoryMock.Setup(r => r.GetAllSessions(It.IsAny<CancellationToken>())).ReturnsAsync(sessionList);

        var expectedResponse = new GetAllSessionsResponse();
        _mapperMock.Setup(m => m.ListOfSessionToListOfCreateSessionResponse(sessionList, It.IsAny<CancellationToken>()))
                   .Returns(expectedResponse);

        // Act
        var result = await service.GetAllSessionsAsync(CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _repositoryMock.Verify(r => r.GetAllSessions(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.ListOfSessionToListOfCreateSessionResponse(sessionList, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllSessionByIdAsync_ShouldCallRepositoryAndReturnMappedResponse()
    {
        // Arrange
        var service = new SessionService(
            _repositoryMock.Object,
            _mapperMock.Object,
            _userServiceMock.Object,
            _subscriptionRepoMock.Object,
            _fixedSubsOptions,
            _helperMock.Object,
            _sessionGroupRepoMock.Object,
            _subscriptionMapperMock.Object,
            _subscriptionServiceMock.Object,
            _loggerMock.Object
        );
        var guidOfNewSession = Guid.NewGuid();
        var session = new Session { Id = guidOfNewSession, Teamname = "Test Team" };

        _repositoryMock.Setup(r => r.GetSessionByIdAsync(guidOfNewSession, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var expectedResponse = new GetSessionResponse();
        _mapperMock.Setup(m => m.SessionToGetSessionResponse(session, It.IsAny<CancellationToken>()))
                   .Returns(expectedResponse);

        // Act
        var result = await service.GetSessionByIdAsync(guidOfNewSession, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _repositoryMock.Verify(r => r.GetSessionByIdAsync(guidOfNewSession, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.SessionToGetSessionResponse(session, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSessionAsync_ThrowsArgumentNullException_WhenUserIsNull()
    {
        _userServiceMock.Setup(u => u.User).Returns((ClaimsPrincipal?)null);

        var fixedSubsOptions = Options.Create(new FixedSubsOptions
        {
            FixedSubs = new List<string>() { "John", "Jane" }
        });

        var service = new SessionService(
           _repositoryMock.Object,
           _mapperMock.Object,
           _userServiceMock.Object,
           _subscriptionRepoMock.Object,
           _fixedSubsOptions,
           _helperMock.Object,
           _sessionGroupRepoMock.Object,
           _subscriptionMapperMock.Object,
           _subscriptionServiceMock.Object,
           _loggerMock.Object
       );

        var request = new CreateSessionRequest
        {
            Teamname = "TestTeam",
            ApplicationsLimit = 10,
            CountSessionsToCreate = 1,
            TrainingStart = DateTime.UtcNow,
            TrainingEnd = DateTime.UtcNow.AddHours(1),
            PreAddMitglieder = false,
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateSessionAsync(request, CancellationToken.None));
        Assert.Contains("Es konnte kein angemeldeter Nutzer ermittlert werden.", ex.Message);
    }

    [Fact]
    public async Task CreateSessionAsync_CreateSessionsSuccessful()
    {
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Name, "Test User")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity); 

        _userServiceMock.Setup(u => u.User).Returns(claimsPrincipal);

        var fixedSubsOption = new FixedSubsOptions { FixedSubs = new List<string> { "Alice", "Bob", "Charlie" } };
        var options = Options.Create(fixedSubsOption);

        var request = new CreateSessionRequest
        {
            Teamname = "Team A",
            TrainingStart = DateTime.UtcNow,
            TrainingEnd = DateTime.UtcNow.AddHours(1),
            ApplicationsLimit = 10,
            ApplicationsRequired = 2,
            PreAddMitglieder = false,
            CountSessionsToCreate = 2
        };

        var createdSession1 = new Session { Id = Guid.NewGuid(), Teamname = "Team A" };
        var createdSession2 = new Session { Id = Guid.NewGuid(), Teamname = "Team A" };

        _helperMock.SetupSequence(h => h.AddWeeksToDates(It.IsAny<CreateSessionRequest>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(request)
            .Returns(request);

        _repositoryMock.SetupSequence(r => r.CreateSessionAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSession1)
            .ReturnsAsync(createdSession2);

        _mapperMock.Setup(m => m.CreateSessionRequestToSession(It.IsAny<CreateSessionRequest>(), claimsPrincipal, It.IsAny<CancellationToken>()))
            .Returns((CreateSessionRequest req, ClaimsPrincipal user, CancellationToken _) =>
                new Session { Id = Guid.NewGuid(), Teamname = req.Teamname });

        _mapperMock.Setup(m => m.SessionToCreateSessionResponse(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .Returns((Session s, CancellationToken _) => new CreateSessionResponse { Id = s.Id });

        var service = new SessionService(
           _repositoryMock.Object,
           _mapperMock.Object,
           _userServiceMock.Object,
           _subscriptionRepoMock.Object,
           options,
           _helperMock.Object,
           _sessionGroupRepoMock.Object,
           _subscriptionMapperMock.Object,
           _subscriptionServiceMock.Object,
           _loggerMock.Object
       );

        // Act
        var result = await service.CreateSessionAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Sessions.Count);
        Assert.Contains(result.Sessions, s => s.Id == createdSession1.Id);
        Assert.Contains(result.Sessions, s => s.Id == createdSession2.Id);

        _repositoryMock.Verify(r => r.CreateSessionAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _sessionGroupRepoMock.Verify(g => g.CreateSessionGroupAsync(It.IsAny<List<Session>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSessionAsync_ThrowsArgumentException_WhenApplicationsLimitTooLowForUpdate()
    {
        _userServiceMock.Setup(u => u.User).Returns(new ClaimsPrincipal());

        var fixedSubsOption = new FixedSubsOptions { FixedSubs = new List<string> { "Alice", "Bob", "Charlie" } };
        var options = Options.Create(fixedSubsOption);

        var sessionId = Guid.NewGuid();
        var session = new Session 
        { 
            Id = sessionId, 
            Teamname = "My Team", 
            ApplicationsLimit = 4,
            Subscriptions = [new Subscription { SubscriptionType = SubscriptionType.Angemeldet }, new Subscription { SubscriptionType = SubscriptionType.Angemeldet }, new Subscription{ SubscriptionType = SubscriptionType.Angemeldet }, new Subscription{ SubscriptionType = SubscriptionType.Angemeldet }] // Add 4 subscriptions
        };

        _repositoryMock.Setup(r => r.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var service = new SessionService(
           _repositoryMock.Object,
           _mapperMock.Object,
           _userServiceMock.Object,
           _subscriptionRepoMock.Object,
           options,
           _helperMock.Object,
           _sessionGroupRepoMock.Object,
           _subscriptionMapperMock.Object,
           _subscriptionServiceMock.Object,
           _loggerMock.Object
       );

        var request = new UpdateSessionRequest
        {
            Id = sessionId,
            ApplicationsLimit = 2, // limit lower than already subscribed list amount
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateSessionAsync(request, CancellationToken.None));
        Assert.Contains($"Das Limit muss mindestens die Anzahl an angemeldeten Teilnahmen ({session.Subscriptions.Count()}) betragen.", ex.Message);
    }
}
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Trainingsmanager.Database.Models;
using Trainingsmanager.Helper;
using Trainingsmanager.Mappers;
using Trainingsmanager.Models;
using Trainingsmanager.Options;
using Trainingsmanager.Repositories;
using Trainingsmanager.Services;

public class SessionServiceTests
{
    private readonly SessionService _service;
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
        _service = new SessionService(
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
    }

    [Fact]
    public async Task GetAllSessionsAsync_ShouldCallRepositoryAndReturnMappedResponse()
    {
        // Arrange
        var sessionList = new List<Session> { new Session { Id = Guid.NewGuid(), Teamname = "Test Team" } };
        _repositoryMock.Setup(r => r.GetAllSessions(It.IsAny<CancellationToken>())).ReturnsAsync(sessionList);

        var expectedResponse = new GetAllSessionsResponse();
        _mapperMock.Setup(m => m.ListOfSessionToListOfCreateSessionResponse(sessionList, It.IsAny<CancellationToken>()))
                   .Returns(expectedResponse);

        // Act
        var result = await _service.GetAllSessionsAsync(CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _repositoryMock.Verify(r => r.GetAllSessions(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.ListOfSessionToListOfCreateSessionResponse(sessionList, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllSessionByIdAsync_ShouldCallRepositoryAndReturnMappedResponse()
    {
        // Arrange
        var guidOfNewSession = Guid.NewGuid();
        var session = new Session { Id = guidOfNewSession, Teamname = "Test Team" };

        _repositoryMock.Setup(r => r.GetSessionByIdAsync(guidOfNewSession, It.IsAny<CancellationToken>())).ReturnsAsync(session);

        var expectedResponse = new GetSessionResponse();
        _mapperMock.Setup(m => m.SessionToGetSessionResponse(session, It.IsAny<CancellationToken>()))
                   .Returns(expectedResponse);

        // Act
        var result = await _service.GetSessionByIdAsync(guidOfNewSession, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        _repositoryMock.Verify(r => r.GetSessionByIdAsync(guidOfNewSession, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.SessionToGetSessionResponse(session, It.IsAny<CancellationToken>()), Times.Once);
    }
}
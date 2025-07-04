using Trainingsmanager.Mappers;
using Trainingsmanager.Models;
using Trainingsmanager.Repositories;

namespace Trainingsmanager.Services
{
    public class SessionTemplateService : ISessionTemplateService
    {
        private readonly ISessionTemplateRepository _repository;
        private readonly ISessionTemplateMapper _mapper;
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public SessionTemplateService(ISessionTemplateRepository repository, ISessionTemplateMapper mapper, IUserService userService, ILogger<SessionTemplateService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _userService = userService;
            _logger = logger;
        }

        public async Task<CreateSessionTemplateResponse> CreateSessionTemplateAsync(CreateSessionTemplateRequest request, CancellationToken ct)
        {
            if (_userService.User == null)
            {
                throw new ArgumentNullException("No user found");
            }

            if (request.TemplateName == null)
            {
                throw new ArgumentNullException("Templatename must not be null");
            }

            var templateWithSameName = await _repository.GetSessionTemplateByNameAsync(request.TemplateName, ct);

            if (templateWithSameName != null)
            {
                throw new ArgumentException("Es existiert bereits ein Template mit dem gleichen Namen!");
            }

            var templateRequest = _mapper.CreateSessionTemplateRequestToSessionTemplate(request, _userService.User, ct);
            var response = await _repository.CreateSessionTemplateAsync(templateRequest, ct);

            return _mapper.SessionTemplateToCreateSessionTemplateResponse(response, ct);
        }

        public async Task<GetAllSessionTemplatesResponse> GetAllSessionTemplatesAsync(CancellationToken ct)
        {
            var responses = await _repository.GetAllSessionTemplates(ct);

            return _mapper.SessionTemplatesToGetAllSessionTemplatesResponse(responses, ct);
        }

        public async Task<GetSessionTemplateResponse> GetSessionTemplateByTemplateNameAsync(string sessionTemplateName, CancellationToken ct)
        {
            var response = await _repository.GetSessionTemplateByNameAsync(sessionTemplateName, ct);

            return _mapper.SessionTemplateToGetSessionTemplateResponse(response, ct);
        }
    }
}

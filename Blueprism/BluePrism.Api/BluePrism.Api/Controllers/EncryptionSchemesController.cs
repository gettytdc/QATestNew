namespace BluePrism.Api.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Func;
    using Mappers;
    using Models;
    using Services;

    using static Func.ResultHelper;

    [RoutePrefix("encryptionschemes")]
    public class EncryptionSchemesController : ResultControllerBase
    {
        private readonly IEncryptionSchemeService _encryptionSchemeService;

        public EncryptionSchemesController(IEncryptionSchemeService encryptionSchemeService) =>
            _encryptionSchemeService = encryptionSchemeService;

        [HttpGet, Route("")]
        public async Task<Result<IEnumerable<EncryptionSchemeModel>>> GetEncryptionSchemes() =>
            await _encryptionSchemeService.GetEncryptionSchemes()
                .Then(schemes => Succeed(schemes.Select(s => s.ToModelObject())));
    }
}

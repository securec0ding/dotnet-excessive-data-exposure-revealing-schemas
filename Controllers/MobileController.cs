using Backend.Model;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace Backend.Controllers
{
    [ApiController]
    [Route("/api")]
    public class MobileController : ControllerBase
    {
        private IIdentityService _identityService;
        private IMobileService _mobileService;

        public MobileController(IIdentityService identityService, IMobileService mobileService)
        {
            _identityService = identityService;
            _mobileService = mobileService;
        }

        [HttpPost]
        [Route("mobile/unpair")]
        [Authorize]
        public async Task<IActionResult> Unpair([FromBody] UnpairRequest request)
        {
            var user = await GetUserIdentity();
            var result = await _mobileService.Unpair(user.Id, request);

            if (result == null)
                return BadRequest(new ErrorModel { Message = "Could not unpair your phone" });

            return Ok(result);
        }

        private async Task<UserModel> GetUserIdentity()
        {
            var userName = HttpContext.User.Identity.Name;
            var user = await _identityService.GetUserAsync(userName);

            return user;
        }
    }
}
using Backend.Model;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/api")]
    public class AccountController : ControllerBase
    {
        private IIdentityService _identityService;
        private IAccountService _accountService;

        public AccountController(IIdentityService identityService, IAccountService accountService)
        {
            _identityService = identityService;
            _accountService = accountService;
        }

        [HttpGet]
        [Route("account/{accountId}")]
        [Authorize]
        public async Task<IActionResult> GetAccount([FromRoute] string accountId)
        {
            var user = await GetUserIdentity();
            var account = await _accountService.GetAccountById(user.Id, accountId);

            if (account == null)
                return NotFound(new ErrorModel { Message = "Account not found" });

            var response = new BankAccountResponse(account);
            return Ok(response);
        }

        [HttpGet]
        [Route("account-user/{accountId}")]
        [Authorize]
        public async Task<IActionResult> GetUserByAccountId([FromRoute] string accountId)
        {
            var userIdentity = await GetUserIdentity();
            var user = await _accountService.GetUserByAccountId(userIdentity.Id, accountId);

            if (user == null)
                return NotFound(new ErrorModel { Message = "Account not found" });

            var response = new UserResponse(user);
            return Ok(response);
        }


        [HttpPost]
        [Route("account-transfer/")]
        [Authorize]
        public async Task<IActionResult> Transfer([FromBody] Transfer transfer)
        {
            var user = await GetUserIdentity();
            var transferResult = await _accountService.TransferMoney(user.Id, transfer);

            if (transferResult == null)
                return BadRequest(new ErrorModel { Message = "Sorry, you cannot transfer money" });

            //var response = new TransferResponse(transferResult);
            return Ok(transferResult);
        }

        [HttpGet]
        [Route("account-code/{cardId}")]
        [Authorize]
        public async Task<IActionResult> TransactionCode([FromRoute] string cardId)
        {
            var user = await GetUserIdentity();
            var code = await _accountService.GenerateTransactionCode(user.Id, cardId);

            if (code == null)
                return BadRequest(new ErrorModel { Message = "Could not generate transaction code" });

            var response = new TransactionCodeResponse(code);
            return Ok(response);
        }

        private async Task<UserModel> GetUserIdentity()
        {
            var userName = HttpContext.User.Identity.Name;
            var user = await _identityService.GetUserAsync(userName);

            return user;
        }
    }
}
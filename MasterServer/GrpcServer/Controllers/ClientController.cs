using MasterServer.ClientShared;
using MasterServer.ClientShared.Models;
using MasterServer.DarkRift;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SessionKeyManager;
using System;
using System.Threading.Tasks;


namespace MasterServer.Controllers
{
    public class ClientController : ControllerBase
    {
        private readonly DRClientManager clientManager;

        public ClientController(DRClientManager clientManager)
        {
            this.clientManager = clientManager;
        }

        [Authorize(AuthenticationSchemes = JwtAuthenticationHelper.JwtAuthenticationScheme)]
        [Route(MasterServerRoutes.USER_LOGIN_ROUTE)]
        public async Task<IActionResult> Secret()
        {
            try
            {
                var res = await clientManager.UserLoggedIn(HttpContext.GetUserIDFromJWTHeader());
                return Ok(new ClientAuthResponse() { sessionToken = res});
            }
            catch(Exception e) 
            {
                Console.WriteLine(e.ToString());
                return BadRequest();
            }
        }
    }
}

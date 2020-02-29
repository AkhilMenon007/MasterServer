using MasterServer.DarkRift;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SessionKeyManager;
using System;
using System.Threading.Tasks;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        [Route("secret")]
        public async Task<IActionResult> Secret()
        {
            try
            {
                var res = await clientManager.UserLoggedIn(HttpContext.GetUserIDFromJWTHeader());
                return Ok(res);
            }
            catch(Exception e) 
            {
                Console.WriteLine(e.ToString());
                return BadRequest();
            }
        }
    }
}

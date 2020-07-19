using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace WebApi.Controllers
{
    [Authorize]
    [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
    public abstract class ApiBaseController : ApiController
    {
        public string Password
        {
            get
            {
                var identity = (ClaimsIdentity)User.Identity;
                IEnumerable<Claim> claims = identity.Claims;
                string str = claims.Where(o => o.Type == "Password").First().Value;
                return str;
            }
        }

        public string UserName
        {
            get
            {
                var identity = (ClaimsIdentity)User.Identity;
                IEnumerable<Claim> claims = identity.Claims;
                string str = claims.Where(o => o.Type == "UserName").First().Value;
                return str;
            }
        }
    }
}

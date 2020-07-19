
using Microsoft.AspNet.Identity;
using Microsoft.Web.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using WebApi.Controllers;
using WebApi.Models;

namespace WebPOS.WebApi.Controllers
{
    [RoutePrefix("account")]
    public class AccountController : ApiBaseController
    {
        [Route(""), HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public HttpResponseMessage RedirectToSwaggerUi()
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Found);
            httpResponseMessage.Headers.Location = new Uri("/swagger/ui/index", UriKind.Relative);
            return httpResponseMessage;
        }


        [ApiVersionNeutral]
        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<IHttpActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bool AuthResult=true;
	    // Some checs for good login
            if (AuthResult!=false)
            {
                return InternalServerError(new Exception("Username or password is incorrect"));
            }


            string url = Request.RequestUri.AbsoluteUri.Substring(0, Request.RequestUri.AbsoluteUri.IndexOf("account")) + "/" + "token";

            string token = "";
            string tokenType = "";

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string result = wc.UploadString(url,
                    string.Format("grant_type=password&username={0}&password={1}", model.Username, model.Password));
                dynamic stuff = JsonConvert.DeserializeObject(result);
                token = stuff.access_token;
                tokenType = stuff.token_type;
            }

                


            return Json(new
            {
                /*
                username = user.UserName,
                userId = user.Id,
                email = user.Email,
                phone = user.PhoneNumber,
                 * */
                token = token,
                tokenType = tokenType               
            });
        }
    }
}

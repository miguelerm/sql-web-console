using SqlWebConsole.ApiModels;
using SqlWebConsole.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SqlWebConsole.ApiControllers
{
    public class AuthenticationController : ApiController
    {

        public IHttpActionResult Post(Credentials credentials)
        {
            var svc = new AuthenticationService();
            var usr = svc.Login(credentials.Username, credentials.Password);

            if (usr != null)
            {
                var sessionId = Guid.NewGuid().ToString();

                var session = new SessionModel { Id = sessionId, User = usr };

                HttpContext.Current.Application.Add(sessionId, session);

                return Ok(session);
            }

            return BadRequest();
        }

    }
}

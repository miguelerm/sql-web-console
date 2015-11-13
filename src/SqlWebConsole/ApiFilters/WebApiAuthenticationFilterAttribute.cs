using SqlWebConsole.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace SqlWebConsole.ApiFilters
{
    public class WebApiAuthenticationFilterAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {

            var identity = ParseAuthorizationHeader(actionContext);
            if (identity == null)
            {
                HandleUnauthorizedRequest(actionContext);
                return;
            }

           var principal = new GenericPrincipal(identity, null);

           if (HttpContext.Current != null)
                HttpContext.Current.User = principal;

            Thread.CurrentPrincipal = principal;

            base.OnAuthorization(actionContext);
        }

        private IIdentity ParseAuthorizationHeader(HttpActionContext actionContext)
        {
            string authHeader = null;
            var auth = actionContext.Request.Headers.Authorization;
            if (auth != null && auth.Scheme == "Custom")
                authHeader = auth.Parameter;

            Guid sessionId;

            if (string.IsNullOrEmpty(authHeader) || !Guid.TryParse(authHeader, out sessionId))
                return null;

            var sid = sessionId.ToString();

            if (!HttpContext.Current.Application.AllKeys.Contains(sid))
            {
                return null;
            }

            var session = HttpContext.Current.Application[sid] as SessionModel;

            if (session == null)
            {
                return null;
            }

            return new GenericIdentity(session.User.Login);
        }
    }
}
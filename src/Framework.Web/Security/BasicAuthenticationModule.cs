﻿using System;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using log4net;

namespace PDS.Framework.Web.Security
{
    public class BasicAuthenticationModule : IHttpModule
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(BasicAuthenticationModule));

        public void Init(HttpApplication context)
        {
            // Register event handlers
            context.AuthenticateRequest += OnApplicationAuthenticateRequest;
            context.EndRequest += OnApplicationEndRequest;
        }

        private static void SetPrincipal(IPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;

            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }

        private static bool CheckPassword(string username, string password)
        {
            _log.InfoFormat("Authenticating user: {0}", username);
            return Membership.ValidateUser(username, password);
        }

        private static void AuthenticateUser(string credentials)
        {
            try
            {
                var encoding = Encoding.GetEncoding("iso-8859-1");
                credentials = encoding.GetString(Convert.FromBase64String(credentials));

                int separator = credentials.IndexOf(':');
                string name = credentials.Substring(0, separator);
                string password = credentials.Substring(separator + 1);

                if (CheckPassword(name, password))
                {
                    var identity = new GenericIdentity(name);
                    SetPrincipal(new GenericPrincipal(identity, null));
                }
                else
                {
                    // Invalid username or password.
                    DenyAccess();
                }
            }
            catch (FormatException)
            {
                // Credentials were not formatted correctly.
                DenyAccess();
            }
        }

        private static void OnApplicationAuthenticateRequest(object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;
            var authHeader = request.Headers["Authorization"];

            if (authHeader != null)
            {
                var authHeaderVal = AuthenticationHeaderValue.Parse(authHeader);

                // RFC 2617 sec 1.2, "scheme" name is case-insensitive
                if (authHeaderVal.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) && authHeaderVal.Parameter != null)
                {
                    AuthenticateUser(authHeaderVal.Parameter);
                }
            }
            else
            {
                DenyAccess();
            }
        }

        // If the request was unauthorized, add the WWW-Authenticate header to the response.
        private static void OnApplicationEndRequest(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            var response = context.Response;

            if (response.StatusCode == 401)
            {
                var realm = context.Request.Url.Host;
                response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", realm));
            }
        }
        private static void DenyAccess()
        {
            var context = HttpContext.Current;
            context.Response.StatusCode = 401;
            context.Response.End();
        }

        public void Dispose()
        {
        }
    }
}
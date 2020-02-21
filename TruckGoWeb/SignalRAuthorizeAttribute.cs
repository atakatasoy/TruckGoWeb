using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using TruckGoWeb.Helpers;
using TruckGoWeb.Models;

namespace TruckGoWeb
{
    public class SignalRAuthorizeAttribute : AuthorizeAttribute
    {
        //<AccessToken,Username>
        public static Dictionary<string, string> ConfirmedUsers { get; } = new Dictionary<string, string>();

        //<AccessToken,ListOfCookies<CookieName,Cookie>>
        public static Dictionary<string,List<KeyValuePair<string, Cookie>>> UserCookies { get; } = new Dictionary<string, List<KeyValuePair<string, Cookie>>>();

        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            return AlreadyConfirmed(request.Headers["AccessToken"], request.Headers["username"], request) ? true : UserAuthorized(request);
        }

        bool UserAuthorized(IRequest request)
        {
            var accessToken = request.Headers["AccessToken"];
            var userName = request.Headers["username"];

            if (TruckGo.ValidateAccessToken(accessToken, userName) is Users requesterUser)
            {
                var companyIDCookie = new KeyValuePair<string, Cookie>("CompanyID", new Cookie("CompanyID", requesterUser.CompanyID.ToString()));
                var userIDCookie = new KeyValuePair<string, Cookie>("UserID", new Cookie("UserID", requesterUser.UserID.ToString()));

                if (!request.Cookies.ContainsKey("CompanyID"))
                    request.Cookies.Add(companyIDCookie);
                if (!request.Cookies.ContainsKey("UserID"))
                    request.Cookies.Add(userIDCookie);

                if (!UserCookies.ContainsKey(accessToken))
                {
                    UserCookies.Add(accessToken, new List<KeyValuePair<string, Cookie>>());
                    UserCookies[accessToken].Add(companyIDCookie);
                    UserCookies[accessToken].Add(userIDCookie);
                }

                if (!ConfirmedUsers.ContainsKey(accessToken))
                    ConfirmedUsers.Add(accessToken, userName);

                return true;
            }
            return false;
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            var request = hubIncomingInvokerContext.Hub.Context.Request;

            return AlreadyConfirmed(request.Headers["AccessToken"], request.Headers["username"], request) ? true : UserAuthorized(request);
        }

        private bool AlreadyConfirmed(string accessToken,string userName,IRequest request)
        {
            var confirmed = ConfirmedUsers.ContainsKey(accessToken) && ConfirmedUsers[accessToken] == userName;

            if (confirmed && request.Cookies.Count == 0)
                foreach (var cookie in UserCookies[accessToken])
                    request.Cookies.Add(cookie);
            
            return confirmed;
        }
    }
}
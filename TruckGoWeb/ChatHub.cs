using LinqKit;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TruckGoWeb.Helpers;
using TruckGoWeb.Models;

namespace TruckGoWeb
{
    [SignalRAuthorize]
    public class ChatHub : Hub
    {
        static Dictionary<string, string> mCompanyChatResponsibles = null;
        Dictionary<string, string> CompanyChatResponsibles
        {
            get
            {
                if (mCompanyChatResponsibles == null)
                {
                    mCompanyChatResponsibles = new Dictionary<string, string>();
                    using (TruckGoEntities db = new TruckGoEntities())
                    {
                        var responsiblesIds = db.Companies.Select(c => c.ResponsibleUserID).ToList();
                        var predicate = PredicateBuilder.New<Users>();

                        foreach (var item in responsiblesIds)
                        {
                            predicate.Or(p => p.UserID == item);
                        }

                        db.Users.Where(predicate)
                            .Select(user => new { user.AccessToken, user.CompanyID })
                            .ToList()
                            .ForEach(each => mCompanyChatResponsibles.Add(each.CompanyID.ToString(), each.AccessToken));
                    }
                }
                return mCompanyChatResponsibles;
            }
        }

        public void SendMessage(string username, string message,bool isSound)
        {
            var accessToken = Context.Headers["AccessToken"];
            var userName = Context.Headers["username"];

            List<string> receivers = new List<string>()
            {
                accessToken,
                CompanyChatResponsibles[Context.Request.Cookies["CompanyID"].Value]
            };

            bool success = true;

            Task.Run(async () => await TruckGo.CreateMessageAsync(accessToken, Convert.ToInt32(Context.Request.Cookies["UserID"].Value), message, isSound)).ContinueWith(t =>
             {
                 if (t.IsFaulted)
                     success = false;
             });

            Clients.Users(receivers).MessageReceived(username, message, isSound);
        }

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var accessToken = Context.Headers["AccessToken"];

            SignalRAuthorizeAttribute.UserCookies.Remove(accessToken);
            SignalRAuthorizeAttribute.ConfirmedUsers.Remove(accessToken);

            return base.OnDisconnected(stopCalled);
        }
    }
}
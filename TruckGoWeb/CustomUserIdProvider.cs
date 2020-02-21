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
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(IRequest request)
        {
            return request.Headers["AccessToken"];
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using TruckGoWeb.Helpers;
using TruckGoWeb.Models;

namespace TruckGoWeb.Controllers.API
{
    public class AppController : ApiController
    {

        #region LogError

        [HttpPost]
        public async Task<HttpResponseMessage> LogError()
        {
            int responseVal = 0;
            string responseText = "OK";
            bool error = false;

            var rawContent = await Request.Content.ReadAsStringAsync();

            #region Method Specific Variables
            Users requesterUser = default;
            var parameters = new
            {
                AccessToken = default(string),
                responseText = default(string),
                date = default(string),
                parameters = default(string)
            };
            #endregion

            #region Parameter Controls

            try { parameters = JsonConvert.DeserializeAnonymousType(rawContent, parameters); }
            catch
            {
                responseVal = 4;
                responseText = TruckGo.WarningDictionary[4];
                error = true;
            }

            if (!error)
            {
                string lastControl = default;
                try
                {

                    lastControl = nameof(parameters.AccessToken);
                    if (string.IsNullOrWhiteSpace(parameters.AccessToken))
                        throw new Exception();

                    lastControl = nameof(parameters.date);
                    if (string.IsNullOrWhiteSpace(parameters.date))
                        throw new Exception();
                    Convert.ToDateTime(parameters.date);

                    lastControl = nameof(parameters.responseText);
                    if (string.IsNullOrWhiteSpace(parameters.responseText))
                        throw new Exception();

                    lastControl = nameof(parameters.parameters);
                    if (string.IsNullOrWhiteSpace(parameters.parameters))
                        throw new Exception();
                }
                catch
                {
                    responseVal = 8;
                    responseText = TruckGo.WarningDictionary[8].Replace("Parameter", lastControl);
                    error = true;
                }
            }

            #endregion

            #region Main Process
            if (!error)
            {
                using (TruckGoEntities db = new TruckGoEntities())
                {
                    requesterUser = await TruckGo.GetUserByAccessToken(parameters.AccessToken, db);

                    if (requesterUser == null)
                    {
                        responseVal = 2;
                        responseText = TruckGo.WarningDictionary[2];
                        error = true;
                    }
                    else
                    {
                        db.Errors.Add(new Errors
                        {
                            UserID = requesterUser.UserID,
                            Date = Convert.ToDateTime(parameters.date),
                            ResponseText = parameters.responseText,
                            Parameters = parameters.parameters
                        });
                        await db.SaveChangesAsync();
                    }
                }
            }

            #endregion

            string responseString = JsonConvert.SerializeObject(new
            {
                responseVal,
                responseText
            });

            return new HttpResponseMessage
            {
                Content = new StringContent(responseString)
            };
        } 

        #endregion

    }
}

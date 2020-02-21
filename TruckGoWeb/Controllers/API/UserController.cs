using LinqKit;
using Newtonsoft.Json;
using SimpleCrypto;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using TruckGoWeb.Helpers;
using TruckGoWeb.Models;

namespace TruckGoWeb.Controllers.API
{
    public class UserController : ApiController
    {
        #region Login
        [HttpPost]
        public async Task<HttpResponseMessage> Login()
        {
            int responseVal = 0;
            string responseText = "OK";
            bool error = false;
            var rawContent = await Request.Content.ReadAsStringAsync();

            #region Method Specific Variables

            var parameters = new
            {
                Username = default(string),
                Password = default(string)
            };
            Users requestedUser = default;

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
                if (string.IsNullOrWhiteSpace(parameters.Username))
                {
                    responseVal = 1;
                    responseText = TruckGo.WarningDictionary[1];
                    error = true;
                }

                if (!error && string.IsNullOrWhiteSpace(parameters.Password))
                {
                    responseVal = 1;
                    responseText = TruckGo.WarningDictionary[1];
                    error = true;
                } 
            }

            #endregion

            #region Main Process

            if (!error)
            {
                using (TruckGoEntities db = new TruckGoEntities())
                {
                    requestedUser = await TruckGo.GetUserByUsername(parameters.Username, db);

                    //User not found
                    if (requestedUser == null)
                    {
                        responseVal = 2;
                        responseText = TruckGo.WarningDictionary[2];
                        error = true;
                    }
                    else
                    {
                        PBKDF2 hashing = new PBKDF2();

                        var hashedPassword = hashing.Compute(parameters.Password, requestedUser.Salt);

                        //Password doesnt match
                        if (hashedPassword != requestedUser.Userpass)
                        {
                            responseVal = 3;
                            responseText = TruckGo.WarningDictionary[3];
                            error = true;
                        }
                    }
                }
            }

            #endregion

            string responseString = responseVal == 0 ? JsonConvert.SerializeObject(new
            {
                responseVal,
                responseText,
                requestedUser.AccessToken,
                requestedUser.Username,
                requestedUser.NameSurname,
                requestedUser.UserType,
            }) : JsonConvert.SerializeObject(new
            {
                responseVal,
                responseText
            });

            return new HttpResponseMessage()
            {
                Content = new StringContent(responseString)
            };
        }
        #endregion

        #region Register

        [HttpPost]
        public async Task<HttpResponseMessage> Register()
        {
            int responseVal = 0;
            string responseText = "OK";
            bool error = false;
            var rawContent = await Request.Content.ReadAsStringAsync();

            #region Method Specific Variables

            var parameters = new
            {
                Username = default(string),
                Password = default(string),
                NameSurname = default(string),
                ContactInfo = default(string)
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
                if (string.IsNullOrWhiteSpace(parameters.Username))
                {
                    responseVal = 1;
                    responseText = TruckGo.WarningDictionary[1];
                    error = true;
                }
                else if (string.IsNullOrWhiteSpace(parameters.Password))
                {
                    responseVal = 1;
                    responseText = TruckGo.WarningDictionary[1];
                    error = true;
                }
                else if (string.IsNullOrWhiteSpace(parameters.NameSurname))
                {
                    responseVal = 5;
                    responseText = TruckGo.WarningDictionary[5].Replace("#Parameter#", nameof(parameters.NameSurname));
                    error = true;
                }
                else if (string.IsNullOrWhiteSpace(parameters.ContactInfo))
                {
                    responseVal = 5;
                    responseText = TruckGo.WarningDictionary[5].Replace("#Parameter#", nameof(parameters.ContactInfo));
                    error = true;
                }
            }

            #endregion

            #region Main Process

            if (!error)
            {
                using(TruckGoEntities db=new TruckGoEntities())
                {
                    //Kullanıcı adı kontrolü
                    if (!await TruckGo.UsernameTaken(parameters.Username))
                    {
                        responseVal = 6;
                        responseText = TruckGo.WarningDictionary[6];
                        error = true;
                    }
                    else
                    {
                        PBKDF2 hashing = new PBKDF2();
                        var hashedPass = hashing.Compute(parameters.Password);
                        var salt = hashing.Salt;

                        db.Users.Add(new Users
                        {
                            AccessToken = Guid.NewGuid().ToString("N"),
                            NameSurname = parameters.NameSurname,
                            Username = parameters.Username,
                            Userpass = hashedPass,
                            Salt = salt,
                            ContactInfo = parameters.ContactInfo,
                            CreateDate = DateTime.Now,
                            UserType = 1,
                            State = true
                        });

                        await db.SaveChangesAsync();
                    }
                }
            }

            #endregion

            var responseString = JsonConvert.SerializeObject(new
            {
                responseVal,
                responseText
            });

            return new HttpResponseMessage()
            {
                Content = new StringContent(responseString)
            };
        }

        #endregion

        #region SetLocationUpdates

        [HttpPost]
        public async Task<HttpResponseMessage> SetLocationUpdates()
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
                LocationList = new[]
                {
                    new
                    {
                        Latitude = default(string),
                        Longitude = default(string),
                        Altitude = default(string),
                        Speed = default(string),
                        Accuracy = default(string),
                        Date = default(string)
                    }
                }.ToList()
            };
            #endregion

            #region Parameter Controls

            try
            {
                parameters = JsonConvert.DeserializeAnonymousType(rawContent, parameters);
            }
            catch
            {
                responseVal = 4;
                responseText = TruckGo.WarningDictionary[4];
                error = true;
            }

            if (!error)
            {
                string lastControl = default;
                decimal lat, lon, alt, speed;
                try
                {
                    lastControl = nameof(parameters.AccessToken);
                    if (string.IsNullOrWhiteSpace(parameters.AccessToken))
                        throw new Exception();

                    foreach (var eachData in parameters.LocationList)
                    { 
                        lastControl = nameof(eachData.Latitude);
                        lat = Convert.ToDecimal(eachData.Latitude.ToString().Replace(".", ","));

                        lastControl = nameof(eachData.Longitude);
                        lon = Convert.ToDecimal(eachData.Longitude.ToString().Replace(".", ","));

                        lastControl = nameof(eachData.Altitude);
                        alt = Convert.ToDecimal(eachData.Altitude.ToString().Replace(".", ","));

                        lastControl = nameof(eachData.Speed);
                        speed = Convert.ToDecimal(eachData.Speed.ToString().Replace(".", ","));

                        lastControl = nameof(eachData.Accuracy);
                        if (string.IsNullOrEmpty(eachData.Accuracy))
                            throw new Exception();

                        lastControl = nameof(eachData.Date);
                        if (string.IsNullOrWhiteSpace(eachData.Date))
                        {
                            throw new Exception();
                        }
                        var trial = Convert.ToDateTime(eachData.Date);
                    }   
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

                    if (!error)
                    {
                        var activeVehicle = await db.Vehicles.FirstOrDefaultAsync(vehicle => vehicle.UserID == requesterUser.UserID && vehicle.State);

                        if (activeVehicle == null)
                        {
                            responseVal = 9;
                            responseText = TruckGo.WarningDictionary[9];
                            error = true;
                        }
                        else
                        {
                            var lastLocation = await db.Atc.OrderByDescending(atc => atc.Date).FirstOrDefaultAsync(atc => atc.VehicleID == activeVehicle.VehicleID);

                            foreach (var eachData in parameters.LocationList)
                            {
                                bool insert = false;

                                decimal angle = 0;

                                if (lastLocation == null)
                                    insert = true;
                                else
                                {
                                    if (TruckGo.DistanceCalculator(
                                        Convert.ToDouble(eachData.Latitude.Replace(".", ",")),
                                        Convert.ToDouble(eachData.Longitude.Replace(".", ",")),
                                        Convert.ToDouble(lastLocation.Latitude.Replace(".", ",")),
                                        Convert.ToDouble(lastLocation.Longitude.Replace(".", ","))) >= 0.05)
                                    {
                                        insert = true;
                                    }

                                    angle = (int)TruckGo.AngleFromCoordinate(
                                        Convert.ToDouble(lastLocation.Latitude.Replace(".", ",")),
                                        Convert.ToDouble(lastLocation.Longitude.Replace(".", ",")),
                                        Convert.ToDouble(eachData.Latitude.Replace(".", ",")),
                                        Convert.ToDouble(eachData.Longitude.Replace(".", ",")));
                                }

                                if (insert)
                                {
                                    Atc newLocation = new Atc
                                    {
                                        VehicleID = activeVehicle.VehicleID,
                                        Accuracy = eachData.Accuracy,
                                        Altitude = eachData.Altitude,
                                        Angle = angle,
                                        Latitude = eachData.Latitude.Replace(",", "."),
                                        Longitude = eachData.Longitude.Replace(",", "."),
                                        Speed = Convert.ToDecimal(eachData.Speed),
                                        Date = Convert.ToDateTime(eachData.Date),
                                        CreateDate = DateTime.Now
                                    };

                                    db.Atc.Add(newLocation);
                                }
                            }
                            await db.SaveChangesAsync();
                        }
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

        #region EmergencyCall

        [HttpPost]
        public async Task<HttpResponseMessage> EmergencyCall()
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
                Emergency = default(int)
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
                if (string.IsNullOrWhiteSpace(parameters.AccessToken))
                {
                    responseVal = 8;
                    responseText = TruckGo.WarningDictionary[8].Replace("Parameter", nameof(parameters.AccessToken));
                    error = true;
                }
                else if (parameters.Emergency == 0)
                {
                    responseVal = 8;
                    responseText = TruckGo.WarningDictionary[8].Replace("Parameter", nameof(parameters.Emergency));
                    error = true;
                }
            }
            #endregion

            #region Main Process
            if (!error)
            {
                using (TruckGoEntities db = new TruckGoEntities())
                {
                    //SignalR
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

        #region GetRoomMessages

        [HttpPost]
        public async Task<HttpResponseMessage> GetRoomMessages()
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
            };
            var messagesList = new[]
            {
                new
                {
                    MessageContent=default(string),
                    MessageOwner=default(string),
                    IsSound=default(bool)
                }
            }.ToList();
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
                if (string.IsNullOrWhiteSpace(parameters.AccessToken))
                {
                    responseVal = 8;
                    responseText = TruckGo.WarningDictionary[8].Replace("Parameter", nameof(parameters.AccessToken));
                    error = true;
                }
            }

            #endregion

            #region Main Process
            if (!error)
            {
                using(TruckGoEntities db=new TruckGoEntities())
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
                        if (db.MessageRooms.FirstOrDefault(mr => mr.DriverUserID == requesterUser.UserID) is MessageRooms room)
                        {
                            var companyName = (await db.Companies.FirstOrDefaultAsync(c => c.CompanyID == room.CompanyID)).CompanyName;
                            var mainPath = HttpContext.Current.Server.MapPath("~/VoiceRecords");
                            messagesList = await db.Messages.Where(m => m.MessageRoomID == room.MessageRoomID).OrderBy(m => m.CreateDate).Select(m => new
                            {
                                m.MessageContent,
                                MessageOwner = m.UserID == requesterUser.UserID ? requesterUser.NameSurname : companyName,
                                m.IsSound
                            }).ToListAsync();

                            //
                            // Sending the voices directly
                            //
                            //messagesList.ForEach(message =>
                            //{
                            //    if (message.IsSound)
                            //    {
                            //        message = new
                            //        {
                            //            MessageContent = Convert.ToBase64String(File.ReadAllBytes(mainPath + "/" + message.MessageContent + ".wav")),
                            //            message.MessageOwner,
                            //            message.IsSound
                            //        };
                            //    }
                            //});
                        }
                        else
                        {
                            responseVal = 10;
                            responseText = TruckGo.WarningDictionary[10];
                            error = true;
                        }
                    }
                }
            }
            #endregion

            var responseString = responseVal == 0 ? JsonConvert.SerializeObject(new
            {
                responseVal,
                responseText,
                messagesList
            }) : JsonConvert.SerializeObject(new
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

        #region GetProfileInfo

        [HttpPost]
        public async Task<HttpResponseMessage> GetProfileInfo()
        {
            int responseVal = 0;
            string responseText = "OK";
            bool error = false;

            var rawContent = await Request.Content.ReadAsStringAsync();

            #region Method Specific Variables

            Users requesterUser = default;
            var parameters = new
            {
                AccessToken = default(string)
            };
            var userInfo = new
            {
                NameSurname = default(string),
                CompanyName = default(string),
                ContactInfo = default(string),
                CreateDate = default(string),
                UserType = default(string),
                CompanyResponsible = default(string),
                CompanyContactInfo = default(string),
                CompanyAddress = default(string)
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
                if (string.IsNullOrWhiteSpace(parameters.AccessToken))
                {
                    responseVal = 8;
                    responseText = TruckGo.WarningDictionary[8].Replace("Parameter", nameof(parameters.AccessToken));
                    error = true;
                }
            }
            #endregion

            #region Main Process
            if (!error)
            {
                using(TruckGoEntities db=new TruckGoEntities())
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
                        var company = await db.Companies.FirstOrDefaultAsync(c => c.CompanyID == requesterUser.CompanyID);
                        userInfo = new
                        {
                            requesterUser.NameSurname,
                            db.Companies.FirstOrDefault(c => c.CompanyID == requesterUser.CompanyID).CompanyName,
                            requesterUser.ContactInfo,
                            CreateDate = requesterUser.CreateDate.ToString("dd.MM.yyyy"),
                            UserType = (await db.UserTypes.FirstOrDefaultAsync(u => u.UserTypeID == requesterUser.UserType)).UserTypeExplanation,
                            CompanyResponsible = (await db.Users.FirstOrDefaultAsync(u => u.UserID == company.ResponsibleUserID)).NameSurname,
                            CompanyContactInfo = company.ContactInfo,
                            CompanyAddress = company.Address
                        };
                    }
                }
            }
            #endregion

            var XD = string.Empty;


            string responseString = responseVal == 0 ?
                JsonConvert.SerializeObject(new
                {
                    responseVal,
                    responseText,
                    userInfo
                }) :
                JsonConvert.SerializeObject(new
                {
                    responseVal,
                    responseText
                });

            return new HttpResponseMessage
            {
                Content = new StringContent(responseString),
            };
        }

        #endregion

        #region RegisterSound

        [HttpPost]
        public async Task<HttpResponseMessage> RegisterSound()
        {
            int responseVal = 0;
            string responseText = "OK";
            bool error = false;

            var rawContent = await Request.Content.ReadAsStringAsync();

            #region Method Specific Variables
            string fileId = default;
            Users requesterUser = default;
            var parameters = new
            {
                AccessToken = default(string),
                SoundBase64String=default(string)
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
                if (string.IsNullOrWhiteSpace(parameters.AccessToken))
                {
                    responseVal = 8;
                    responseText = TruckGo.WarningDictionary[8].Replace("Parameter", nameof(parameters.AccessToken));
                    error = true;
                }
                if (!error && string.IsNullOrWhiteSpace(parameters.SoundBase64String))
                {
                    responseVal = 8;
                    responseText = TruckGo.WarningDictionary[8].Replace("Parameter", nameof(parameters.SoundBase64String));
                    error = true;
                }
            }

            #endregion

            #region Main Process
            if (!error)
            {
                using(TruckGoEntities db=new TruckGoEntities())
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
                        try
                        {
                            var bytes = Convert.FromBase64String(parameters.SoundBase64String);
                            fileId = Guid.NewGuid().ToString("N");
                            File.WriteAllBytes(HttpContext.Current.Server.MapPath("~/VoiceRecords/" + fileId + TruckGo.SoundMediaType), bytes);
                        }
                        catch
                        {
                            responseVal = 4;
                            responseText = TruckGo.WarningDictionary[4];
                            error = true;
                        }
                    }
                }
            }
            #endregion
            string responseString = responseVal == 0 ? JsonConvert.SerializeObject(new
            {
                responseVal,
                responseText,
                fileId
            }) : JsonConvert.SerializeObject(new
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

        #region GetSounds

        [HttpPost]
        public async Task<HttpResponseMessage> GetSounds()
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
                fileIds = default(List<string>)
            };
            Dictionary<string, string> soundsBase64Dic = new Dictionary<string, string>();
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
                if (string.IsNullOrWhiteSpace(parameters.AccessToken))
                {
                    responseVal = 8;
                    responseText = TruckGo.WarningDictionary[8].Replace("Parameter", nameof(parameters.AccessToken));
                    error = true;
                }
                if (!error && parameters.fileIds.Count == 0)
                {
                    responseVal = 8;
                    responseText = TruckGo.WarningDictionary[8].Replace("Parameter", nameof(parameters.fileIds));
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
                        var predicate = PredicateBuilder.New<Messages>();

                        foreach(var fileId in parameters.fileIds)
                        {
                            predicate.Or(p => p.MessageContent == fileId);
                        }
                        predicate.And(p => p.IsSound);

                        var sounds = await db.Messages.Where(predicate).ToListAsync();

                        var mainPath = HttpContext.Current.Server.MapPath("~/VoiceRecords/");
                        foreach (var sound in sounds)
                        {
                            var bytes = File.ReadAllBytes(mainPath + sound.MessageContent + TruckGo.SoundMediaType);
                            var base64String = Convert.ToBase64String(bytes);
                            soundsBase64Dic.Add(sound.MessageContent, base64String);
                        }
                    }
                }
            }
            #endregion

            string responseString = responseVal == 0 ? JsonConvert.SerializeObject(new
            {
                responseVal,
                responseText,
                soundsBase64Dic
            }) : JsonConvert.SerializeObject(new
            {
                responseVal,
                responseText
            });

            return new HttpResponseMessage()
            {
                Content = new StringContent(responseString)
            };
        }

        #endregion
    }
}

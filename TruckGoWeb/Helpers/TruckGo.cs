using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TruckGoWeb.Models;

namespace TruckGoWeb.Helpers
{
    public static class TruckGo
    {
        public static string SoundMediaType => ".wav";

        public static Dictionary<int, string> WarningDictionary { get; } = new Dictionary<int, string>();

        static TruckGo()
        {
            using (TruckGoEntities db = new TruckGoEntities())
            {
                var all = db.Warnings.ToList();
                foreach(var warning in all)
                {
                    WarningDictionary.Add(warning.WarningID, warning.WarningContent);
                }
            }
        }

        public static async Task CreateMessageAsync(string accessToken,int userId,string message,bool isSound)
        {
            using (TruckGoEntities db = new TruckGoEntities())
            {
                var bufferMessage = new Messages
                {
                    UserID = userId,
                    MessageContent = message,
                    Opened = false,
                    IsSound = isSound,
                    CreateDate = DateTime.Now,
                    State = true
                };
                if (db.MessageRooms.FirstOrDefault(mr => mr.DriverUserID == userId) is MessageRooms room)
                {
                    bufferMessage.MessageRoomID = room.MessageRoomID;
                    db.Messages.Add(bufferMessage);
                }
                else
                {
                    MessageRooms createdRoom = default;
                    createdRoom = await CreateMessageRoom(accessToken);
                    bufferMessage.MessageRoomID = createdRoom.MessageRoomID;
                    db.Messages.Add(bufferMessage);
                }
                await db.SaveChangesAsync();
            }
        }

        public static async Task<MessageRooms> CreateMessageRoom(string accessToken)
        {
            MessageRooms room = default;
            using (TruckGoEntities db = new TruckGoEntities())
            {
                var user = await GetUserByAccessToken(accessToken, db);
                room = new MessageRooms
                {
                    CompanyID = user.CompanyID,
                    DriverUserID = user.UserID,
                    State = true,
                };
                db.MessageRooms.Add(room);
                await db.SaveChangesAsync();
            }
            return room;
        }

        public static Users ValidateAccessToken(string accessToken,string username)
        {
            Users bufferUser = default;

            using(TruckGoEntities db=new TruckGoEntities())
            {
                bufferUser = db.Users.FirstOrDefault(user => user.AccessToken == accessToken && user.Username == username);
            }

            return bufferUser;
        }

        public static async Task<Users> GetUserByAccessToken(string accessToken,TruckGoEntities db)
        {
            return await db.Users.FirstOrDefaultAsync(user => user.AccessToken == accessToken & user.State);
         }

        //public static async Task<string> GetWarningString(int id)
        //{
        //    var warningContent = default(string);

        //    using (TruckGoEntities db = new TruckGoEntities())
        //    {
        //        warningContent = (await db.Warnings.FirstOrDefaultAsync(warning => warning.WarningID == id)).WarningContent;
        //    }

        //    return warningContent;
        //}
        //public static async Task<string> GetWarningString(int id,TruckGoEntities db)
        //{
        //    return (await db.Warnings.FirstOrDefaultAsync(warning => warning.WarningID == id)).WarningContent;
        //}

        public static async Task<Users> GetUserByUsername(string username,TruckGoEntities db)
        {
            return await db.Users.FirstOrDefaultAsync(user => user.Username == username && user.State);
        }

        public static async Task<bool> UsernameTaken(string username)
        {
            bool _isValid = false;
            using (TruckGoEntities db = new TruckGoEntities())
            {
                _isValid = await db.Users.FirstOrDefaultAsync(user => user.Username == username) == null;
            }
            return _isValid;
        }
        public static double DistanceCalculator(double firstLat, double firstLng, double secondLat, double secondLng)
        {
            int Rk = 6373;

            double lat1, lon1, lat2, lon2, dlat, dlon, a, c1, dk, km;

            lat1 = firstLat * Math.PI / 180;
            lon1 = firstLng * Math.PI / 180;
            lat2 = secondLat * Math.PI / 180;
            lon2 = secondLng * Math.PI / 180;

            dlat = lat2 - lat1;
            dlon = lon2 - lon1;

            a = Math.Pow(Math.Sin(dlat / 2), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dlon / 2), 2);
            c1 = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            dk = c1 * Rk; // kilometre cinsinden

            km = Math.Round(dk, 3);

            return km;
        }

        public static double AngleFromCoordinate(double lat1, double long1, double lat2, double long2)
        {
            double dLon = (long2 - long1);

            double y = Math.Sin(dLon) * Math.Cos(lat2);
            double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1)
                    * Math.Cos(lat2) * Math.Cos(dLon);

            double brng = Math.Atan2(y, x);

            brng = brng * 180 / Math.PI;
            brng = (brng + 360) % 360;
            //brng = 360 - brng; // count degrees counter-clockwise - remove to make clockwise

            return brng;
        }
    }
}
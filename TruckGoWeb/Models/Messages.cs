//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TruckGoWeb.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Messages
    {
        public int MessageID { get; set; }
        public int MessageRoomID { get; set; }
        public int UserID { get; set; }
        public string MessageContent { get; set; }
        public bool Opened { get; set; }
        public System.DateTime CreateDate { get; set; }
        public bool State { get; set; }
        public bool IsSound { get; set; }
    
        public virtual MessageRooms MessageRooms { get; set; }
        public virtual Users Users { get; set; }
    }
}

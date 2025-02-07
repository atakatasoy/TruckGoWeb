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
    
    public partial class Users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Users()
        {
            this.Companies = new HashSet<Companies>();
            this.MessageRooms = new HashSet<MessageRooms>();
            this.Messages = new HashSet<Messages>();
            this.Vehicles = new HashSet<Vehicles>();
            this.Errors = new HashSet<Errors>();
        }
    
        public int UserID { get; set; }
        public int UserType { get; set; }
        public int CompanyID { get; set; }
        public string NameSurname { get; set; }
        public string AccessToken { get; set; }
        public string Username { get; set; }
        public string Userpass { get; set; }
        public string Salt { get; set; }
        public string ContactInfo { get; set; }
        public System.DateTime CreateDate { get; set; }
        public bool State { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Companies> Companies { get; set; }
        public virtual Companies Companies1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MessageRooms> MessageRooms { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Messages> Messages { get; set; }
        public virtual UserTypes UserTypes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vehicles> Vehicles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Errors> Errors { get; set; }
    }
}

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
    
    public partial class Vehicles
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Vehicles()
        {
            this.Atc = new HashSet<Atc>();
        }
    
        public int VehicleID { get; set; }
        public int CompanyID { get; set; }
        public int UserID { get; set; }
        public string LicensePlate { get; set; }
        public System.DateTime CreateDate { get; set; }
        public bool State { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Atc> Atc { get; set; }
        public virtual Companies Companies { get; set; }
        public virtual Users Users { get; set; }
    }
}

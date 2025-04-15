using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDynamics.Shared.Modals
{
    public class Supplier
    {
        public string SupplierID { get; set; } = "";
        public string Contact { get; set; } = "";
        public string Name { get; set; } = "";
        public string GSTIN { get; set; } = "";
        public string Address { get; set; } = "";
        public string Area { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string State { get; set; } = "";
        public string District { get; set; } = "";
        public string PinCode { get; set; } = "";
        public string Nationality { get; set; } = "";
        public string Email { get; set; } = "";
        public string CreatedBy { get; set; } = "";
        public string LastUpdatedBy { get; set; } = "";
        public string CreateAt { get; set; } = "";
        public string LastUpdatedAt { get; set; } = "";

        // Constructor with Optional Update (React-like Props Handling)
        public Supplier(Supplier? existingData = null)
        {
            if (existingData != null)
            {
                Contact = existingData.Contact;
                Name = existingData.Name;
                Address = existingData.Address;
                Area = existingData.Area;
                City = existingData.City;
                Country = existingData.Country;
                State = existingData.State;
                District = existingData.District;
                PinCode = existingData.PinCode;
                Nationality = existingData.Nationality;
                Email = existingData.Email;
                CreatedBy = existingData.CreatedBy;
            }
        }

        public void Reset()
        {
            SupplierID = "";
            Contact = "";
            Name = "";
            Address = "";
            Area = "";
            City = "";
            Country = "";
            State = "";
            District = "";
            PinCode = "";
            Nationality = "";
            Email = "";
            CreatedBy = "";
            LastUpdatedBy = "";
            CreateAt = "";
            LastUpdatedAt = "";
        }
    }
}


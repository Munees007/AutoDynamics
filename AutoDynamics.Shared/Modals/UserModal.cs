namespace AutoDynamics.Shared.Modals
{
    public class UserModal
    {
        public string CustomerId { get; set; } = "";
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
        public string Website { get; set; } = "";
        public bool IsSMSAllowed { get; set; } = false;
        public bool IsEmailAllowed { get; set; } = false;
        public bool IsWhatsAppAllowed { get; set; } = false;
        public string CreatedBy { get; set; } = "";
        public string LastUpdatedBy { get; set; } = "";
        public string CreateAt { get; set; } = "";
        public string LastUpdatedAt { get; set; } = "";

        // Constructor with Optional Update (React-like Props Handling)
        public UserModal(UserModal? existingData = null)
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
                Website = existingData.Website;
                IsSMSAllowed = existingData.IsSMSAllowed;
                IsEmailAllowed = existingData.IsEmailAllowed;
                IsWhatsAppAllowed = existingData.IsWhatsAppAllowed;
                CreatedBy = existingData.CreatedBy;
            }
        }

        public void Reset()
        {
            CustomerId = "";
            Contact = "";
            Name = "";
            Address = "";
            Area = "";
            GSTIN = "";
            City = "";
            Country = "";
            State = "";
            District = "";
            PinCode = "";
            Nationality = "";
            Email = "";
            Website = "";
            IsSMSAllowed = false;
            IsEmailAllowed = false;
            IsWhatsAppAllowed = false;
            CreatedBy = "";
            LastUpdatedBy = "";
            CreateAt = "";
            LastUpdatedAt = "";
        }
    }
}

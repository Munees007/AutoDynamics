namespace AutoDynamics.Shared.Modals.Vehicle
{
    public class VehicleType
    {
        public string CustomerID { get; set; } = "";
        public string VehicleNo { set; get; } = ""; 
        public string VehicleMake {set;get;} = "";
        public string ModelName {set;get;} = "";
        public int MfgYear {set;get;} = 0;
        public string Description {set;get;} = "";
        public string CreateAt {set;get;} = "";
        public string CreatedBy {set;get;} = "";
        public string LastUpdatedBy {set;get;} = "";
        public string LastUpdatedAt {set;get;} = "";
    }
}

namespace AutoDynamics.Shared.Modals
{
    public class ProductType
    {
        public string ProductID { set;get; } = "";
        public string HSNCode { get; set; } = "";
        public string  Brand {set;get;} = "";
        public string BrandID { set; get; } = "";
        public string  Size {set;get;} = "";
        public string Pattern { set; get; } = "";
        public string  TubeOrTubeless {set;get;} = "";
        public double  Price {set;get;} = 0f;
        public string  CreateAt {set;get;} = "";
        public string  CreatedBy {set;get;} = "";
        public string  LastUpdatedBy {set;get;} = "";
        public string  LastUpdatedAt {set;get;} = "";
    }
}

namespace Domain.Entities
{
    public class PhoneticInfo
    {
        public string Text { get; set; } 
        public string AudioUrl { get; set; } 
        public string SourceUrl { get; set; }
        public LicenseInfo License { get; set; }
    }
}

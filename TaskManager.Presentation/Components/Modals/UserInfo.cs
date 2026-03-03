namespace TaskManager.Presentation.Models
{
    public class UserInfo
    {
        public bool IsAuthenticated { get; set; }
        public List<ClaimValue> Claims { get; set; } = new();
    }

    public class ClaimValue
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}

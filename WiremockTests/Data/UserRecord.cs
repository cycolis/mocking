namespace MockOktaApi.Data
{
    public class UserRecord
    {
        public int Id { get; set; } // Primary key
        public string Username { get; set; }
        public string OobCode { get; set; }
        public bool Validated { get; set; }
    }
}

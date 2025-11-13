namespace appApi.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Hobby { get; set; }
        public int Age { get; set; }
        public int Balance { get; set; } = 0;
    }
}

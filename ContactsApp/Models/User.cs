namespace ContactsApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public bool isMinor { get; set; } = false;  

        public ICollection<Contact> Contacts { get; } = new List<Contact>();
    }
}

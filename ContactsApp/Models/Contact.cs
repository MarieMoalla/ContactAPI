﻿using System.ComponentModel.DataAnnotations.Schema;

namespace ContactsApp.Models
{
    public class Contact
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string  MobileNumber { get; set; }
        public string Address { get; set; } 

        [ForeignKey("UserId")]
        public int userId { get; set; }
    }
}

﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VAIISemestralkaASPNET.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string VIN { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        public Car()
        {
            Name = string.Empty;
            VIN = string.Empty;
            UserId = string.Empty;
        }
    }
}

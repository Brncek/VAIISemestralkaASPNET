using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VAIISemestralkaASPNET.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int? CarID { get; set; }

        [ForeignKey("CarID")]
        public Car Car { get; set; }

        public string VIN { get; set; }
        public string ServiseInfo { get; set; }

        public string State { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        public Order()
        {
            VIN = string.Empty;
            ServiseInfo = string.Empty; 
            State = string.Empty;
            UserId = string.Empty;
        }
    }
}

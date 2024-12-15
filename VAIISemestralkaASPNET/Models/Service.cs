using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VAIISemestralkaASPNET.Models
{
    public class Service
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndtDate { get; set; }

        public int? CarID { get; set; }

        [ForeignKey("CarID")]
        public Car Car { get; set; }

        public string VIN { get; set; }

        public int WorkTime { get; set; }
        public string ServiceImagesLocation { get; set; }
        public string ServisesDone { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public Service()
        {
            VIN = string.Empty;
            ServiceImagesLocation = string.Empty;
            ServisesDone = string.Empty;
        }
    }
}


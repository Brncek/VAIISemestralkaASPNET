using System.ComponentModel.DataAnnotations.Schema;

namespace VAIISemestralkaASPNET.Models
{
    public class Service
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndtDate { get; set; }
        public string VIN { get; set; }

        public int WorkTime { get; set; }
        public string ServiceImages { get; set; }
        public string ServisesDone { get; set; }

        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public Service()
        {
            VIN = string.Empty;
            ServiceImages = string.Empty;
            ServisesDone = string.Empty;
        }
    }
}


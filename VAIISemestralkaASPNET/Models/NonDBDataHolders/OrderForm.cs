namespace VAIISemestralkaASPNET.Models.NonDBDataHolders
{
        public class OrderForm
        {
            public DateTime Date { get; set; }

            public int CarID { get; set; }

            public string ServiseInfo { get; set; }


            public OrderForm()
            {
                ServiseInfo = string.Empty;
            }
        }

}

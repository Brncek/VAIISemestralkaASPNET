namespace VAIISemestralkaASPNET.App
{
    public class CONSTANTS
    {
        public static string CALENDAR_FULL = "FULL";
        public static string CALENDAR_CLOSED = "CLOSED";
        public static string CALENDAR_OPEN = "OPEN";
        public static int[] CALENDAR_START_HOURS = { 8, 10, 12, 14, 16 }; //HAS TO HAVE EAVEN SPACEING

        public static string ORDER_STATE_RECEAVED = "RECEAVED";
        public static string ORDER_STATE_CONFIRMED = "CONFIRMED";
        public static string ORDER_STATE_IN_PROCES = "INPROCES";
        public static string ORDER_STATE_DONE = "DONE";

        public static List<string> ORDER_STATES()
        {
            List<string> list = new List<string>();

            list.Add(ORDER_STATE_RECEAVED);
            list.Add(ORDER_STATE_CONFIRMED);
            list.Add(ORDER_STATE_IN_PROCES);
            list.Add(ORDER_STATE_DONE);

            return list;
        }
    }
}

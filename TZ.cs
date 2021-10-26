namespace iana_win
{
    public class TimeZoneData
    {
        public TimeZoneData() {
            iana = null;
            win = null;
            description = null;
        }

        public string iana { get; set; }
        public string win { get; set; }
        public string description { get; set; }
    }
}

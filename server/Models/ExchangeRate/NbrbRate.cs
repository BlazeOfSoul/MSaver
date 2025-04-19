namespace server.Models.ExchangeRate
{
    public class NbrbRate
    {
        public int Cur_ID { get; set; }
        public string Cur_Abbreviation { get; set; }
        public int Cur_Scale { get; set; }
        public string Cur_Name { get; set; }
        public decimal Cur_OfficialRate { get; set; }
        public DateTime Date { get; set; }
    }
}

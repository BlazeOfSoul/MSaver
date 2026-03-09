namespace server.Infrastructure.ExchangeRate.Models;

public sealed class NbrbRate
{
    public int Cur_ID { get; set; }
    public string Cur_Abbreviation { get; set; } = string.Empty;
    public int Cur_Scale { get; set; }
    public string Cur_Name { get; set; } = string.Empty;
    public decimal Cur_OfficialRate { get; set; }
    public DateTime Date { get; set; }
}

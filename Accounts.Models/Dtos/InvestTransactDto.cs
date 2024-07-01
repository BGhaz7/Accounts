namespace Investment.Models.Dtos
{
    public class InvestTransactDto
    {
        public Guid ProjectId { get; set; }
        public decimal Amount { get; set; }
    }
}
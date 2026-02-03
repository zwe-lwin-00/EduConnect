namespace EduConnect.Domain.Entities;

public class StudentWallet
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public decimal Balance { get; set; } // Hours balance
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Student Student { get; set; } = null!;
    public ICollection<TransactionHistory> Transactions { get; set; } = new List<TransactionHistory>();
}

using EduConnect.Shared.Enums;

namespace EduConnect.Domain.Entities;

public class TransactionHistory
{
    public int Id { get; set; }
    public int WalletId { get; set; }
    public int? ContractId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; } // Hours
    public decimal BalanceAfter { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public StudentWallet Wallet { get; set; } = null!;
    public ContractSession? ContractSession { get; set; }
}

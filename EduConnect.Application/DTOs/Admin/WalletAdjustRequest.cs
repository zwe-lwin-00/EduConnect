namespace EduConnect.Application.DTOs.Admin;

/// <summary>
/// Legacy DTO (hour-based wallet). Billing is monthly subscription only; credit/deduct endpoints removed.
/// </summary>
public class WalletAdjustRequest
{
    public decimal Hours { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class WalletCreditRequest
{
    public int StudentId { get; set; }
    public int ContractId { get; set; }
    public decimal Hours { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class WalletDeductRequest
{
    public int StudentId { get; set; }
    public int ContractId { get; set; }
    public decimal Hours { get; set; }
    public string Reason { get; set; } = string.Empty;
}

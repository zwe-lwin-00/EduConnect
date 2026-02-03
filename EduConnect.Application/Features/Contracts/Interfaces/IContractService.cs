using EduConnect.Application.Common.Interfaces;
using EduConnect.Application.Common.Models;

namespace EduConnect.Application.Features.Contracts.Interfaces;

public interface IContractService : IService
{
    Task<string> CreateContractAsync(CreateContractDto dto);
    Task<bool> ApproveContractAsync(string contractId);
    Task<bool> CancelContractAsync(string contractId);
    Task<ContractDto> GetContractByIdAsync(string contractId);
    Task<PagedResult<ContractDto>> GetContractsAsync(PagedRequest request);
}

public class CreateContractDto
{
    public int TeacherId { get; set; }
    public int StudentId { get; set; }
    public int PackageHours { get; set; }
    public DateTime StartDate { get; set; }
}

public class ContractDto
{
    public string ContractId { get; set; } = string.Empty;
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int PackageHours { get; set; }
    public int RemainingHours { get; set; }
    public int Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

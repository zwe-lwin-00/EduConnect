using EduConnect.Application.DTOs.Admin;

namespace EduConnect.Application.Features.Admin.Interfaces;

public interface ISettingsService
{
    Task<List<HolidayDto>> GetHolidaysAsync(int? year = null);
    Task<HolidayDto?> GetHolidayByIdAsync(int id);
    Task<HolidayDto> CreateHolidayAsync(CreateHolidayRequest request);
    Task<bool> UpdateHolidayAsync(int id, UpdateHolidayRequest request);
    Task<bool> DeleteHolidayAsync(int id);

    Task<List<SystemSettingDto>> GetSystemSettingsAsync();
    Task<SystemSettingDto?> GetSystemSettingByKeyAsync(string key);
    Task<SystemSettingDto> UpsertSystemSettingAsync(UpsertSystemSettingRequest request);

    Task<List<ClassPriceDto>> GetClassPricesAsync();
    Task<ClassPriceDto> UpsertClassPriceAsync(UpsertClassPriceRequest request);
    Task<bool> DeleteClassPriceAsync(int id);
}

using EduConnect.Application.Common.Exceptions;
using EduConnect.Application.DTOs.Admin;
using EduConnect.Application.Features.Admin.Interfaces;
using EduConnect.Domain.Entities;
using EduConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EduConnect.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly ApplicationDbContext _context;

    public SettingsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HolidayDto>> GetHolidaysAsync(int? year = null)
    {
        IQueryable<Holiday> query = _context.Holidays.AsNoTracking();
        if (year.HasValue)
            query = query.Where(h => h.Date.Year == year.Value);
        var list = await query.OrderBy(h => h.Date).ToListAsync();
        return list.Select(MapHolidayToDto).ToList();
    }

    public async Task<HolidayDto?> GetHolidayByIdAsync(int id)
    {
        var h = await _context.Holidays.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return h == null ? null : MapHolidayToDto(h);
    }

    public async Task<HolidayDto> CreateHolidayAsync(CreateHolidayRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BusinessException("Holiday name is required.", "NAME_REQUIRED");
        var date = request.Date.Date;
        var holiday = new Holiday
        {
            Date = date,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _context.Holidays.Add(holiday);
        await _context.SaveChangesAsync();
        return MapHolidayToDto(holiday);
    }

    public async Task<bool> UpdateHolidayAsync(int id, UpdateHolidayRequest request)
    {
        var h = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == id);
        if (h == null) return false;
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BusinessException("Holiday name is required.", "NAME_REQUIRED");
        h.Date = request.Date.Date;
        h.Name = request.Name.Trim();
        h.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteHolidayAsync(int id)
    {
        var h = await _context.Holidays.FirstOrDefaultAsync(x => x.Id == id);
        if (h == null) return false;
        _context.Holidays.Remove(h);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SystemSettingDto>> GetSystemSettingsAsync()
    {
        var list = await _context.SystemSettings.AsNoTracking().OrderBy(s => s.Key).ToListAsync();
        return list.Select(MapSettingToDto).ToList();
    }

    public async Task<SystemSettingDto?> GetSystemSettingByKeyAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        var s = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key.Trim());
        return s == null ? null : MapSettingToDto(s);
    }

    public async Task<SystemSettingDto> UpsertSystemSettingAsync(UpsertSystemSettingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Key))
            throw new BusinessException("Setting key is required.", "KEY_REQUIRED");
        var key = request.Key.Trim();
        var value = request.Value ?? string.Empty;
        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        var existing = await _context.SystemSettings.FirstOrDefaultAsync(x => x.Key == key);
        var now = DateTime.UtcNow;
        if (existing != null)
        {
            existing.Value = value;
            existing.Description = description;
            existing.UpdatedAt = now;
            await _context.SaveChangesAsync();
            return MapSettingToDto(existing);
        }
        var setting = new SystemSetting
        {
            Key = key,
            Value = value,
            Description = description,
            UpdatedAt = now
        };
        _context.SystemSettings.Add(setting);
        await _context.SaveChangesAsync();
        return MapSettingToDto(setting);
    }

    private static HolidayDto MapHolidayToDto(Holiday h)
    {
        return new HolidayDto
        {
            Id = h.Id,
            Date = h.Date,
            Name = h.Name,
            Description = h.Description,
            CreatedAt = h.CreatedAt
        };
    }

    private static SystemSettingDto MapSettingToDto(SystemSetting s)
    {
        return new SystemSettingDto
        {
            Id = s.Id,
            Key = s.Key,
            Value = s.Value,
            Description = s.Description,
            UpdatedAt = s.UpdatedAt
        };
    }
}

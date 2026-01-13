using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Attributes;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Models.DTOs;

namespace Medical.API.Controllers;

/// <summary>
/// 三甲医院控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class TertiaryHospitalsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<TertiaryHospitalsController> _logger;

    public TertiaryHospitalsController(MedicalDbContext context, ILogger<TertiaryHospitalsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取三甲医院列表（支持分页和搜索）
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // 允许匿名访问，患者端需要显示三甲医院数量
    [RequirePermission("tertiary-hospitals.view")]
    public async Task<ActionResult> GetList(
        [FromQuery] Guid? provinceId = null,
        [FromQuery] Guid? cityId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = _context.TertiaryHospitals
                .Include(h => h.Province)
                .Include(h => h.City)
                .AsQueryable();

            // 省份筛选
            if (provinceId.HasValue)
            {
                query = query.Where(h => h.ProvinceId == provinceId.Value);
            }

            // 城市筛选
            if (cityId.HasValue)
            {
                query = query.Where(h => h.CityId == cityId.Value);
            }

            // 关键词搜索
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(h =>
                    h.Name.Contains(keyword) ||
                    (h.Address != null && h.Address.Contains(keyword)) ||
                    (h.Type != null && h.Type.Contains(keyword)));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(h => h.SortOrder)
                .ThenBy(h => h.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new
                {
                    id = h.Id,
                    name = h.Name,
                    provinceId = h.ProvinceId,
                    provinceName = h.Province.Name,
                    cityId = h.CityId,
                    cityName = h.City.Name,
                    address = h.Address,
                    level = h.Level,
                    type = h.Type,
                    phone = h.Phone,
                    website = h.Website,
                    latitude = h.Latitude,
                    longitude = h.Longitude,
                    sortOrder = h.SortOrder,
                    isEnabled = h.IsEnabled,
                    createdAt = h.CreatedAt,
                    updatedAt = h.UpdatedAt
                })
                .ToListAsync();

            return Ok(new { items, total, page, pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取三甲医院列表失败");
            return StatusCode(500, new { message = "获取数据失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取三甲医院详情
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("tertiary-hospitals.view")]
    public async Task<ActionResult> GetById(Guid id)
    {
        try
        {
            var hospital = await _context.TertiaryHospitals
                .Include(h => h.Province)
                .Include(h => h.City)
                .Where(h => h.Id == id)
                .Select(h => new
                {
                    id = h.Id,
                    name = h.Name,
                    provinceId = h.ProvinceId,
                    provinceName = h.Province.Name,
                    cityId = h.CityId,
                    cityName = h.City.Name,
                    address = h.Address,
                    level = h.Level,
                    type = h.Type,
                    phone = h.Phone,
                    website = h.Website,
                    sortOrder = h.SortOrder,
                    isEnabled = h.IsEnabled,
                    createdAt = h.CreatedAt,
                    updatedAt = h.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (hospital == null)
            {
                return NotFound(new { message = "三甲医院不存在" });
            }

            return Ok(hospital);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取三甲医院详情失败，ID: {Id}", id);
            return StatusCode(500, new { message = "获取数据失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 创建三甲医院
    /// </summary>
    [HttpPost]
    [RequirePermission("tertiary-hospitals.create")]
    public async Task<ActionResult> Create([FromBody] CreateTertiaryHospitalDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 验证省份和城市是否存在
            var province = await _context.Provinces.FindAsync(dto.ProvinceId);
            if (province == null)
            {
                return BadRequest(new { message = "省份不存在" });
            }

            var city = await _context.Cities
                .Include(c => c.Province)
                .FirstOrDefaultAsync(c => c.Id == dto.CityId && c.ProvinceId == dto.ProvinceId);
            if (city == null)
            {
                return BadRequest(new { message = "城市不存在或不属于该省份" });
            }

            var hospital = new TertiaryHospital
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                ProvinceId = dto.ProvinceId,
                CityId = dto.CityId,
                Address = dto.Address,
                Level = dto.Level,
                Type = dto.Type,
                Phone = dto.Phone,
                Website = dto.Website,
                SortOrder = dto.SortOrder,
                IsEnabled = dto.IsEnabled,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TertiaryHospitals.Add(hospital);
            await _context.SaveChangesAsync();

            // 返回创建的结果
            var result = await _context.TertiaryHospitals
                .Include(h => h.Province)
                .Include(h => h.City)
                .Where(h => h.Id == hospital.Id)
                .Select(h => new
                {
                    id = h.Id,
                    name = h.Name,
                    provinceId = h.ProvinceId,
                    provinceName = h.Province.Name,
                    cityId = h.CityId,
                    cityName = h.City.Name,
                    address = h.Address,
                    level = h.Level,
                    type = h.Type,
                    phone = h.Phone,
                    website = h.Website,
                    sortOrder = h.SortOrder,
                    isEnabled = h.IsEnabled,
                    createdAt = h.CreatedAt,
                    updatedAt = h.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建三甲医院失败");
            return StatusCode(500, new { message = "创建失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新三甲医院
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("tertiary-hospitals.update")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateTertiaryHospitalDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var hospital = await _context.TertiaryHospitals.FindAsync(id);
            if (hospital == null)
            {
                return NotFound(new { message = "三甲医院不存在" });
            }

            // 验证省份和城市是否存在
            var province = await _context.Provinces.FindAsync(dto.ProvinceId);
            if (province == null)
            {
                return BadRequest(new { message = "省份不存在" });
            }

            var city = await _context.Cities
                .Include(c => c.Province)
                .FirstOrDefaultAsync(c => c.Id == dto.CityId && c.ProvinceId == dto.ProvinceId);
            if (city == null)
            {
                return BadRequest(new { message = "城市不存在或不属于该省份" });
            }

            hospital.Name = dto.Name;
            hospital.ProvinceId = dto.ProvinceId;
            hospital.CityId = dto.CityId;
            hospital.Address = dto.Address;
            hospital.Level = dto.Level;
            hospital.Type = dto.Type;
            hospital.Phone = dto.Phone;
            hospital.Website = dto.Website;
            hospital.SortOrder = dto.SortOrder;
            hospital.IsEnabled = dto.IsEnabled;
            hospital.UpdatedAt = DateTime.UtcNow;

            _context.TertiaryHospitals.Update(hospital);
            await _context.SaveChangesAsync();

            // 返回更新的结果
            var result = await _context.TertiaryHospitals
                .Include(h => h.Province)
                .Include(h => h.City)
                .Where(h => h.Id == hospital.Id)
                .Select(h => new
                {
                    id = h.Id,
                    name = h.Name,
                    provinceId = h.ProvinceId,
                    provinceName = h.Province.Name,
                    cityId = h.CityId,
                    cityName = h.City.Name,
                    address = h.Address,
                    level = h.Level,
                    type = h.Type,
                    phone = h.Phone,
                    website = h.Website,
                    sortOrder = h.SortOrder,
                    isEnabled = h.IsEnabled,
                    createdAt = h.CreatedAt,
                    updatedAt = h.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新三甲医院失败，ID: {Id}", id);
            return StatusCode(500, new { message = "更新失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除三甲医院
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("tertiary-hospitals.delete")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var hospital = await _context.TertiaryHospitals.FindAsync(id);
            if (hospital == null)
            {
                return NotFound(new { message = "三甲医院不存在" });
            }

            _context.TertiaryHospitals.Remove(hospital);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除三甲医院失败，ID: {Id}", id);
            return StatusCode(500, new { message = "删除失败", error = ex.Message });
        }
    }
}


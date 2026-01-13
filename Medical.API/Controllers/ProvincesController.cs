using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Medical.API.Attributes;
using System.Net.Http;

namespace Medical.API.Controllers;

/// <summary>
/// 省份和城市控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ProvincesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<ProvincesController> _logger;

    public ProvincesController(MedicalDbContext context, ILogger<ProvincesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有省份及其城市（树形结构）
    /// </summary>
    /// <returns>省份和城市列表（树形结构）</returns>
    [HttpGet("tree")]
    [RequirePermission("province-city.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetProvinceCityTree()
    {
        try
        {
            var provinces = await _context.Provinces
                .Where(p => p.IsEnabled)
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Name)
                .Include(p => p.Cities.Where(c => c.IsEnabled).OrderBy(c => c.SortOrder).ThenBy(c => c.Name))
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    code = p.Code,
                    shortName = p.ShortName,
                    sortOrder = p.SortOrder,
                    children = p.Cities.Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        code = c.Code,
                        shortName = c.ShortName,
                        sortOrder = c.SortOrder,
                        provinceId = c.ProvinceId
                    }).ToList()
                })
                .ToListAsync();

            return Ok(provinces);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取省份城市树形数据失败");
            return StatusCode(500, new { message = "获取数据失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取所有省份列表
    /// </summary>
    /// <returns>省份列表</returns>
    [HttpGet]
    [AllowAnonymous] // 允许匿名访问，患者端需要选择省份
    [RequirePermission("province-city.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetProvinces()
    {
        try
        {
            var provinces = await _context.Provinces
                .Where(p => p.IsEnabled)
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Name)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    code = p.Code,
                    shortName = p.ShortName,
                    sortOrder = p.SortOrder
                })
                .ToListAsync();

            return Ok(provinces);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取省份列表失败");
            return StatusCode(500, new { message = "获取数据失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 根据省份ID获取城市列表
    /// </summary>
    /// <param name="provinceId">省份ID</param>
    /// <returns>城市列表</returns>
    [HttpGet("{provinceId}/cities")]
    [AllowAnonymous] // 允许匿名访问，患者端需要选择城市
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetCitiesByProvince(Guid provinceId)
    {
        try
        {
            var cities = await _context.Cities
                .Where(c => c.ProvinceId == provinceId && c.IsEnabled)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    code = c.Code,
                    shortName = c.ShortName,
                    sortOrder = c.SortOrder,
                    provinceId = c.ProvinceId
                })
                .ToListAsync();

            return Ok(cities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取城市列表失败");
            return StatusCode(500, new { message = "获取数据失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 根据用户IP获取对应的省市信息
    /// </summary>
    /// <returns>省市信息</returns>
    [HttpGet("location-by-ip")]
    [AllowAnonymous] // 允许匿名访问，患者端需要显示位置信息
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetLocationByIp()
    {
        try
        {
            // 获取客户端IP地址
            var clientIp = GetClientIpAddress();
            _logger.LogInformation("获取用户IP地址: {ClientIp}", clientIp);

            // 使用免费的IP定位服务（ip-api.com）
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            
            try
            {
                // 使用 ip-api.com 免费服务（限制：每分钟45次请求）
                var response = await httpClient.GetStringAsync($"http://ip-api.com/json/{clientIp}?lang=zh-CN&fields=status,message,country,regionName,city,query");
                var ipInfo = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(response);
                
                if (ipInfo.TryGetProperty("status", out var status) && status.GetString() == "success")
                {
                    var regionName = ipInfo.TryGetProperty("regionName", out var rn) ? rn.GetString() : null;
                    var cityName = ipInfo.TryGetProperty("city", out var cn) ? cn.GetString() : null;
                    var country = ipInfo.TryGetProperty("country", out var c) ? c.GetString() : null;
                    
                    // 只处理中国
                    if (country == "中国" || country == "China")
                    {
                        // 尝试匹配省份（去掉"省"、"市"、"自治区"等后缀）
                        var provinceName = regionName;
                        if (!string.IsNullOrEmpty(provinceName))
                        {
                            // 标准化省份名称（例如："广东" -> "广东省"）
                            var normalizedProvinceName = provinceName;
                            if (!normalizedProvinceName.EndsWith("省") && 
                                !normalizedProvinceName.EndsWith("市") && 
                                !normalizedProvinceName.EndsWith("自治区") &&
                                !normalizedProvinceName.EndsWith("特别行政区"))
                            {
                                // 特殊处理直辖市和自治区
                                if (normalizedProvinceName == "北京") normalizedProvinceName = "北京市";
                                else if (normalizedProvinceName == "上海") normalizedProvinceName = "上海市";
                                else if (normalizedProvinceName == "天津") normalizedProvinceName = "天津市";
                                else if (normalizedProvinceName == "重庆") normalizedProvinceName = "重庆市";
                                else if (normalizedProvinceName == "广西") normalizedProvinceName = "广西壮族自治区";
                                else if (normalizedProvinceName == "新疆") normalizedProvinceName = "新疆维吾尔自治区";
                                else if (normalizedProvinceName == "内蒙古") normalizedProvinceName = "内蒙古自治区";
                                else if (normalizedProvinceName == "西藏") normalizedProvinceName = "西藏自治区";
                                else if (normalizedProvinceName == "宁夏") normalizedProvinceName = "宁夏回族自治区";
                                else normalizedProvinceName = normalizedProvinceName + "省";
                            }
                            
                            // 从数据库查找匹配的省份
                            var province = await _context.Provinces
                                .Where(p => p.Name == normalizedProvinceName || p.Name == provinceName || p.ShortName == provinceName)
                                .FirstOrDefaultAsync();
                            
                            if (province != null)
                            {
                                // 如果提供了城市名称，尝试查找城市
                                City? city = null;
                                if (!string.IsNullOrEmpty(cityName))
                                {
                                    var normalizedCityName = cityName.EndsWith("市") ? cityName : cityName + "市";
                                    city = await _context.Cities
                                        .Where(c => c.ProvinceId == province.Id && 
                                                   (c.Name == normalizedCityName || c.Name == cityName || c.ShortName == cityName))
                                        .FirstOrDefaultAsync();
                                }
                                
                                return Ok(new
                                {
                                    provinceId = province.Id,
                                    provinceName = province.Name,
                                    cityId = city?.Id,
                                    cityName = city?.Name,
                                    ip = clientIp
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "IP定位服务调用失败，使用默认值");
            }
            
            // 如果IP定位失败，返回默认值（北京）
            var defaultProvince = await _context.Provinces
                .Where(p => p.Name == "北京市" || p.Name == "北京")
                .FirstOrDefaultAsync();
            
            if (defaultProvince != null)
            {
                return Ok(new
                {
                    provinceId = defaultProvince.Id,
                    provinceName = defaultProvince.Name,
                    cityId = (Guid?)null,
                    cityName = (string?)null,
                    ip = clientIp
                });
            }
            
            return Ok(new
            {
                provinceId = (Guid?)null,
                provinceName = (string?)null,
                cityId = (Guid?)null,
                cityName = (string?)null,
                ip = clientIp
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取IP位置信息失败");
            return StatusCode(500, new { message = "获取位置信息失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取客户端IP地址
    /// </summary>
    private string GetClientIpAddress()
    {
        var request = HttpContext.Request;
        
        // 优先从 X-Forwarded-For 头获取（经过代理时）
        var forwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (ips.Length > 0)
            {
                return ips[0];
            }
        }
        
        // 从 X-Real-IP 头获取
        var realIp = request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }
        
        // 从 RemoteIpAddress 获取
        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            // 如果是 IPv6 映射的 IPv4，转换为 IPv4
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                return remoteIp.MapToIPv4().ToString();
            }
            return remoteIp.ToString();
        }
        
        return "127.0.0.1";
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace Medical.API.Controllers;

/// <summary>
/// IP定位控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class IpLocationController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IpLocationController> _logger;

    public IpLocationController(
        IHttpClientFactory httpClientFactory,
        ILogger<IpLocationController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// 获取当前用户的IP归属地信息
    /// </summary>
    /// <returns>省份信息</returns>
    [HttpGet("province")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetProvince()
    {
        try
        {
            // 获取客户端真实IP地址
            var clientIp = GetClientIpAddress();
            _logger.LogInformation("获取到客户端IP: {ClientIp}", clientIp);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(15); // 增加超时时间到15秒

            // 使用客户端IP调用淘宝IP定位服务
            var url = $"http://ip.taobao.com/service/getIpInfo.php?ip={clientIp}";
            _logger.LogInformation("调用淘宝IP定位服务: {Url}", url);
            
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("IP定位服务返回非成功状态码: {StatusCode}", response.StatusCode);
                return Ok(new { province = "" });
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("淘宝IP定位服务返回: {Content}", content);
            
            var ipData = JsonSerializer.Deserialize<JsonElement>(content);

            // 检查返回数据格式：{ code: 0, data: { region: "广东", ... } }
            if (ipData.TryGetProperty("code", out var codeElement) && 
                codeElement.GetInt32() == 0 &&
                ipData.TryGetProperty("data", out var dataElement) &&
                dataElement.TryGetProperty("region", out var regionElement))
            {
                var provinceName = regionElement.GetString() ?? "";
                _logger.LogInformation("解析到省份名称: {ProvinceName}", provinceName);
                
                // 处理省份名称（去掉"省"、"市"、"自治区"等后缀）
                if (!string.IsNullOrEmpty(provinceName))
                {
                    if (provinceName.EndsWith("省"))
                    {
                        provinceName = provinceName.Substring(0, provinceName.Length - 1);
                    }
                    else if (provinceName.EndsWith("市"))
                    {
                        // 直辖市保留"市"
                        if (provinceName != "北京市" && provinceName != "上海市" && 
                            provinceName != "天津市" && provinceName != "重庆市")
                        {
                            provinceName = provinceName.Substring(0, provinceName.Length - 1);
                        }
                    }
                    else if (provinceName.EndsWith("自治区"))
                    {
                        provinceName = provinceName.Replace("自治区", "");
                    }
                    else if (provinceName.EndsWith("特别行政区"))
                    {
                        provinceName = provinceName.Replace("特别行政区", "");
                    }
                }

                _logger.LogInformation("处理后的省份名称: {ProvinceName}", provinceName);
                return Ok(new { province = provinceName });
            }

            return Ok(new { province = "" });
        }
        catch (TaskCanceledException ex)
        {
            // 处理超时异常
            _logger.LogWarning("IP定位服务请求超时: {Message}", ex.Message);
            // 超时失败，返回空省份，不影响前端功能
            return Ok(new { province = "" });
        }
        catch (HttpRequestException ex)
        {
            // 处理HTTP请求异常
            _logger.LogWarning("IP定位服务HTTP请求失败: {Message}", ex.Message);
            return Ok(new { province = "" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取IP归属地信息失败: {Message}", ex.Message);
            // 静默失败，返回空省份，不影响前端功能
            return Ok(new { province = "" });
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


using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Medical.API.Controllers;

/// <summary>
/// 微信相关控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class WeChatController : ControllerBase
{
    private readonly ILogger<WeChatController> _logger;
    private readonly IConfiguration _configuration;

    public WeChatController(ILogger<WeChatController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// 获取微信 JS-SDK 签名配置
    /// </summary>
    /// <param name="url">当前页面的完整URL（不包含#及其后面部分）</param>
    /// <returns>微信 JS-SDK 配置信息</returns>
    [HttpGet("signature")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetSignature([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest(new { message = "URL参数不能为空" });
        }

        try
        {
            // 从配置中获取微信公众平台信息
            var appId = _configuration["WeChat:AppId"];
            var appSecret = _configuration["WeChat:AppSecret"];

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            {
                _logger.LogWarning("微信配置未设置，返回模拟签名");
                // 如果未配置，返回模拟数据（仅用于开发测试）
                return Ok(new
                {
                    appId = appId ?? "your-app-id",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    nonceStr = Guid.NewGuid().ToString("N"),
                    signature = "mock-signature-for-development"
                });
            }

            // 1. 获取 access_token
            var accessToken = await GetAccessToken(appId, appSecret);
            if (string.IsNullOrEmpty(accessToken))
            {
                return StatusCode(500, new { message = "获取微信 access_token 失败" });
            }

            // 2. 获取 jsapi_ticket
            var jsapiTicket = await GetJsApiTicket(accessToken);
            if (string.IsNullOrEmpty(jsapiTicket))
            {
                return StatusCode(500, new { message = "获取微信 jsapi_ticket 失败" });
            }

            // 3. 生成签名
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var nonceStr = Guid.NewGuid().ToString("N");
            var signature = GenerateSignature(jsapiTicket, nonceStr, timestamp, url);

            return Ok(new
            {
                appId = appId,
                timestamp = timestamp,
                nonceStr = nonceStr,
                signature = signature
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取微信签名失败");
            return StatusCode(500, new { message = "获取微信签名失败，请稍后重试" });
        }
    }

    /// <summary>
    /// 获取微信 access_token
    /// </summary>
    private async Task<string?> GetAccessToken(string appId, string appSecret)
    {
        try
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appId}&secret={appSecret}";
            
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            
            var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            
            if (result != null && result.ContainsKey("access_token"))
            {
                return result["access_token"]?.ToString();
            }
            
            _logger.LogError("获取 access_token 失败: {Content}", content);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 access_token 异常");
            return null;
        }
    }

    /// <summary>
    /// 获取微信 jsapi_ticket
    /// </summary>
    private async Task<string?> GetJsApiTicket(string accessToken)
    {
        try
        {
            var url = $"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={accessToken}&type=jsapi";
            
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            
            var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            
            if (result != null && result.ContainsKey("ticket"))
            {
                return result["ticket"]?.ToString();
            }
            
            _logger.LogError("获取 jsapi_ticket 失败: {Content}", content);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 jsapi_ticket 异常");
            return null;
        }
    }

    /// <summary>
    /// 生成微信签名
    /// </summary>
    private string GenerateSignature(string jsapiTicket, string nonceStr, long timestamp, string url)
    {
        // 移除URL中的hash部分
        var cleanUrl = url.Split('#')[0];
        
        // 按照微信要求拼接字符串
        var string1 = $"jsapi_ticket={jsapiTicket}&noncestr={nonceStr}&timestamp={timestamp}&url={cleanUrl}";
        
        // 使用SHA1加密
        using var sha1 = SHA1.Create();
        var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(string1));
        var signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        
        return signature;
    }
}


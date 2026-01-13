using Medical.API.Attributes;
using Medical.API.Models.DTOs;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

namespace Medical.API.Controllers;

/// <summary>
/// AI预诊控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AiDiagnosisController : ControllerBase
{
    private readonly ILogger<AiDiagnosisController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AiDiagnosisController(
        ILogger<AiDiagnosisController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// AI预诊接口
    /// </summary>
    /// <param name="request">预诊请求</param>
    /// <returns>AI预诊响应</returns>
    [HttpPost("chat")]
    [AllowAnonymous] // 允许匿名访问，APP端需要调用
    public async Task<ActionResult<AiDiagnosisResponseDto>> Chat([FromBody] AiDiagnosisRequestDto request)
    {
        // 记录接收到的请求
        _logger.LogInformation("收到AI预诊请求: Prompt={Prompt}, Images={ImagesCount}", 
            request?.Prompt ?? "null", 
            request?.Images?.Count ?? 0);
        
        // 验证请求对象
        if (request == null)
        {
            _logger.LogWarning("AI预诊请求为null");
            return BadRequest(new { message = "请求体不能为空" });
        }
        
        // 验证Prompt不能为空
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            _logger.LogWarning("AI预诊请求验证失败: Prompt为空");
            return BadRequest(new { message = "提示语不能为空" });
        }
        
        // 验证模型状态（在Prompt验证之后，避免重复错误）
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            _logger.LogWarning("AI预诊请求验证失败: {Errors}", string.Join(", ", errors));
            return BadRequest(new { message = string.Join(", ", errors) });
        }
        
        try
        {
            // 从配置中获取API URL和API Key
            var apiUrl = _configuration["DashScope:ApiUrl"] 
                ?? "https://dashscope.aliyuncs.com/api/v1/apps/1c7521e71988498499bd238cc96f7a4e/completions";
            var apiKey = "sk-93091c4d58bb448db20c59e24c161ae6";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("DashScope API Key未配置，使用默认测试响应");
                // 如果没有配置API Key，返回一个测试响应，方便开发调试
                return Ok(new AiDiagnosisResponseDto
                {
                    SessionId = Guid.NewGuid().ToString(),
                    FinishReason = "stop",
                    Text = "您好！我是AI助手。由于API Key未配置，当前返回测试响应。请配置DashScope:ApiKey后即可使用真实的AI服务。",
                    RequestId = Guid.NewGuid().ToString()
                });
            }

            // 创建HttpClient
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = TimeSpan.FromSeconds(120); // 设置超时时间为120秒，AI响应可能需要更长时间

            // 构建 messages 数组（支持 text + image_url）
            object? requestBody = null;
            
            // 如果只有文本（没有图片）
            if (!string.IsNullOrWhiteSpace(request.Prompt) && (request.Images == null || request.Images.Count == 0))
            {
                requestBody = new
                {
                    input = new
                    {
                        messages = new object[]
                        {
                     new { role = "system", content = "You are a helpful assistant." },
                     new { role = "user", content = request.Prompt }
                        }
                    },
                    parameters = new
                    {
                        result_format = "message"
                    }
                };
            }
            // 如果有图片（可能同时有文本）
            else if (request.Images != null && request.Images.Count > 0)
            {
                var imageContents = new List<object>();
                
                // 添加图片
                foreach (var img in request.Images)
                {
                    if (!string.IsNullOrWhiteSpace(img.Url))
                    {
                        imageContents.Add(new
                        {
                            type = "image_url",
                            image_url = new
                            {
                                url = img.Url,
                            }
                        });
                    }
                }
                
                // 添加文本（如果有）
                if (!string.IsNullOrWhiteSpace(request.Prompt))
                {
                    imageContents.Add(new
                    {
                        type = "text",
                        text = request.Prompt
                    });
                }
                
                requestBody = new
                {
                    input = new
                    {
                        messages = new[]
                        {
                            new
                            {
                                role = "user",
                                content = imageContents
                            }
                        },
                        parameters = new { temperature = 0.7, max_tokens = 1000 }
                    }
                };
            }
            
            // 如果没有构建请求体，说明请求无效
            if (requestBody == null)
            {
                _logger.LogWarning("AI预诊请求无效: Prompt为空或请求格式不正确");
                return BadRequest(new { message = "请求无效：提示语不能为空" });
            }
            
            // 序列化请求体
            var jsonRequestBody = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var requestContent = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

            // 发送POST请求
            _logger.LogInformation("发送AI预诊请求: {Prompt}", request.Prompt);
            var response = await client.PostAsync(apiUrl, requestContent);

            // 检查响应状态码
            if (response.IsSuccessStatusCode)
            {
                // 读取响应内容
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("AI预诊响应成功: {ResponseBody}", responseBody);

                // 反序列化JSON响应
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var locationData = JsonSerializer.Deserialize<DashScopeResponse>(responseBody, jsonOptions);

                if (locationData?.Output == null)
                {
                    _logger.LogError("AI预诊响应数据格式错误: {ResponseBody}", responseBody);
                    return StatusCode(500, new { message = "AI服务返回数据格式错误" });
                }

                // 转换为DTO
                var result = new AiDiagnosisResponseDto
                {
                    SessionId = locationData.Output.SessionId ?? string.Empty,
                    FinishReason = locationData.Output.FinishReason ?? string.Empty,
                    Text = locationData.Output.Text ?? string.Empty,
                    RequestId = locationData.RequestId ?? string.Empty,
                    Usage = locationData.Usage != null ? new AiDiagnosisUsageDto
                    {
                        Models = locationData.Usage.Models?.Select(m => new AiDiagnosisModelUsageDto
                        {
                            ModelId = m.ModelId ?? string.Empty,
                            InputTokens = m.InputTokens,
                            OutputTokens = m.OutputTokens
                        }).ToList() ?? new List<AiDiagnosisModelUsageDto>()
                    } : null
                };

                return Ok(result);
            }
            else
            {
                // 处理错误响应
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("AI预诊请求失败: 状态码={StatusCode}, 错误信息={Error}", 
                    response.StatusCode, errorBody);
                
                return StatusCode((int)response.StatusCode, new 
                { 
                    message = $"AI服务请求失败: {response.StatusCode}",
                    error = errorBody 
                });
            }
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "AI预诊请求超时");
            return StatusCode(500, new { message = "AI服务请求超时，请稍后重试" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI预诊请求发生异常");
            return StatusCode(500, new { message = "AI服务请求失败，请稍后重试" });
        }
    }
    private readonly HttpClient httpClient = new HttpClient();
    private async Task<string> SendPostRequestAsync(string url, string jsonContent, string apiKey)
    {
        using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
        {
            // 设置请求头
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 发送请求并获取响应
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            // 处理响应
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return $"请求失败: {response.StatusCode}";
            }
        }
    }
    /// <summary>
    /// DashScope API响应模型（内部使用）
    /// </summary>
    private class DashScopeResponse
    {
        public DashScopeOutput? Output { get; set; }
        public DashScopeUsage? Usage { get; set; }
        public string? RequestId { get; set; }
    }

    private class DashScopeOutput
    {
        public string? SessionId { get; set; }
        public string? FinishReason { get; set; }
        public string? Text { get; set; }
    }

    private class DashScopeUsage
    {
        public List<DashScopeModelUsage>? Models { get; set; }
    }

    private class DashScopeModelUsage
    {
        public string? ModelId { get; set; }
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
    }

}


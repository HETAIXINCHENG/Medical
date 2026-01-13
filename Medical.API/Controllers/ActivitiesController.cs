using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Medical.API.Attributes;
using System.Text.Json;

namespace Medical.API.Controllers;

/// <summary>
/// 活动控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ActivitiesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(MedicalDbContext context, ILogger<ActivitiesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取活动列表
    /// </summary>
    /// <param name="isHot">是否热门</param>
    /// <param name="activityType">活动类型</param>
    /// <param name="keyword">关键词搜索（标题、描述）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>活动列表</returns>
    [HttpGet]
    [AllowAnonymous] // 允许匿名访问，首页需要显示活动列表
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetActivities(
        [FromQuery] bool? isHot = null,
        [FromQuery] string? activityType = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Activities.AsQueryable();

        if (isHot.HasValue)
        {
            query = query.Where(a => a.IsHot == isHot.Value);
        }

        if (!string.IsNullOrEmpty(activityType))
        {
            query = query.Where(a => a.ActivityType == activityType);
        }

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(a => 
                a.Title.Contains(keyword) ||
                (a.Description != null && a.Description.Contains(keyword)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(a => a.SortOrder)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });

        var activities = await query
            .OrderBy(a => a.SortOrder)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(activities);
    }

    /// <summary>
    /// 根据ID获取活动详情
    /// </summary>
    /// <param name="id">活动ID</param>
    /// <returns>活动详情</returns>
    [HttpGet("{id}")]
    [RequirePermission("activity.view")]
    [ProducesResponseType(typeof(Activity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Activity>> GetActivityById(Guid id)
    {
        var activity = await _context.Activities.FindAsync(id);

        if (activity == null)
        {
            return NotFound(new { message = "活动不存在" });
        }

        return Ok(activity);
    }

    /// <summary>
    /// 创建活动
    /// </summary>
    /// <param name="activity">活动信息</param>
    /// <returns>创建的活动</returns>
    [HttpPost]
    [RequirePermission("activity.create")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(Activity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Activity>> CreateActivity([FromBody] Activity activity)
    {
        _logger.LogInformation("创建活动请求: Title={Title}, ActivityType={ActivityType}, CoverImageUrl={CoverImageUrl}, StartTime={StartTime}, EndTime={EndTime}", 
            activity?.Title, activity?.ActivityType, activity?.CoverImageUrl, activity?.StartTime, activity?.EndTime);
        
        // 记录完整的请求数据（用于调试）
        _logger.LogDebug("完整活动数据: {Activity}", System.Text.Json.JsonSerializer.Serialize(activity));

        // 验证模型状态
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"));
            _logger.LogWarning("模型验证失败: {Errors}", string.Join(", ", errors));
            return BadRequest(new { message = "数据验证失败", errors });
        }

        if (activity == null)
        {
            _logger.LogWarning("创建活动失败: 请求数据为空");
            return BadRequest(new { message = "请求数据不能为空" });
        }

        if (string.IsNullOrWhiteSpace(activity.Title))
        {
            return BadRequest(new { message = "活动标题不能为空" });
        }

        // 确保空字符串转换为 null（对于可选字段）
        if (string.IsNullOrWhiteSpace(activity.CoverImageUrl))
        {
            activity.CoverImageUrl = null;
        }
        else if (activity.CoverImageUrl != null)
        {
            // 检查长度限制（1000 字符）
            if (activity.CoverImageUrl.Length > 1000)
            {
                _logger.LogWarning("封面图片URL过长: {Length} 字符，最大允许 1000 字符", activity.CoverImageUrl.Length);
                return BadRequest(new { 
                    message = $"封面图片URL过长（{activity.CoverImageUrl.Length} 字符），最大允许 1000 字符。请使用较短的图片路径。",
                    field = "coverImageUrl",
                    maxLength = 1000,
                    actualLength = activity.CoverImageUrl.Length
                });
            }
        }
        if (string.IsNullOrWhiteSpace(activity.Subtitle))
        {
            activity.Subtitle = null;
        }
        if (string.IsNullOrWhiteSpace(activity.Description))
        {
            activity.Description = null;
        }
        if (string.IsNullOrWhiteSpace(activity.ActivityType))
        {
            activity.ActivityType = null;
        }
        if (string.IsNullOrWhiteSpace(activity.DiscountInfo))
        {
            activity.DiscountInfo = null;
        }

        activity.Id = Guid.NewGuid();
        activity.CreatedAt = DateTime.UtcNow;
        activity.UpdatedAt = DateTime.UtcNow;

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("活动创建成功: Id={Id}, Title={Title}", activity.Id, activity.Title);

        return CreatedAtAction(nameof(GetActivityById), new { id = activity.Id }, activity);
    }

    /// <summary>
    /// 更新活动
    /// </summary>
    /// <param name="id">活动ID</param>
    /// <param name="activity">活动信息</param>
    /// <returns>更新后的活动</returns>
    [HttpPut("{id}")]
    [RequirePermission("activity.update")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(Activity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Activity>> UpdateActivity(Guid id, [FromBody] Activity activity)
    {
        var existingActivity = await _context.Activities.FindAsync(id);
        if (existingActivity == null)
        {
            return NotFound(new { message = "活动不存在" });
        }

        existingActivity.Title = activity.Title;
        existingActivity.Subtitle = activity.Subtitle;
        existingActivity.ActivityType = activity.ActivityType;
        existingActivity.DiscountInfo = activity.DiscountInfo;
        existingActivity.CoverImageUrl = activity.CoverImageUrl;
        existingActivity.SortOrder = activity.SortOrder;
        existingActivity.IsHot = activity.IsHot;
        existingActivity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(existingActivity);
    }

    /// <summary>
    /// 删除活动
    /// </summary>
    /// <param name="id">活动ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("activity.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null)
        {
            return NotFound(new { message = "活动不存在" });
        }

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}


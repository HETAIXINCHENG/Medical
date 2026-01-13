using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Medical.API.Controllers;

/// <summary>
/// 订阅控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(MedicalDbContext context, ILogger<SubscriptionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取当前用户订阅的医生列表
    /// </summary>
    /// <returns>订阅的医生列表</returns>
    [HttpGet("my-subscriptions")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMySubscriptions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var subscriptions = await _context.UserDoctorSubscriptions
            .Where(s => s.UserId == userId)
            .Include(s => s.Doctor)
                .ThenInclude(d => d.Department)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.DoctorId,
                DoctorName = s.Doctor.Name,
                DoctorTitle = s.Doctor.Title,
                DoctorAvatar = s.Doctor.AvatarUrl,
                DoctorDepartment = s.Doctor.Department != null ? s.Doctor.Department.Name : null,
                DoctorHospital = s.Doctor.Hospital,
                s.CreatedAt
            })
            .ToListAsync();

        return Ok(subscriptions);
    }

    /// <summary>
    /// 订阅医生
    /// </summary>
    /// <param name="doctorId">医生ID</param>
    /// <returns>订阅结果</returns>
    [HttpPost("{doctorId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SubscribeDoctor(Guid doctorId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        // 检查医生是否存在
        var doctor = await _context.Doctors.FindAsync(doctorId);
        if (doctor == null)
        {
            return NotFound(new { message = "医生不存在" });
        }

        // 检查是否已经订阅
        var existingSubscription = await _context.UserDoctorSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.DoctorId == doctorId);

        if (existingSubscription != null)
        {
            return BadRequest(new { message = "您已经订阅过该医生了" });
        }

        // 创建订阅
        var subscription = new UserDoctorSubscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DoctorId = doctorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.UserDoctorSubscriptions.Add(subscription);
        
        // 更新医生的粉丝数
        doctor.FollowerCount++;
        doctor.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return Ok(new { message = "订阅成功", subscriptionId = subscription.Id });
    }

    /// <summary>
    /// 取消订阅医生
    /// </summary>
    /// <param name="doctorId">医生ID</param>
    /// <returns>取消订阅结果</returns>
    [HttpDelete("{doctorId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UnsubscribeDoctor(Guid doctorId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var subscription = await _context.UserDoctorSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.DoctorId == doctorId);

        if (subscription == null)
        {
            return NotFound(new { message = "未找到订阅记录" });
        }

        _context.UserDoctorSubscriptions.Remove(subscription);
        
        // 更新医生的粉丝数
        var doctor = await _context.Doctors.FindAsync(doctorId);
        if (doctor != null && doctor.FollowerCount > 0)
        {
            doctor.FollowerCount--;
            doctor.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();

        return Ok(new { message = "取消订阅成功" });
    }

    /// <summary>
    /// 检查是否已订阅医生
    /// </summary>
    /// <param name="doctorId">医生ID</param>
    /// <returns>是否已订阅</returns>
    [HttpGet("check/{doctorId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> CheckSubscription(Guid doctorId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Ok(new { isSubscribed = false });
        }

        var isSubscribed = await _context.UserDoctorSubscriptions
            .AnyAsync(s => s.UserId == userId && s.DoctorId == doctorId);

        return Ok(new { isSubscribed });
    }

    /// <summary>
    /// 获取订阅医生发布的健康知识列表
    /// </summary>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>健康知识列表</returns>
    [HttpGet("health-knowledge")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSubscribedDoctorsHealthKnowledge(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        // 获取用户订阅的医生ID列表
        var subscribedDoctorIds = await _context.UserDoctorSubscriptions
            .Where(s => s.UserId == userId)
            .Select(s => s.DoctorId)
            .ToListAsync();

        if (subscribedDoctorIds.Count == 0)
        {
            return Ok(new { items = new List<object>(), total = 0, page, pageSize });
        }

        // 获取这些医生发布的健康知识
        var knowledge = await _context.HealthKnowledge
            .Where(k => !string.IsNullOrEmpty(k.Author) && 
                       subscribedDoctorIds.Any(did => k.Author == did.ToString()))
            .OrderByDescending(k => k.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var total = await _context.HealthKnowledge
            .CountAsync(k => !string.IsNullOrEmpty(k.Author) && 
                            subscribedDoctorIds.Any(did => k.Author == did.ToString()));

        // 获取所有相关的医生信息
        var doctorIds = knowledge
            .Where(k => !string.IsNullOrEmpty(k.Author) && Guid.TryParse(k.Author, out _))
            .Select(k => Guid.Parse(k.Author))
            .Distinct()
            .ToList();

        var doctors = await _context.Doctors
            .Include(d => d.Department)
            .Where(d => doctorIds.Contains(d.Id))
            .ToListAsync();

        var doctorDict = doctors.ToDictionary(d => d.Id, d => d);

        // 为每个健康知识关联医生信息
        var result = knowledge.Select(k => {
            if (!string.IsNullOrEmpty(k.Author) && Guid.TryParse(k.Author, out var doctorId) && doctorDict.TryGetValue(doctorId, out var doctor))
            {
                return new {
                    k.Id,
                    k.Title,
                    k.Content,
                    k.Summary,
                    k.CoverImageUrl,
                    k.Category,
                    k.IsHot,
                    k.IsRecommended,
                    k.ReadCount,
                    k.FavoriteCount,
                    k.CreatedAt,
                    k.UpdatedAt,
                    AuthorName = doctor.Name,
                    AuthorTitle = doctor.Title,
                    AuthorAvatar = doctor.AvatarUrl,
                    AuthorDepartment = doctor.Department?.Name,
                    AuthorHospital = doctor.Hospital
                };
            }
            
            return new {
                k.Id,
                k.Title,
                k.Content,
                k.Summary,
                k.CoverImageUrl,
                k.Category,
                k.IsHot,
                k.IsRecommended,
                k.ReadCount,
                k.FavoriteCount,
                k.CreatedAt,
                k.UpdatedAt,
                AuthorName = (string?)null,
                AuthorTitle = (string?)null,
                AuthorAvatar = (string?)null,
                AuthorDepartment = (string?)null,
                AuthorHospital = (string?)null
            };
        }).ToList();

        return Ok(new { items = result, total, page, pageSize });
    }
}


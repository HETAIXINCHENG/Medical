using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 点赞管理控制器（管理后台使用）
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PostLikesController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<PostLikesController> _logger;

    public PostLikesController(MedicalDbContext context, ILogger<PostLikesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取点赞列表
    /// </summary>
    [HttpGet]
    [RequirePermission("post-likes.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetLikes(
        [FromQuery] Guid? postId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.PostLikes
            .Include(l => l.User)
            .Include(l => l.Post)
                .ThenInclude(p => p.PatientSupportGroup)
            .AsQueryable();

        if (postId.HasValue)
        {
            query = query.Where(l => l.PostId == postId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(l => l.UserId == userId.Value);
        }

        var total = await query.CountAsync();

        var likes = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = likes.Select(l => new
        {
            l.Id,
            l.PostId,
            PostTitle = l.Post.Title,
            GroupName = l.Post.PatientSupportGroup.Name,
            UserName = l.User.Username,
            l.CreatedAt
        }).ToList();

        return Ok(new { items = dtos, total, page, pageSize });
    }

    /// <summary>
    /// 获取点赞详情
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("post-likes.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetLikeById(Guid id)
    {
        var like = await _context.PostLikes
            .Include(l => l.User)
            .Include(l => l.Post)
                .ThenInclude(p => p.PatientSupportGroup)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (like == null)
        {
            return NotFound(new { message = "点赞不存在" });
        }

        var dto = new
        {
            like.Id,
            like.PostId,
            PostTitle = like.Post.Title,
            GroupName = like.Post.PatientSupportGroup.Name,
            UserName = like.User.Username,
            like.CreatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// 删除点赞
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("post-likes.delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteLike(Guid id)
    {
        var like = await _context.PostLikes
            .Include(l => l.Post)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (like == null)
        {
            return NotFound(new { message = "点赞不存在" });
        }

        // 更新帖子的点赞数
        like.Post.LikeCount = Math.Max(0, like.Post.LikeCount - 1);
        like.Post.UpdatedAt = DateTime.UtcNow;

        _context.PostLikes.Remove(like);
        await _context.SaveChangesAsync();

        return Ok(new { message = "删除成功" });
    }
}


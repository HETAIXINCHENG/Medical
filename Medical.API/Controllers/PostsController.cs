using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.DTOs;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 帖子管理控制器（管理后台使用）
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PostsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<PostsController> _logger;

    public PostsController(MedicalDbContext context, ILogger<PostsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取帖子列表
    /// </summary>
    [HttpGet]
    [RequirePermission("posts.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPosts(
        [FromQuery] Guid? groupId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Posts
            .Include(p => p.User)
            .Include(p => p.PatientSupportGroup)
                .ThenInclude(g => g.Doctor)
            .AsQueryable();

        if (groupId.HasValue)
        {
            query = query.Where(p => p.PatientSupportGroupId == groupId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(p => p.UserId == userId.Value);
        }

        if (isDeleted.HasValue)
        {
            query = query.Where(p => p.IsDeleted == isDeleted.Value);
        }
        else
        {
            query = query.Where(p => !p.IsDeleted);
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(p => p.Title.Contains(keyword) || p.Content.Contains(keyword));
        }

        var total = await query.CountAsync();

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = posts.Select(p => new
        {
            p.Id,
            p.PatientSupportGroupId,
            GroupName = p.PatientSupportGroup.Name,
            DoctorName = p.PatientSupportGroup.Doctor.Name,
            p.Title,
            Content = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
            p.Tag,
            p.ReadCount,
            p.CommentCount,
            p.LikeCount,
            p.IsPinned,
            p.IsDeleted,
            AuthorName = p.User.Username,
            AttachmentUrls = string.IsNullOrEmpty(p.AttachmentUrls)
                ? null
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.AttachmentUrls),
            p.CreatedAt,
            p.UpdatedAt,
            p.LastReplyAt
        }).ToList();

        return Ok(new { items = dtos, total, page, pageSize });
    }

    /// <summary>
    /// 获取帖子详情
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("posts.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetPostById(Guid id)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.PatientSupportGroup)
                .ThenInclude(g => g.Doctor)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound(new { message = "帖子不存在" });
        }

        var dto = new
        {
            post.Id,
            post.PatientSupportGroupId,
            GroupName = post.PatientSupportGroup.Name,
            DoctorName = post.PatientSupportGroup.Doctor.Name,
            post.Title,
            post.Content,
            post.Tag,
            post.ReadCount,
            post.CommentCount,
            post.LikeCount,
            post.IsPinned,
            post.IsDeleted,
            AuthorName = post.User.Username,
            AttachmentUrls = string.IsNullOrEmpty(post.AttachmentUrls)
                ? null
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(post.AttachmentUrls),
            post.CreatedAt,
            post.UpdatedAt,
            post.LastReplyAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// 更新帖子
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("posts.update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdatePost(Guid id, [FromBody] UpdatePostDto dto)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound(new { message = "帖子不存在" });
        }

        if (dto.Title != null) post.Title = dto.Title;
        if (dto.Content != null) post.Content = dto.Content;
        if (dto.Tag != null) post.Tag = dto.Tag;
        if (dto.IsPinned.HasValue) post.IsPinned = dto.IsPinned.Value;
        if (dto.IsDeleted.HasValue) post.IsDeleted = dto.IsDeleted.Value;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "更新成功" });
    }

    /// <summary>
    /// 删除帖子（软删除）
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("posts.delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePost(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound(new { message = "帖子不存在" });
        }

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "删除成功" });
    }
}

/// <summary>
/// 更新帖子DTO
/// </summary>
public class UpdatePostDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Tag { get; set; }
    public bool? IsPinned { get; set; }
    public bool? IsDeleted { get; set; }
}


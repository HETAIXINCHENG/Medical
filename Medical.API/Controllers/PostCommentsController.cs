using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Medical.API.Attributes;

namespace Medical.API.Controllers;

/// <summary>
/// 评论管理控制器（管理后台使用）
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class PostCommentsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<PostCommentsController> _logger;

    public PostCommentsController(MedicalDbContext context, ILogger<PostCommentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取评论列表
    /// </summary>
    [HttpGet]
    [RequirePermission("post-comments.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetComments(
        [FromQuery] Guid? postId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.PostComments
            .Include(c => c.User)
            .Include(c => c.Post)
                .ThenInclude(p => p.PatientSupportGroup)
            .AsQueryable();

        if (postId.HasValue)
        {
            query = query.Where(c => c.PostId == postId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(c => c.UserId == userId.Value);
        }

        if (isDeleted.HasValue)
        {
            query = query.Where(c => c.IsDeleted == isDeleted.Value);
        }
        else
        {
            query = query.Where(c => !c.IsDeleted);
        }

        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(c => c.Content.Contains(keyword));
        }

        var total = await query.CountAsync();

        var comments = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = comments.Select(c => new
        {
            c.Id,
            c.PostId,
            PostTitle = c.Post.Title,
            GroupName = c.Post.PatientSupportGroup.Name,
            c.Content,
            AttachmentUrls = string.IsNullOrEmpty(c.AttachmentUrls)
                ? null
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(c.AttachmentUrls),
            c.ParentCommentId,
            AuthorName = c.User.Username,
            c.IsDeleted,
            c.CreatedAt,
            c.UpdatedAt
        }).ToList();

        return Ok(new { items = dtos, total, page, pageSize });
    }

    /// <summary>
    /// 获取评论详情
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("post-comments.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetCommentById(Guid id)
    {
        var comment = await _context.PostComments
            .Include(c => c.User)
            .Include(c => c.Post)
                .ThenInclude(p => p.PatientSupportGroup)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (comment == null)
        {
            return NotFound(new { message = "评论不存在" });
        }

        var dto = new
        {
            comment.Id,
            comment.PostId,
            PostTitle = comment.Post.Title,
            GroupName = comment.Post.PatientSupportGroup.Name,
            comment.Content,
            AttachmentUrls = string.IsNullOrEmpty(comment.AttachmentUrls)
                ? null
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(comment.AttachmentUrls),
            comment.ParentCommentId,
            AuthorName = comment.User.Username,
            comment.IsDeleted,
            comment.CreatedAt,
            comment.UpdatedAt
        };

        return Ok(dto);
    }

    /// <summary>
    /// 更新评论
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("post-comments.update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentDto dto)
    {
        var comment = await _context.PostComments.FindAsync(id);
        if (comment == null)
        {
            return NotFound(new { message = "评论不存在" });
        }

        if (dto.Content != null) comment.Content = dto.Content;
        if (dto.IsDeleted.HasValue) comment.IsDeleted = dto.IsDeleted.Value;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "更新成功" });
    }

    /// <summary>
    /// 删除评论（软删除）
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("post-comments.delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteComment(Guid id)
    {
        var comment = await _context.PostComments.FindAsync(id);
        if (comment == null)
        {
            return NotFound(new { message = "评论不存在" });
        }

        comment.IsDeleted = true;
        comment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "删除成功" });
    }
}

/// <summary>
/// 更新评论DTO
/// </summary>
public class UpdateCommentDto
{
    public string? Content { get; set; }
    public bool? IsDeleted { get; set; }
}


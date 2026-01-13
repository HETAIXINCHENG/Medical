using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medical.API.Data;
using Medical.API.Models.DTOs;
using Medical.API.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Medical.API.Attributes;
using Microsoft.AspNetCore.Hosting;

namespace Medical.API.Controllers;

/// <summary>
/// 患友会控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatientSupportGroupsController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly ILogger<PatientSupportGroupsController> _logger;
    private readonly IWebHostEnvironment _environment;

    public PatientSupportGroupsController(MedicalDbContext context, ILogger<PatientSupportGroupsController> logger, IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// 获取患友会列表（管理后台使用）
    /// 注意：患友会是自动生成的，每个医生都有一个对应的患友会
    /// </summary>
    [HttpGet]
    [RequirePermission("patient-support-groups.view")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPatientSupportGroups(
        [FromQuery] Guid? doctorId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        // 确保所有医生都有对应的患友会（自动创建缺失的）
        var allDoctors = await _context.Doctors
            .Include(d => d.Department)
            .ToListAsync();

        foreach (var doctor in allDoctors)
        {
            var existingGroup = await _context.PatientSupportGroups
                .FirstOrDefaultAsync(g => g.DoctorId == doctor.Id);

            if (existingGroup == null)
            {
                // 自动创建缺失的患友会
                var supportGroup = new PatientSupportGroup
                {
                    Id = Guid.NewGuid(),
                    DoctorId = doctor.Id,
                    Name = $"{doctor.Name}医生的患友会",
                    Description = $"欢迎加入{doctor.Name}医生的患友会，与医生和其他患者交流。",
                    PostCount = 0,
                    TotalReadCount = 0,
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.PatientSupportGroups.Add(supportGroup);

                // 创建默认会规
                var rules = new GroupRules
                {
                    Id = Guid.NewGuid(),
                    PatientSupportGroupId = supportGroup.Id,
                    Content = GetDefaultRulesContent(doctor.Name),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.GroupRules.Add(rules);
            }
        }

        await _context.SaveChangesAsync();

        var query = _context.PatientSupportGroups
            .Include(g => g.Doctor)
                .ThenInclude(d => d.Department)
            .AsQueryable();

        if (doctorId.HasValue)
        {
            query = query.Where(g => g.DoctorId == doctorId.Value);
        }

        if (isEnabled.HasValue)
        {
            query = query.Where(g => g.IsEnabled == isEnabled.Value);
        }

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(g => 
                g.Name.Contains(keyword) ||
                (g.Description != null && g.Description.Contains(keyword)) ||
                g.Doctor.Name.Contains(keyword));
        }

        var total = await query.CountAsync();

        var groups = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new PatientSupportGroupDto
            {
                Id = g.Id,
                DoctorId = g.DoctorId,
                Name = g.Name,
                Description = g.Description,
                PostCount = g.PostCount,
                TotalReadCount = g.TotalReadCount,
                IsEnabled = g.IsEnabled,
                CreatedAt = g.CreatedAt,
                DoctorName = g.Doctor.Name,
                DoctorTitle = g.Doctor.Title,
                DoctorAvatarUrl = g.Doctor.AvatarUrl,
                DoctorHospital = g.Doctor.Hospital,
                DoctorDepartmentName = g.Doctor.Department != null ? g.Doctor.Department.Name : null
            })
            .ToListAsync();

        return Ok(new { items = groups, total, page, pageSize });
    }

    /// <summary>
    /// 根据ID获取患友会详情（管理后台使用）
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("patient-support-groups.view")]
    [ProducesResponseType(typeof(PatientSupportGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientSupportGroupDto>> GetPatientSupportGroupById(Guid id)
    {
        var group = await _context.PatientSupportGroups
            .Include(g => g.Doctor)
                .ThenInclude(d => d.Department)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound(new { message = "患友会不存在" });
        }

        var dto = new PatientSupportGroupDto
        {
            Id = group.Id,
            DoctorId = group.DoctorId,
            Name = group.Name,
            Description = group.Description,
            PostCount = group.PostCount,
            TotalReadCount = group.TotalReadCount,
            IsEnabled = group.IsEnabled,
            CreatedAt = group.CreatedAt,
            DoctorName = group.Doctor.Name,
            DoctorTitle = group.Doctor.Title,
            DoctorAvatarUrl = group.Doctor.AvatarUrl,
            DoctorHospital = group.Doctor.Hospital,
            DoctorDepartmentName = group.Doctor.Department?.Name
        };

        return Ok(dto);
    }

    /// <summary>
    /// 创建患友会（已废弃：患友会现在在创建医生时自动生成）
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(PatientSupportGroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Obsolete("患友会现在在创建医生时自动生成，此接口已废弃")]
    public async Task<ActionResult<PatientSupportGroupDto>> CreatePatientSupportGroup([FromBody] CreatePatientSupportGroupDto dto)
    {
        return BadRequest(new { message = "患友会已改为在创建医生时自动生成，无法手动创建。如需创建患友会，请先创建医生。" });
    }

    /// <summary>
    /// 更新患友会（管理后台使用）
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("patient-support-groups.update")]
    [ProducesResponseType(typeof(PatientSupportGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatientSupportGroupDto>> UpdatePatientSupportGroup(Guid id, [FromBody] UpdatePatientSupportGroupDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "请求数据不能为空" });
        }

        var group = await _context.PatientSupportGroups
            .Include(g => g.Doctor)
                .ThenInclude(d => d.Department)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound(new { message = "患友会不存在" });
        }

        // 如果更新了医生ID，验证新医生是否存在
        if (dto.DoctorId.HasValue && dto.DoctorId.Value != group.DoctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId.Value);

            if (doctor == null)
            {
                return BadRequest(new { message = "医生不存在" });
            }

            // 检查新医生是否已有患友会
            var existingGroup = await _context.PatientSupportGroups
                .FirstOrDefaultAsync(g => g.DoctorId == dto.DoctorId.Value && g.Id != id);

            if (existingGroup != null)
            {
                return BadRequest(new { message = "该医生已存在患友会" });
            }

            group.DoctorId = dto.DoctorId.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            group.Name = dto.Name;
        }

        if (dto.Description != null)
        {
            group.Description = dto.Description;
        }

        if (dto.IsEnabled.HasValue)
        {
            group.IsEnabled = dto.IsEnabled.Value;
        }

        group.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // 重新加载以获取最新的医生信息
        await _context.Entry(group).Reference(g => g.Doctor).LoadAsync();
        await _context.Entry(group.Doctor).Reference(d => d.Department).LoadAsync();

        var resultDto = new PatientSupportGroupDto
        {
            Id = group.Id,
            DoctorId = group.DoctorId,
            Name = group.Name,
            Description = group.Description,
            PostCount = group.PostCount,
            TotalReadCount = group.TotalReadCount,
            IsEnabled = group.IsEnabled,
            CreatedAt = group.CreatedAt,
            DoctorName = group.Doctor.Name,
            DoctorTitle = group.Doctor.Title,
            DoctorAvatarUrl = group.Doctor.AvatarUrl,
            DoctorHospital = group.Doctor.Hospital,
            DoctorDepartmentName = group.Doctor.Department?.Name
        };

        return Ok(resultDto);
    }

    /// <summary>
    /// 冻结/解冻患友会（管理后台使用）
    /// </summary>
    [HttpPost("{id}/toggle-freeze")]
    [RequirePermission("patient-support-groups.update")]
    [ProducesResponseType(typeof(PatientSupportGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientSupportGroupDto>> ToggleFreezePatientSupportGroup(Guid id)
    {
        var group = await _context.PatientSupportGroups
            .Include(g => g.Doctor)
                .ThenInclude(d => d.Department)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound(new { message = "患友会不存在" });
        }

        // 切换冻结状态（IsEnabled 取反）
        group.IsEnabled = !group.IsEnabled;
        group.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var dto = new PatientSupportGroupDto
        {
            Id = group.Id,
            DoctorId = group.DoctorId,
            Name = group.Name,
            Description = group.Description,
            PostCount = group.PostCount,
            TotalReadCount = group.TotalReadCount,
            IsEnabled = group.IsEnabled,
            CreatedAt = group.CreatedAt,
            DoctorName = group.Doctor.Name,
            DoctorTitle = group.Doctor.Title,
            DoctorAvatarUrl = group.Doctor.AvatarUrl,
            DoctorHospital = group.Doctor.Hospital,
            DoctorDepartmentName = group.Doctor.Department?.Name
        };

        _logger.LogInformation("患友会{Action}成功: Id={Id}, Name={Name}", 
            group.IsEnabled ? "解冻" : "冻结", group.Id, group.Name);

        return Ok(dto);
    }

    /// <summary>
    /// 根据医生ID获取患友会
    /// </summary>
    [HttpGet("by-doctor/{doctorId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PatientSupportGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientSupportGroupDto>> GetByDoctorId(Guid doctorId)
    {
        var group = await _context.PatientSupportGroups
            .Include(g => g.Doctor)
                .ThenInclude(d => d.Department)
            .FirstOrDefaultAsync(g => g.DoctorId == doctorId && g.IsEnabled);

        if (group == null)
        {
            // 如果不存在，自动创建一个
            var doctor = await _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return NotFound(new { message = "医生不存在" });
            }

            group = new PatientSupportGroup
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                Name = $"{doctor.Name}医生的患友会",
                Description = $"欢迎加入{doctor.Name}医生的患友会",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PatientSupportGroups.Add(group);
            await _context.SaveChangesAsync();

            // 创建默认会规
            var rules = new GroupRules
            {
                Id = Guid.NewGuid(),
                PatientSupportGroupId = group.Id,
                Content = GetDefaultRulesContent(doctor.Name),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.GroupRules.Add(rules);
            await _context.SaveChangesAsync();
        }

        // 统计发布人数（不重复的用户数）
        var postCount = await _context.Posts
            .Where(p => p.PatientSupportGroupId == group.Id && !p.IsDeleted)
            .Select(p => p.UserId)
            .Distinct()
            .CountAsync();

        // 统计总阅读数
        var totalReadCount = await _context.Posts
            .Where(p => p.PatientSupportGroupId == group.Id && !p.IsDeleted)
            .SumAsync(p => (int?)p.ReadCount) ?? 0;

        // 更新统计数据
        group.PostCount = postCount;
        group.TotalReadCount = totalReadCount;
        group.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var dto = new PatientSupportGroupDto
        {
            Id = group.Id,
            DoctorId = group.DoctorId,
            Name = group.Name,
            Description = group.Description,
            PostCount = group.PostCount,
            TotalReadCount = group.TotalReadCount,
            IsEnabled = group.IsEnabled,
            CreatedAt = group.CreatedAt,
            DoctorName = group.Doctor.Name,
            DoctorTitle = group.Doctor.Title,
            DoctorAvatarUrl = group.Doctor.AvatarUrl,
            DoctorHospital = group.Doctor.Hospital,
            DoctorDepartmentName = group.Doctor.Department?.Name
        };

        return Ok(dto);
    }

    /// <summary>
    /// 获取患友会的帖子列表（需要登录）
    /// </summary>
    [HttpGet("{groupId}/posts")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPosts(
        Guid groupId,
        [FromQuery] string sortBy = "reply", // reply: 回复优先, post: 发布优先
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // 检查患友会是否存在且已启用
        var group = await _context.PatientSupportGroups.FindAsync(groupId);
        if (group == null || !group.IsEnabled)
        {
            return NotFound(new { message = "患友会不存在或已冻结" });
        }

        var query = _context.Posts
            .Include(p => p.User)
            .Where(p => p.PatientSupportGroupId == groupId && !p.IsDeleted)
            .AsQueryable();

        // 排序
        if (sortBy == "reply")
        {
            // 回复优先：按最后回复时间排序，然后按创建时间
            query = query.OrderByDescending(p => p.LastReplyAt ?? p.CreatedAt)
                .ThenByDescending(p => p.CreatedAt);
        }
        else
        {
            // 发布优先：按创建时间排序
            query = query.OrderByDescending(p => p.CreatedAt);
        }

        var total = await query.CountAsync();

        var posts = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = posts.Select(p => new PostDto
        {
            Id = p.Id,
            PatientSupportGroupId = p.PatientSupportGroupId,
            Title = p.Title,
            Content = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
            Tag = p.Tag,
            ReadCount = p.ReadCount,
            CommentCount = p.CommentCount,
            LikeCount = p.LikeCount,
            LastReplyAt = p.LastReplyAt,
            IsPinned = p.IsPinned,
            CreatedAt = p.CreatedAt,
            AuthorDisplayName = MaskUsername(p.User.Username ?? "匿名"),
            AuthorAvatarUrl = p.User.AvatarUrl,
            AttachmentUrls = string.IsNullOrEmpty(p.AttachmentUrls) 
                ? null 
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(p.AttachmentUrls)
        }).ToList();

        return Ok(new { items = dtos, total, page, pageSize });
    }

    /// <summary>
    /// 获取帖子详情
    /// </summary>
    [HttpGet("posts/{postId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostDto>> GetPostById(Guid postId)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == postId && !p.IsDeleted);

        if (post == null)
        {
            return NotFound(new { message = "帖子不存在" });
        }

        var dto = new PostDto
        {
            Id = post.Id,
            PatientSupportGroupId = post.PatientSupportGroupId,
            Title = post.Title,
            Content = post.Content,
            Tag = post.Tag,
            ReadCount = post.ReadCount,
            CommentCount = post.CommentCount,
            LikeCount = post.LikeCount,
            LastReplyAt = post.LastReplyAt,
            IsPinned = post.IsPinned,
            CreatedAt = post.CreatedAt,
            AuthorDisplayName = MaskUsername(post.User.Username ?? "匿名"),
            AuthorAvatarUrl = post.User.AvatarUrl,
            AttachmentUrls = string.IsNullOrEmpty(post.AttachmentUrls) 
                ? null 
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(post.AttachmentUrls)
        };

        return Ok(dto);
    }

    /// <summary>
    /// 创建帖子
    /// </summary>
    [HttpPost("{groupId}/posts")]
    [Authorize]
    [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PostDto>> CreatePost(Guid groupId, [FromBody] CreatePostDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        // 验证用户是否存在
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return BadRequest(new { message = "用户不存在" });
        }

        // 验证患友会是否存在
        var group = await _context.PatientSupportGroups.FindAsync(groupId);
        if (group == null || !group.IsEnabled)
        {
            return NotFound(new { message = "患友会不存在或已禁用" });
        }

        // 序列化附件URL列表
        string? attachmentUrlsJson = null;
        if (dto.AttachmentUrls != null && dto.AttachmentUrls.Any())
        {
            attachmentUrlsJson = System.Text.Json.JsonSerializer.Serialize(dto.AttachmentUrls);
        }

        var post = new Post
        {
            Id = Guid.NewGuid(),
            PatientSupportGroupId = groupId,
            UserId = userId,
            Title = dto.Title,
            Content = dto.Content,
            Tag = dto.Tag ?? "求助",
            AttachmentUrls = attachmentUrlsJson,
            ReadCount = 0,
            CommentCount = 0,
            LikeCount = 0,
            IsPinned = false,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        // 重新加载用户信息
        await _context.Entry(post).Reference(p => p.User).LoadAsync();

        var postDto = new PostDto
        {
            Id = post.Id,
            PatientSupportGroupId = post.PatientSupportGroupId,
            Title = post.Title,
            Content = post.Content,
            Tag = post.Tag,
            ReadCount = post.ReadCount,
            CommentCount = post.CommentCount,
            LikeCount = post.LikeCount,
            LastReplyAt = post.LastReplyAt,
            IsPinned = post.IsPinned,
            CreatedAt = post.CreatedAt,
            AuthorDisplayName = MaskUsername(user.Username ?? "匿名"),
            AuthorAvatarUrl = user.AvatarUrl,
            AttachmentUrls = dto.AttachmentUrls
        };

        return CreatedAtAction(nameof(GetPosts), new { groupId }, postDto);
    }

    /// <summary>
    /// 获取会规
    /// </summary>
    [HttpGet("{groupId}/rules")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetRules(Guid groupId)
    {
        var rules = await _context.GroupRules
            .Include(r => r.PatientSupportGroup)
                .ThenInclude(g => g.Doctor)
                    .ThenInclude(d => d.Department)
            .FirstOrDefaultAsync(r => r.PatientSupportGroupId == groupId);

        if (rules == null)
        {
            return NotFound(new { message = "会规不存在" });
        }

        // 检查患友会是否已启用
        if (!rules.PatientSupportGroup.IsEnabled)
        {
            return NotFound(new { message = "患友会已冻结" });
        }

        // 根据实际内容决定展示：如果为空或仍为默认模板，则优先使用患友会描述
        var defaultRulesContent = GetDefaultRulesContent(rules.PatientSupportGroup.Doctor.Name).Trim();
        var currentContent = (rules.Content ?? string.Empty).Trim();
        var useDescription = string.IsNullOrWhiteSpace(currentContent) ||
                             currentContent.Equals(defaultRulesContent, StringComparison.Ordinal);
        var effectiveContent = useDescription && !string.IsNullOrWhiteSpace(rules.PatientSupportGroup.Description)
            ? rules.PatientSupportGroup.Description
            : rules.Content ?? string.Empty;

        return Ok(new
        {
            rules.Id,
            Content = effectiveContent,
            rules.CreatedAt,
            rules.UpdatedAt,
            GroupName = rules.PatientSupportGroup.Name,
            GroupDescription = rules.PatientSupportGroup.Description,
            DoctorName = rules.PatientSupportGroup.Doctor.Name,
            DoctorTitle = rules.PatientSupportGroup.Doctor.Title,
            DoctorAvatarUrl = rules.PatientSupportGroup.Doctor.AvatarUrl,
            DoctorHospital = rules.PatientSupportGroup.Doctor.Hospital,
            DoctorDepartmentName = rules.PatientSupportGroup.Doctor.Department?.Name
        });
    }

    /// <summary>
    /// 增加帖子阅读数
    /// </summary>
    [HttpPost("posts/{postId}/read")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> IncrementReadCount(Guid postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null || post.IsDeleted)
        {
            return NotFound(new { message = "帖子不存在" });
        }

        post.ReadCount++;
        post.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { readCount = post.ReadCount });
    }

    /// <summary>
    /// 点赞/取消点赞帖子
    /// </summary>
    [HttpPost("posts/{postId}/like")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ToggleLike(Guid postId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var post = await _context.Posts.FindAsync(postId);
        if (post == null || post.IsDeleted)
        {
            return NotFound(new { message = "帖子不存在" });
        }

        var existingLike = await _context.PostLikes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

        if (existingLike != null)
        {
            // 取消点赞
            _context.PostLikes.Remove(existingLike);
            post.LikeCount = Math.Max(0, post.LikeCount - 1);
        }
        else
        {
            // 点赞
            var like = new PostLike
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.PostLikes.Add(like);
            post.LikeCount++;
        }

        post.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { likeCount = post.LikeCount, isLiked = existingLike == null });
    }

    /// <summary>
    /// 获取帖子的评论列表
    /// </summary>
    [HttpGet("posts/{postId}/comments")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetPostComments(Guid postId)
    {
        var comments = await _context.PostComments
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Where(c => c.PostId == postId && !c.IsDeleted && c.ParentCommentId == null)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var dtos = comments.Select(c => new PostCommentDto
        {
            Id = c.Id,
            PostId = c.PostId,
            Content = c.Content,
            AttachmentUrls = string.IsNullOrEmpty(c.AttachmentUrls)
                ? null
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(c.AttachmentUrls),
            ParentCommentId = c.ParentCommentId,
            CreatedAt = c.CreatedAt,
            AuthorDisplayName = MaskUsername(c.User.Username ?? "匿名"),
            AuthorAvatarUrl = c.User.AvatarUrl,
            Replies = c.Replies
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new PostCommentDto
                {
                    Id = r.Id,
                    PostId = r.PostId,
                    Content = r.Content,
                    AttachmentUrls = string.IsNullOrEmpty(r.AttachmentUrls)
                        ? null
                        : System.Text.Json.JsonSerializer.Deserialize<List<string>>(r.AttachmentUrls),
                    ParentCommentId = r.ParentCommentId,
                    CreatedAt = r.CreatedAt,
                    AuthorDisplayName = MaskUsername(r.User.Username ?? "匿名"),
                    AuthorAvatarUrl = r.User.AvatarUrl
                })
                .ToList()
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// 创建评论
    /// </summary>
    [HttpPost("posts/{postId}/comments")]
    [Authorize]
    [ProducesResponseType(typeof(PostCommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PostCommentDto>> CreateComment(Guid postId, [FromBody] CreatePostCommentDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return BadRequest(new { message = "用户不存在" });
        }

        var post = await _context.Posts.FindAsync(postId);
        if (post == null || post.IsDeleted)
        {
            return NotFound(new { message = "帖子不存在" });
        }

        // 如果是指定父评论的回复，验证父评论是否存在
        if (dto.ParentCommentId.HasValue)
        {
            var parentComment = await _context.PostComments.FindAsync(dto.ParentCommentId.Value);
            if (parentComment == null || parentComment.IsDeleted || parentComment.PostId != postId)
            {
                return BadRequest(new { message = "父评论不存在" });
            }
        }

        // 序列化附件URL列表
        string? attachmentUrlsJson = null;
        if (dto.AttachmentUrls != null && dto.AttachmentUrls.Any())
        {
            attachmentUrlsJson = System.Text.Json.JsonSerializer.Serialize(dto.AttachmentUrls);
        }

        var comment = new PostComment
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            UserId = userId,
            Content = dto.Content,
            AttachmentUrls = attachmentUrlsJson,
            ParentCommentId = dto.ParentCommentId,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PostComments.Add(comment);

        // 更新帖子的评论数和最后回复时间
        post.CommentCount++;
        post.LastReplyAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // 重新加载用户信息
        await _context.Entry(comment).Reference(c => c.User).LoadAsync();

        var commentDto = new PostCommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            Content = comment.Content,
            AttachmentUrls = dto.AttachmentUrls,
            ParentCommentId = comment.ParentCommentId,
            CreatedAt = comment.CreatedAt,
            AuthorDisplayName = MaskUsername(user.Username ?? "匿名"),
            AuthorAvatarUrl = user.AvatarUrl
        };

        return CreatedAtAction(nameof(GetPostComments), new { postId }, commentDto);
    }

    /// <summary>
    /// 上传帖子附件（图片/视频）
    /// </summary>
    [HttpPost("posts/upload")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UploadPostAttachment(IFormFile file)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        // 获取用户信息以获取username
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return BadRequest(new { message = "用户不存在" });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "请选择要上传的文件" });
        }

        // 验证文件类型
        var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var allowedVideoExtensions = new[] { ".mp4", ".mov", ".avi", ".webm" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedImageExtensions.Contains(fileExtension) && !allowedVideoExtensions.Contains(fileExtension))
        {
            return BadRequest(new { message = "不支持的文件类型，仅支持图片：jpg, jpeg, png, gif, webp；视频：mp4, mov, avi, webm" });
        }

        // 验证文件大小（图片最大 5MB，视频最大 50MB）
        var isVideo = allowedVideoExtensions.Contains(fileExtension);
        const long maxImageSize = 5 * 1024 * 1024; // 5MB
        const long maxVideoSize = 50 * 1024 * 1024; // 50MB
        
        if (isVideo && file.Length > maxVideoSize)
        {
            return BadRequest(new { message = "视频文件大小不能超过 50MB" });
        }
        if (!isVideo && file.Length > maxImageSize)
        {
            return BadRequest(new { message = "图片文件大小不能超过 5MB" });
        }

        try
        {
            // 清理用户名中的非法字符，确保可以作为文件夹名称
            var safeUsername = string.Join("_", user.Username.Split(Path.GetInvalidFileNameChars()));
            
            // 创建上传目录：uploads/images/Post/{username}/
            var uploadSubFolder = Path.Combine("uploads", "images", "Post", safeUsername);
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, uploadSubFolder);
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 生成唯一文件名
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // 保存文件
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 返回相对URL路径
            var relativeUrl = $"/uploads/images/Post/{safeUsername}/{fileName}";
            
            return Ok(new { url = relativeUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传帖子附件失败");
            return StatusCode(500, new { message = "上传失败，请稍后重试" });
        }
    }

    /// <summary>
    /// 上传评论附件（图片/视频）
    /// </summary>
    [HttpPost("posts/comments/upload")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UploadCommentAttachment(IFormFile file)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "请选择要上传的文件" });
        }

        // 验证文件类型
        var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var allowedVideoExtensions = new[] { ".mp4", ".mov", ".avi", ".webm" };
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedImageExtensions.Contains(fileExtension) && !allowedVideoExtensions.Contains(fileExtension))
        {
            return BadRequest(new { message = "不支持的文件类型，仅支持图片：jpg, jpeg, png, gif, webp；视频：mp4, mov, avi, webm" });
        }

        // 验证文件大小（图片最大 5MB，视频最大 50MB）
        var isVideo = allowedVideoExtensions.Contains(fileExtension);
        const long maxImageSize = 5 * 1024 * 1024; // 5MB
        const long maxVideoSize = 50 * 1024 * 1024; // 50MB
        
        if (isVideo && file.Length > maxVideoSize)
        {
            return BadRequest(new { message = "视频文件大小不能超过 50MB" });
        }
        if (!isVideo && file.Length > maxImageSize)
        {
            return BadRequest(new { message = "图片文件大小不能超过 5MB" });
        }

        try
        {
            // 创建上传目录：uploads/images/postcomments/{userId}/
            var uploadSubFolder = Path.Combine("uploads", "images", "postcomments", userId.ToString());
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, uploadSubFolder);
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 生成唯一文件名
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // 保存文件
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 返回相对URL路径
            var relativeUrl = $"/uploads/images/postcomments/{userId}/{fileName}";
            
            return Ok(new { url = relativeUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传评论附件失败");
            return StatusCode(500, new { message = "上传失败，请稍后重试" });
        }
    }

    // 辅助方法：脱敏用户名
    private string MaskUsername(string username)
    {
        if (string.IsNullOrEmpty(username) || username.Length <= 2)
        {
            return "匿名";
        }

        if (username.Length <= 4)
        {
            return username.Substring(0, 1) + "***";
        }

        return username.Substring(0, 2) + "*******" + username.Substring(username.Length - 1);
    }

    // 获取默认会规内容
    private string GetDefaultRulesContent(string doctorName)
    {
        return $@"{doctorName}医生患友会会规

1、严格按照版块分区要求发帖，乱占用版块发帖将会被删除。严重乱水帖者直接封号处理。没意义且影响阅读性，任由超水帖杂草般在患友会里丛生，不利于患友会整体发展。

2、违反患友会协议，包括但不限于色情帖、暴力帖、辱骂吵架帖、广告帖、毫无节操的猥琐帖，地域攻击帖，各种骗子帖等各种不利于患友会发展的不良信息帖将一律删除，严重者将予以封号处理。双方互怼互骂、恶意滋扰患友会秩序、重复发布攻击帖将进行拉黑处理。

3、咨询看病问题，请购买医生的服务。";
    }
}


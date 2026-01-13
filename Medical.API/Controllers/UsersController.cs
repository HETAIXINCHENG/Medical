using Medical.API.Data;
using Medical.API.Models.DTOs;
using Medical.API.Models.Entities;
using Medical.API.Models.Enums;
using Medical.API.Repositories;
using Medical.API.Services;
using Medical.API.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Medical.API.Controllers;

/// <summary>
/// 用户控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly MedicalDbContext _context;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<UsersController> _logger;
    private readonly IUserRepository _userRepository;  // 注意这里是 IUserRepository 接口
    public UsersController(
        MedicalDbContext context,
        IEncryptionService encryptionService,
        ILogger<UsersController> logger,
        IUserRepository userRepository)
    {
        _context = context;
        _encryptionService = encryptionService;
        _logger = logger;
        _userRepository = userRepository; // 初始化_userRepository字段

    }

    /// <summary>
    /// 获取所有用户列表（管理员）
    /// </summary>
    /// <param name="role">角色筛选</param>
    /// <param name="isActive">是否激活</param>
    /// <param name="keyword">关键词搜索（用户名、邮箱、手机号）</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns>用户列表</returns>
    [HttpGet]
    [RequirePermission("user.view")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAllUsers(
[FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? keyword = null,
        [FromQuery] int? userTypeId = null, // 新增：用户类型ID筛选
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Users.AsQueryable();

        // 如果指定了userTypeId，按用户类型筛选（用于用户管理只显示System类型）
        if (userTypeId.HasValue)
        {
            query = query.Where(u => u.UserTypeId == userTypeId.Value);
        }

        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => u.Role == role);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        // 关键词搜索
        if (!string.IsNullOrEmpty(keyword))
        {
            query = query.Where(u => 
                u.Username.Contains(keyword) ||
                (u.Email != null && u.Email.Contains(keyword)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(keyword)));
        }

        // 获取总数
        var total = await query.CountAsync();

        // 获取分页数据（需要 Include UserRoles 来获取角色信息）
        var users = await query
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 转换为 DTO，从 UserRoles 获取角色的 Code（用于绑定）和 Name（用于显示）
        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            PhoneNumber = u.PhoneNumber,
            Email = u.Email,
            AvatarUrl = u.AvatarUrl ?? null,
            Bio = u.Bio,
            // 从 UserRoles 获取角色的 Code（用于绑定），如果没有则使用 User.Role
            Role = u.UserRoles.Any() 
                ? u.UserRoles.OrderBy(ur => ur.Role.Code).First().Role.Code 
                : u.Role,
            // 添加 RoleName 字段用于显示（前端可以根据 Code 查找 Name，或使用此字段）
            RoleName = u.UserRoles.Any() 
                ? u.UserRoles.OrderBy(ur => ur.Role.Code).First().Role.Name 
                : u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();

        // 返回包含总数和数据的对象
        return Ok(new { items = userDtos, total, page, pageSize });
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns>用户信息</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userIdGuid);
        
        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber)
                ? null
                : user.PhoneNumber,
            Email = string.IsNullOrEmpty(user.Email)
                ? null
                : user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Role = user.UserRoles.Any() 
                ? user.UserRoles.OrderBy(ur => ur.Role.Code).First().Role.Code 
                : user.Role,
            RoleName = user.UserRoles.Any() 
                ? user.UserRoles.OrderBy(ur => ur.Role.Code).First().Role.Name 
                : user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(userDto);
    }

    /// <summary>
    /// 更新当前用户信息
    /// </summary>
    /// <param name="userDto">用户信息</param>
    /// <returns>更新后的用户信息</returns>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> UpdateCurrentUser([FromBody] UserDto userDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userIdGuid);
        
        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        // 更新允许修改的字段
        if (!string.IsNullOrEmpty(userDto.PhoneNumber) && userDto.PhoneNumber != user.PhoneNumber)
        {
            user.PhoneNumber = userDto.PhoneNumber;
        }

        if (!string.IsNullOrEmpty(userDto.Email) && userDto.Email != user.Email)
        {
            user.Email = userDto.Email;
        }

        user.AvatarUrl = userDto.AvatarUrl;
        user.Bio = userDto.Bio;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // 重新加载角色信息
        await _context.Entry(user).Collection(u => u.UserRoles).LoadAsync();
        foreach (var userRole in user.UserRoles)
        {
            await _context.Entry(userRole).Reference(ur => ur.Role).LoadAsync();
        }

        var updatedDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber)
                ? null
                : user.PhoneNumber,
            Email = string.IsNullOrEmpty(user.Email)
                ? null
                : user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Role = user.UserRoles.Any() 
                ? user.UserRoles.OrderBy(ur => ur.Role.Code).First().Role.Code 
                : user.Role,
            RoleName = user.UserRoles.Any() 
                ? user.UserRoles.OrderBy(ur => ur.Role.Code).First().Role.Name 
                : user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(updatedDto);
    }

    /// <summary>
    /// 修改当前用户密码
    /// </summary>
    /// <param name="request">密码修改请求</param>
    /// <returns>操作结果</returns>
    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
        {
            return Unauthorized();
        }

        var user = await _context.Users.FindAsync(userIdGuid);
        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        // 验证原密码
        if (!_encryptionService.VerifyPassword(request.OldPassword, user.PasswordHash))
        {
            return BadRequest(new { message = "原密码错误" });
        }

        // 验证新密码长度
        if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return BadRequest(new { message = "新密码长度至少6个字符" });
        }

        // 更新密码
        user.PasswordHash = _encryptionService.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("用户密码修改成功: UserId={UserId}", userIdGuid);

        return Ok(new { message = "密码修改成功" });
    }

    /// <summary>
    /// 获取当前登录用户的家庭成员列表
    /// </summary>
    /// <returns>家庭成员列表</returns>
    [HttpGet("me/family-members")]
    [ProducesResponseType(typeof(List<FamilyMember>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FamilyMember>>> GetFamilyMembers()
    {
        var currentUserIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserIdentifier) || !Guid.TryParse(currentUserIdentifier, out var currentUserId))
        {
            return Unauthorized();
        }

        // 先获取当前登录用户对应的患者信息
        var currentPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.UserId == currentUserId);
        
        var familyMembers = new List<FamilyMember>();
        if (currentPatient != null)
        {
            familyMembers = await _context.FamilyMembers
                .Where(fm => fm.PatientId == currentPatient.Id)
                .OrderBy(fm => fm.CreatedAt)
                .ToListAsync();
        }

        // 直接返回明文信息（不再解密）

        return Ok(familyMembers);
    }

    /// <summary>
    /// 添加家庭成员
    /// </summary>
    /// <param name="familyMember">家庭成员信息</param>
    /// <returns>创建的家庭成员</returns>
    [HttpPost("me/family-members")]
    [ProducesResponseType(typeof(FamilyMember), StatusCodes.Status201Created)]
    public async Task<ActionResult<FamilyMember>> AddFamilyMember([FromBody] CreateFamilyMemberDto dto)
    {
        try
        {
            // 检查 ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "数据验证失败", errors = errors });
            }

            if (dto == null)
            {
                return BadRequest(new { message = "家庭成员信息不能为空" });
            }

            var currentUserIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserIdentifier) || !Guid.TryParse(currentUserIdentifier, out var currentUserId))
            {
                return Unauthorized();
            }

            // 先获取当前登录用户对应的患者信息
            var currentPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == currentUserId);
            
            if (currentPatient == null)
            {
                return BadRequest(new { message = "请先创建患者信息" });
            }

            // 创建 FamilyMember 实体
            var familyMember = new FamilyMember
            {
                Id = Guid.NewGuid(),
                PatientId = currentPatient.Id,
                Name = dto.Name,
                Relationship = dto.GetRelationshipType(), // 使用 DTO 的方法转换枚举
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                IdCardNumber = dto.IdCardNumber,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 直接保存明文信息（不再加密）

            _context.FamilyMembers.Add(familyMember);
            await _context.SaveChangesAsync();

            // 直接返回明文信息（不再解密）

            return CreatedAtAction(nameof(GetFamilyMembers), new { id = familyMember.Id }, familyMember);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加家庭成员失败");
            return BadRequest(new { message = "添加家庭成员失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取用户信息（管理员）
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户信息</returns>
    [HttpGet("{id}")]
    [RequirePermission("user.view")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
        
        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber)
                ? null
                : user.PhoneNumber,
            Email = string.IsNullOrEmpty(user.Email)
                ? null
                : user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Role = user.UserRoles.Any() 
                ? user.UserRoles.OrderBy(ur => ur.Role.Code).First().Role.Code 
                : user.Role,
            RoleName = user.UserRoles.Any() 
                ? user.UserRoles.OrderBy(ur => ur.Role.Code).First().Role.Name 
                : user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(userDto);
    }

    /// <summary>
    /// 创建用户（管理员）
    /// </summary>
    /// <param name="userDto">用户信息</param>
    /// <returns>创建的用户</returns>
    [HttpPost]
    [RequirePermission("user.create")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto userDto)
    {
        if (string.IsNullOrWhiteSpace(userDto.Username))
        {
            return BadRequest(new { message = "用户名不能为空" });
        }
        
        // 检查用户名是否已存在
        if (await _userRepository.GetByUsernameAsync(userDto.Username) != null)
        {
            return BadRequest(new { message = "用户名已存在" });
        }
        // 检查手机号是否已存在
        if (!string.IsNullOrEmpty(userDto.PhoneNumber) &&
            await _userRepository.GetByPhoneAsync(userDto.PhoneNumber) != null)
        {
            return BadRequest(new { message = "手机号已被注册" });
        }

        // 检查邮箱是否已存在
        if (!string.IsNullOrEmpty(userDto.Email) &&
            await _userRepository.GetByEmailAsync(userDto.Email) != null)
        {
            return BadRequest(new { message = "邮箱已被注册" });
        }

        // 用户管理创建的用户默认为System类型（UserTypeId = 1）
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = userDto.Username,
            PasswordHash = string.IsNullOrEmpty(userDto.Password)
                ? _encryptionService.HashPassword("123456") // 默认密码
                : _encryptionService.HashPassword(userDto.Password),
            PhoneNumber = userDto.PhoneNumber,
            Email = userDto.Email,
            Role = userDto.Role ?? "2", // 默认使用 Admin 的 Code "2"
            UserTypeId = 1, // 用户管理创建的用户默认为System类型（1=System）
            IsActive = userDto.IsActive ?? true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 根据 role（Code）查找角色并创建 UserRole 关联
        var roleCode = userDto.Role ?? "2"; // 默认 Admin
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Code == roleCode);
        if (role != null)
        {
            // 先添加 UserRole 到导航属性（在保存 User 之前）
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        // 使用 AddAsync 会同时保存 User 和 UserRole（因为 UserRole 在导航属性中）
        await _userRepository.AddAsync(user);

        // 重新加载以获取角色信息
        await _context.Entry(user).Collection(u => u.UserRoles).LoadAsync();
        foreach (var userRole in user.UserRoles)
        {
            await _context.Entry(userRole).Reference(ur => ur.Role).LoadAsync();
        }

        var responseDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber)
                ? null
                : user.PhoneNumber,
            Email = string.IsNullOrEmpty(user.Email)
                ? null
                : user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Role = user.UserRoles.Any() 
                ? user.UserRoles.OrderBy(ur => ur.Role.Code).First().Role.Code 
                : user.Role,
            RoleName = user.UserRoles.Any() 
                ? user.UserRoles.OrderBy(ur => ur.Role.Code).First().Role.Name 
                : user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, responseDto);
    }

    /// <summary>
    /// 更新用户信息（管理员）
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="userDto">用户信息</param>
    /// <returns>更新后的用户信息</returns>
    [HttpPut("{id}")]
    [RequirePermission("user.update")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto userDto)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
        
        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        // 更新用户名（如果提供）
        if (!string.IsNullOrWhiteSpace(userDto.Username))
        {
            // 检查新用户名是否已被使用
            if (userDto.Username != user.Username)
            {
                var existingUser = await _userRepository.GetByUsernameAsync(userDto.Username);
                if (existingUser != null && existingUser.Id != id)
                {
                    return BadRequest(new { message = "用户名已被其他用户使用" });
                }
            }
            user.Username = userDto.Username;
        }

        // 更新密码（如果提供且不为空）
        if (!string.IsNullOrEmpty(userDto.Password))
        {
            user.PasswordHash = _encryptionService.HashPassword(userDto.Password);
        }

        // 更新手机号（如果提供）
        if (userDto.PhoneNumber != null)
        {
            if (userDto.PhoneNumber != user.PhoneNumber)
            {
                // 检查新手机号是否已被使用
                var existingUser = await _userRepository.GetByPhoneAsync(userDto.PhoneNumber);
                if (existingUser != null && existingUser.Id != id)
                {
                    return BadRequest(new { message = "手机号已被其他用户使用" });
                }
            }
            user.PhoneNumber = userDto.PhoneNumber;
        }

        // 更新邮箱（如果提供）
        if (userDto.Email != null)
        {
            if (userDto.Email != user.Email)
            {
                // 检查新邮箱是否已被使用
                var existingUser = await _userRepository.GetByEmailAsync(userDto.Email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return BadRequest(new { message = "邮箱已被其他用户使用" });
                }
            }
            user.Email = userDto.Email;
        }

        // 更新角色（通过 UserRoles 关联表）
        if (userDto.Role != null)
        {
            // 查询现有的 UserRoles（使用独立的查询，避免跟踪冲突）
            var existingUserRoles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .ToListAsync();
            
            // 移除所有现有角色关联
            if (existingUserRoles.Any())
            {
                _context.UserRoles.RemoveRange(existingUserRoles);
                // 先保存删除操作
                await _context.SaveChangesAsync();
            }
            
            // 根据 role（Code）查找角色并创建新的 UserRole 关联
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Code == userDto.Role);
            if (role != null)
            {
                var newUserRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserRoles.Add(newUserRole);
            }
            
            // 同时更新 User.Role 字段（兼容字段）
            user.Role = userDto.Role;
        }

        if (userDto.IsActive.HasValue)
        {
            user.IsActive = userDto.IsActive.Value;
        }

        // 更新头像URL：如果前端明确发送了值（包括null），则更新；否则保持原值
        // 注意：前端发送 null 或空字符串时，应该清空头像
        if (userDto.AvatarUrl != null)
        {
            // 如果前端发送的是空字符串，清空头像
            user.AvatarUrl = string.IsNullOrWhiteSpace(userDto.AvatarUrl) ? null : userDto.AvatarUrl;
        }
        // 如果 userDto.AvatarUrl 是 null，说明前端没有发送这个字段，保持原值不变
        
        // 更新Bio：如果前端明确发送了值（包括null），则更新；否则保持原值
        if (userDto.Bio != null)
        {
            user.Bio = string.IsNullOrWhiteSpace(userDto.Bio) ? null : userDto.Bio;
        }
        
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var updatedDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            PhoneNumber = string.IsNullOrEmpty(user.PhoneNumber)
                ? null
                : user.PhoneNumber,
            Email = string.IsNullOrEmpty(user.Email)
                ? null
                : user.Email,
            AvatarUrl = user.AvatarUrl,
            Bio = user.Bio,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(updatedDto);
    }

    /// <summary>
    /// 删除用户（管理员）
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [RequirePermission("user.delete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        // 检查是否有关联的患者记录，如果有则先删除
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.UserId == id);
        
        if (patient != null)
        {
            // 先删除关联的患者记录（这会级联删除家庭成员等）
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
        }

        // 获取用户创建的帖子ID列表（用于后续判断）
        var userPostIds = await _context.Posts
            .Where(p => p.UserId == id)
            .Select(p => p.Id)
            .ToListAsync();

        // 删除用户关联的帖子记录（这会级联删除这些帖子下的 PostComments 和 PostLikes）
        if (userPostIds.Any())
        {
            var posts = await _context.Posts
                .Where(p => p.UserId == id)
                .ToListAsync();
            _context.Posts.RemoveRange(posts);
            await _context.SaveChangesAsync();
        }

        // 删除用户在其他帖子下的评论（不属于该用户帖子的评论）
        var otherPostComments = await _context.PostComments
            .Where(pc => pc.UserId == id && !userPostIds.Contains(pc.PostId))
            .ToListAsync();
        if (otherPostComments.Any())
        {
            _context.PostComments.RemoveRange(otherPostComments);
            await _context.SaveChangesAsync();
        }

        // 删除用户在其他帖子下的点赞（不属于该用户帖子的点赞）
        var otherPostLikes = await _context.PostLikes
            .Where(pl => pl.UserId == id && !userPostIds.Contains(pl.PostId))
            .ToListAsync();
        if (otherPostLikes.Any())
        {
            _context.PostLikes.RemoveRange(otherPostLikes);
            await _context.SaveChangesAsync();
        }

        // 然后删除用户记录（UserRoles 会自动级联删除）
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("用户删除成功: Id={Id}, Username={Username}", id, user.Username);

        return NoContent();
    }

    /// <summary>
    /// 为用户分配角色
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="dto">角色ID列表</param>
    /// <returns>分配结果</returns>
    [HttpPost("{id}/roles")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AssignRoles(
        Guid id,
        [FromBody] AssignRolesDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        // 删除现有角色关联
        var existingRoles = await _context.UserRoles
            .Where(ur => ur.UserId == id)
            .ToListAsync();
        _context.UserRoles.RemoveRange(existingRoles);

        // 添加新角色关联
        if (dto.RoleIds != null && dto.RoleIds.Count > 0)
        {
            var roles = await _context.Roles
                .Where(r => dto.RoleIds.Contains(r.Id))
                .ToListAsync();

            foreach (var role in roles)
            {
                var userRole = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = id,
                    RoleId = role.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserRoles.Add(userRole);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("用户角色分配成功: UserId={UserId}, RoleCount={Count}", 
            id, dto.RoleIds?.Count ?? 0);

        return Ok(new { message = "角色分配成功" });
    }

    /// <summary>
    /// 获取用户的角色列表
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>角色列表</returns>
    [HttpGet("{id}/roles")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetUserRoles(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        var roles = await _context.UserRoles
            .Where(ur => ur.UserId == id)
            .Include(ur => ur.Role)
            .Select(ur => new
            {
                ur.Role.Id,
                ur.Role.Name,
                ur.Role.Code,
                ur.Role.Description
            })
            .ToListAsync();

        return Ok(roles);
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="dto">新密码</param>
    /// <returns>重置结果</returns>
    [HttpPost("{id}/reset-password")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ResetPassword(
        Guid id,
        [FromBody] ResetPasswordDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "用户不存在" });
        }

        if (string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return BadRequest(new { message = "新密码不能为空" });
        }

        if (dto.NewPassword.Length < 6)
        {
            return BadRequest(new { message = "密码长度至少6个字符" });
        }

        user.PasswordHash = _encryptionService.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("用户密码重置成功: UserId={UserId}", id);

        return Ok(new { message = "密码重置成功" });
    }
}


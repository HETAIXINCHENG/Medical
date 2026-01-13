using Microsoft.AspNetCore.Mvc;
using Medical.API.Models.DTOs;
using Medical.API.Services;
using Medical.API.Repositories;
using Medical.API.Models.Entities;
using Medical.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Controllers;

/// <summary>
/// 管理员认证控制器（后台管理系统专用）
/// 只允许 UserTypeId = 1 (System类型) 且 Role 为 Admin 或 SuperAdmin 的用户登录
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdminAuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AdminAuthController> _logger;
    private readonly MedicalDbContext _context;

    public AdminAuthController(
        IUserRepository userRepository,
        IEncryptionService encryptionService,
        IJwtService jwtService,
        ILogger<AdminAuthController> logger,
        MedicalDbContext context)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _jwtService = jwtService;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// 管理员登录（只允许 UserTypeId = 1 且通过 UserRoles 关联表查询到的角色为 Admin 或 SuperAdmin 的用户）
    /// </summary>
    /// <param name="request">登录信息</param>
    /// <returns>登录结果</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] AdminLoginRequestDto request)
    {
        try
        {
            // 查询 User 表，只允许 UserTypeId = 1 (System类型) 的用户
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.UserTypeId == 1);

            if (user == null || !_encryptionService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "用户名或密码错误" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "账户已被禁用" });
            }

            // 通过 UserRoles 关联表查询用户的角色
            var userRoleCodes = user.UserRoles
                .Select(ur => ur.Role.Code)
                .ToList();
            
            var userRoleNames = user.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList();

            // 检查用户是否有 Admin 或 SuperAdmin 角色
            // 根据 roles 表：SuperAdmin Code=1, Admin Code=2
            // Code 是字符串类型，但可能存储为 "1", "2" 或 "SuperAdmin", "Admin"
            bool hasAdminRole = userRoleCodes.Contains("1") || userRoleCodes.Contains("2") ||
                               userRoleCodes.Contains("SuperAdmin") || userRoleCodes.Contains("Admin") ||
                               userRoleNames.Contains("SuperAdmin") || userRoleNames.Contains("Admin");

            // 也检查 User.Role 字段（兼容旧数据，可能是字符串 "Admin" 或 "SuperAdmin"，也可能是数字字符串 "1" 或 "2"）
            bool hasAdminRoleInUserField = user.Role == "Admin" || user.Role == "SuperAdmin" ||
                                          user.Role == "1" || user.Role == "2";

            if (!hasAdminRole && !hasAdminRoleInUserField)
            {
                return Unauthorized(new { message = "该用户没有管理员权限" });
            }

            // 获取用户的主要角色（用于Token生成）
            // 优先使用 UserRoles 关联表中的角色，如果没有则使用 User.Role 字段
            string userRoleForToken = "Admin"; // 默认值
            if (userRoleNames.Any() || userRoleCodes.Any())
            {
                // 优先使用 SuperAdmin，然后是 Admin
                if (userRoleNames.Contains("SuperAdmin") || userRoleCodes.Contains("1") || userRoleCodes.Contains("SuperAdmin"))
                {
                    userRoleForToken = "SuperAdmin";
                }
                else if (userRoleNames.Contains("Admin") || userRoleCodes.Contains("2") || userRoleCodes.Contains("Admin"))
                {
                    userRoleForToken = "Admin";
                }
                else if (userRoleNames.Any())
                {
                    // 使用第一个角色的名称
                    userRoleForToken = userRoleNames.First();
                }
                else
                {
                    userRoleForToken = "Admin";
                }
            }
            else if (!string.IsNullOrEmpty(user.Role))
            {
                // 如果 UserRoles 为空，使用 User.Role 字段
                if (user.Role == "1" || user.Role == "SuperAdmin")
                {
                    userRoleForToken = "SuperAdmin";
                }
                else if (user.Role == "2" || user.Role == "Admin")
                {
                    userRoleForToken = "Admin";
                }
                else
                {
                    userRoleForToken = user.Role;
                }
            }

            // 更新最后登录时间
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // 临时设置 user.Role 为查询到的角色名称，以便 GenerateToken 使用
            var originalRole = user.Role;
            user.Role = userRoleForToken;

            // 生成Token（使用查询到的角色）
            var token = _jwtService.GenerateToken(user);

            // 恢复原始 Role 值（避免影响实体状态）
            user.Role = originalRole;

            var response = new AuthResponseDto
            {
                Token = token,
                User = new UserDto
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
                    Role = userRoleForToken, // 使用查询到的角色名称
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "管理员登录失败");
            return StatusCode(500, new { message = "登录失败，请稍后重试" });
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using Medical.API.Models.DTOs;
using Medical.API.Services;
using Medical.API.Repositories;
using Medical.API.Models.Entities;

namespace Medical.API.Controllers;

/// <summary>
/// 认证控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IEncryptionService encryptionService,
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    /// <param name="request">注册信息</param>
    /// <returns>注册结果</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            // 检查用户名是否已存在
            if (await _userRepository.GetByUsernameAsync(request.Username) != null)
            {
                return BadRequest(new { message = "用户名已存在" });
            }

            // 检查手机号是否已存在
            if (!string.IsNullOrEmpty(request.PhoneNumber) && 
                await _userRepository.GetByPhoneAsync(request.PhoneNumber) != null)
            {
                return BadRequest(new { message = "手机号已被注册" });
            }

            // 检查邮箱是否已存在
            if (!string.IsNullOrEmpty(request.Email) && 
                await _userRepository.GetByEmailAsync(request.Email) != null)
            {
                return BadRequest(new { message = "邮箱已被注册" });
            }

            // 创建用户，根据Role设置UserTypeId
            // 1=System, 2=Doctor, 3=Patient
            int userTypeId = request.Role switch
            {
                "Doctor" => 2,
                "Patient" => 3,
                _ => 3 // 默认为Patient
            };
            
            var user = new User
            {
                Username = request.Username,
                PasswordHash = _encryptionService.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Role = request.Role,
                UserTypeId = userTypeId
            };

            await _userRepository.AddAsync(user);

            // 生成Token
            var token = _jwtService.GenerateToken(user);

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
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注册失败");
            return StatusCode(500, new { message = "注册失败，请稍后重试" });
        }
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">登录信息</param>
    /// <returns>登录结果</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null || !_encryptionService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "用户名或密码错误" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "账户已被禁用" });
            }

            // 生成Token
            var token = _jwtService.GenerateToken(user);

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
                    Role = user.Role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登录失败");
            return StatusCode(500, new { message = "登录失败，请稍后重试" });
        }
    }
}


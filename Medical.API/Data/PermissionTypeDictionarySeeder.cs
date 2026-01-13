using Medical.API.Data;
using Medical.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Data;

/// <summary>
/// 权限类型字典数据初始化
/// </summary>
public class PermissionTypeDictionarySeeder
{
    public static async Task SeedAsync(MedicalDbContext context)
    {
        // 检查是否已有数据
        if (await context.PermissionTypeDictionaries.AnyAsync())
        {
            return; // 如果已有数据，不再插入
        }

        var permissionTypes = new List<PermissionTypeDictionary>
        {
            new PermissionTypeDictionary
            {
                Id = Guid.NewGuid(),
                Name = "查看",
                Code = "view",
                Description = "查看权限，允许用户查看数据",
                SortOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PermissionTypeDictionary
            {
                Id = Guid.NewGuid(),
                Name = "搜索",
                Code = "search",
                Description = "搜索权限，允许用户搜索数据",
                SortOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PermissionTypeDictionary
            {
                Id = Guid.NewGuid(),
                Name = "新建",
                Code = "create",
                Description = "新建权限，允许用户创建新数据",
                SortOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PermissionTypeDictionary
            {
                Id = Guid.NewGuid(),
                Name = "编辑",
                Code = "update",
                Description = "编辑权限，允许用户修改数据",
                SortOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PermissionTypeDictionary
            {
                Id = Guid.NewGuid(),
                Name = "删除",
                Code = "delete",
                Description = "删除权限，允许用户删除数据",
                SortOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PermissionTypeDictionary
            {
                Id = Guid.NewGuid(),
                Name = "导出",
                Code = "export",
                Description = "导出权限，允许用户导出数据",
                SortOrder = 6,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new PermissionTypeDictionary
            {
                Id = Guid.NewGuid(),
                Name = "导入",
                Code = "import",
                Description = "导入权限，允许用户导入数据",
                SortOrder = 7,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.PermissionTypeDictionaries.AddRange(permissionTypes);
        await context.SaveChangesAsync();
    }
}


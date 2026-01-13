using Medical.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Data;

/// <summary>
/// 权限数据种子（基于菜单和资源配置生成所有权限）
/// </summary>
public static class PermissionSeeder
{
    /// <summary>
    /// 初始化所有权限数据
    /// </summary>
    public static async Task SeedAsync(MedicalDbContext context)
    {
        var permissionTypes = new[] { "view", "create", "update", "delete" };
        
        // 获取已存在的权限代码，用于增量添加与纠正
        var existingPermissions = await context.Permissions.ToListAsync();
        var existingPermissionCodes = existingPermissions.Select(p => p.Code).ToList();
        var existingPermissionDict = existingPermissions
            .GroupBy(p => p.Code)
            .ToDictionary(g => g.Key, g => g.First());
        
        var permissionsToAdd = new List<Permission>();
        var permissionsToUpdate = new List<Permission>();

        // 兼容旧权限的中文名称映射（已有英文名称的权限将被纠正为中文）
        var legacyNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["dashboard"] = "首页",
            ["department-info"] = "科室信息",
            ["doctor-info"] = "医生信息",
            ["health-knowledge"] = "健康知识",
            ["permission-config"] = "权限配置",
            ["system-users"] = "用户管理",
            ["patient-support-groups"] = "医生的患友会",
            ["users"] = "用户管理"
        };

        // 从 resourceConfig 中提取的资源列表（基于实际配置）
        var resources = new[]
        {
            new { Key = "users", Name = "用户管理", BasePath = "/api/users", MenuUrl = "/resources/system-users" },
            new { Key = "patients", Name = "患者信息", BasePath = "/api/users", MenuUrl = "/resources/patients" },
            new { Key = "patient-info", Name = "患者信息库", BasePath = "/api/patients", MenuUrl = "/resources/patient-info" },
            new { Key = "doctors", Name = "医生信息", BasePath = "/api/doctors", MenuUrl = "/resources/doctors" },
            new { Key = "patient-support-groups", Name = "医生的患友会", BasePath = "/api/patientsupportgroups", MenuUrl = "/resources/patient-support-groups" },
            new { Key = "departments", Name = "科室信息", BasePath = "/api/departments", MenuUrl = "/resources/departments" },
            new { Key = "disease-categories", Name = "疾病分类", BasePath = "/api/diseasecategories", MenuUrl = "/resources/disease-categories" },
            new { Key = "activities", Name = "活动管理", BasePath = "/api/activities", MenuUrl = "/resources/activities" },
            new { Key = "knowledge", Name = "健康知识", BasePath = "/api/healthknowledge", MenuUrl = "/resources/knowledge" },
            new { Key = "questions", Name = "提问管理", BasePath = "/api/questions", MenuUrl = "/resources/questions" },
            new { Key = "consultations", Name = "咨询管理", BasePath = "/api/consultations", MenuUrl = "/resources/consultations" },
            new { Key = "visit-records", Name = "就诊记录", BasePath = "/api/visitrecords", MenuUrl = "/resources/visit-records" },
            new { Key = "examination-reports", Name = "检查报告", BasePath = "/api/examinationreports", MenuUrl = "/resources/examination-reports" },
            new { Key = "prescriptions", Name = "处方生成", BasePath = "/api/prescriptions", MenuUrl = "/resources/prescriptions" },
            new { Key = "my-prescriptions", Name = "我的处方", BasePath = "/api/prescriptions", MenuUrl = "/resources/my-prescriptions" },
            new { Key = "drug-categories", Name = "药品分类", BasePath = "/api/drugcategories", MenuUrl = "/resources/drug-categories" },
            new { Key = "drugs", Name = "药品信息", BasePath = "/api/drugs", MenuUrl = "/resources/drugs" },
            new { Key = "drug-inventories", Name = "药品库存", BasePath = "/api/druginventories", MenuUrl = "/resources/drug-inventories" },
            new { Key = "drug-stock-ins", Name = "药品入库", BasePath = "/api/drugstockins", MenuUrl = "/resources/drug-stock-ins" },
            new { Key = "system-roles", Name = "角色管理", BasePath = "/api/roles", MenuUrl = "/resources/system-roles" },
            new { Key = "system-permissions", Name = "权限管理", BasePath = "/api/permissions", MenuUrl = "/resources/system-permissions" },
            new { Key = "posts", Name = "发帖信息", BasePath = "/api/posts", MenuUrl = "/resources/posts" },
            new { Key = "post-comments", Name = "评论信息", BasePath = "/api/postcomments", MenuUrl = "/resources/post-comments" },
            new { Key = "post-likes", Name = "点赞信息", BasePath = "/api/postlikes", MenuUrl = "/resources/post-likes" },
            new { Key = "feedbacks", Name = "反馈投诉", BasePath = "/api/feedbacks", MenuUrl = "/resources/feedbacks" },
            // 商城相关
            new { Key = "product-categories", Name = "商品分类", BasePath = "/api/productcategories", MenuUrl = "/resources/product-categories" },
            new { Key = "products", Name = "商品信息", BasePath = "/api/products", MenuUrl = "/resources/products" },
            new { Key = "product-specs", Name = "商品规格", BasePath = "/api/productspecs", MenuUrl = "/resources/product-specs" },
            new { Key = "user-addresses", Name = "收货地址", BasePath = "/api/useraddresses", MenuUrl = "/resources/user-addresses" },
            new { Key = "carts", Name = "购物车", BasePath = "/api/carts", MenuUrl = "/resources/carts" },
            new { Key = "orders", Name = "订单信息", BasePath = "/api/orders", MenuUrl = "/resources/orders" },
            new { Key = "order-items", Name = "订单明细", BasePath = "/api/orderitems", MenuUrl = "/resources/order-items" },
            new { Key = "payments", Name = "支付记录", BasePath = "/api/payments", MenuUrl = "/resources/payments" },
            new { Key = "ship-companies", Name = "物流公司", BasePath = "/api/shipcompanies", MenuUrl = "/resources/ship-companies" },
            new { Key = "tertiary-hospitals", Name = "三甲医院", BasePath = "/api/tertiaryhospitals", MenuUrl = "/resources/tertiary-hospitals" },
            new { Key = "shipments", Name = "物流信息", BasePath = "/api/shipments", MenuUrl = "/resources/shipments" },
            new { Key = "shipment-tracks", Name = "物流轨迹", BasePath = "/api/shipmenttracks", MenuUrl = "/resources/shipment-tracks" },
            new { Key = "refunds", Name = "退款记录", BasePath = "/api/refunds", MenuUrl = "/resources/refunds" },
            // 财务管理
            new { Key = "financial-receivables", Name = "应收管理", BasePath = "/api/financialreceivables", MenuUrl = "/resources/financial-receivables" },
            new { Key = "financial-payables", Name = "应付管理", BasePath = "/api/financialpayables", MenuUrl = "/resources/financial-payables" },
            new { Key = "financial-fees", Name = "费用管理", BasePath = "/api/financialfees", MenuUrl = "/resources/financial-fees" },
            new { Key = "financial-settlements", Name = "结算管理", BasePath = "/api/financialsettlements", MenuUrl = "/resources/financial-settlements" }
        };

        // 菜单权限（特殊页面）
        var menuPages = new[]
        {
            new { Key = "dashboard", Name = "首页", MenuUrl = "/dashboard" },
            new { Key = "permission-config", Name = "权限配置", MenuUrl = "/permission-config" },
            new { Key = "change-password", Name = "修改密码", MenuUrl = "/change-password" },
            new { Key = "province-city", Name = "省市信息", MenuUrl = "/province-city" }
        };

        int sortOrder = 1;

        // 为每个资源创建 view, create, update, delete 权限
        foreach (var resource in resources)
        {
            foreach (var permissionType in permissionTypes)
            {
                var permissionName = GetPermissionName(resource.Name, permissionType);
                var permissionCode = $"{resource.Key}.{permissionType}";
                var permissionDescription = $"{resource.Name}的{GetPermissionTypeName(permissionType)}权限";

                // 如果权限已存在，更新名称/描述/菜单URL/类型，保持中文显示
                if (existingPermissionDict.TryGetValue(permissionCode, out var existing))
                {
                    bool changed = false;
                    if (!string.Equals(existing.Name, permissionName, StringComparison.Ordinal))
                    {
                        existing.Name = permissionName;
                        changed = true;
                    }
                    if (!string.Equals(existing.Description, permissionDescription, StringComparison.Ordinal))
                    {
                        existing.Description = permissionDescription;
                        changed = true;
                    }
                    if (!string.Equals(existing.MenuUrl, resource.MenuUrl, StringComparison.Ordinal))
                    {
                        existing.MenuUrl = resource.MenuUrl;
                        changed = true;
                    }
                    if (!string.Equals(existing.PermissionType, permissionType, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.PermissionType = permissionType;
                        changed = true;
                    }
                    if (existing.SortOrder != sortOrder)
                    {
                        existing.SortOrder = sortOrder;
                        changed = true;
                    }
                    if (changed)
                    {
                        existing.UpdatedAt = DateTime.UtcNow;
                        permissionsToUpdate.Add(existing);
                    }
                    sortOrder++;
                    continue; // 已存在仅更新
                }

                permissionsToAdd.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = permissionName,
                    Code = permissionCode,
                    Description = permissionDescription,
                    MenuUrl = resource.MenuUrl,
                    PermissionType = permissionType,
                    SortOrder = sortOrder++,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // 为菜单页面创建 view 权限
        foreach (var menuPage in menuPages)
        {
            var permissionName = $"{menuPage.Name} - 查看";
            var permissionCode = $"{menuPage.Key}.view";
            var permissionDescription = $"{menuPage.Name}的查看权限";

            if (existingPermissionDict.TryGetValue(permissionCode, out var existing))
            {
                bool changed = false;
                if (!string.Equals(existing.Name, permissionName, StringComparison.Ordinal))
                {
                    existing.Name = permissionName;
                    changed = true;
                }
                if (!string.Equals(existing.Description, permissionDescription, StringComparison.Ordinal))
                {
                    existing.Description = permissionDescription;
                    changed = true;
                }
                if (!string.Equals(existing.MenuUrl, menuPage.MenuUrl, StringComparison.Ordinal))
                {
                    existing.MenuUrl = menuPage.MenuUrl;
                    changed = true;
                }
                if (!string.Equals(existing.PermissionType, "view", StringComparison.OrdinalIgnoreCase))
                {
                    existing.PermissionType = "view";
                    changed = true;
                }
                if (existing.SortOrder != sortOrder)
                {
                    existing.SortOrder = sortOrder;
                    changed = true;
                }
                if (changed)
                {
                    existing.UpdatedAt = DateTime.UtcNow;
                    permissionsToUpdate.Add(existing);
                }
                sortOrder++;
                continue;
            }

            permissionsToAdd.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Name = permissionName,
                Code = permissionCode,
                Description = permissionDescription,
                MenuUrl = menuPage.MenuUrl,
                PermissionType = "view",
                SortOrder = sortOrder++,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // 上传权限
        if (existingPermissionDict.TryGetValue("upload.create", out var uploadPermission))
        {
            bool changed = false;
            if (!string.Equals(uploadPermission.Name, "文件上传 - 创建", StringComparison.Ordinal))
            {
                uploadPermission.Name = "文件上传 - 创建";
                changed = true;
            }
            if (!string.Equals(uploadPermission.Description, "文件上传权限", StringComparison.Ordinal))
            {
                uploadPermission.Description = "文件上传权限";
                changed = true;
            }
            if (!string.Equals(uploadPermission.PermissionType, "create", StringComparison.OrdinalIgnoreCase))
            {
                uploadPermission.PermissionType = "create";
                changed = true;
            }
            if (uploadPermission.SortOrder != sortOrder)
            {
                uploadPermission.SortOrder = sortOrder;
                changed = true;
            }
            if (changed)
            {
                uploadPermission.UpdatedAt = DateTime.UtcNow;
                permissionsToUpdate.Add(uploadPermission);
            }
        }
        else
        {
            permissionsToAdd.Add(new Permission
            {
                Id = Guid.NewGuid(),
                Name = "文件上传 - 创建",
                Code = "upload.create",
                Description = "文件上传权限",
                MenuUrl = null,
                PermissionType = "create",
                SortOrder = sortOrder++,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // 有新增或更新时保存
        if (permissionsToAdd.Count > 0)
        {
            context.Permissions.AddRange(permissionsToAdd);
        }

        if (permissionsToUpdate.Count > 0)
        {
            context.Permissions.UpdateRange(permissionsToUpdate.Distinct());
        }

        if (permissionsToAdd.Count > 0 || permissionsToUpdate.Count > 0)
        {
            await context.SaveChangesAsync();
        }

        // 第二遍：修正历史遗留的英文名称权限为中文显示
        foreach (var perm in existingPermissions)
        {
            if (string.IsNullOrWhiteSpace(perm.Code)) continue;
            var parts = perm.Code.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            var resourceKey = parts[0];
            var permissionType = parts[1];

            if (!legacyNameMap.TryGetValue(resourceKey, out var resourceName))
            {
                continue;
            }

            var permissionTypeName = GetPermissionTypeName(permissionType);
            var newName = $"{resourceName} - {permissionTypeName}";
            var newDescription = $"{resourceName}的{permissionTypeName}权限";

            bool changed = false;
            if (!string.Equals(perm.Name, newName, StringComparison.Ordinal))
            {
                perm.Name = newName;
                changed = true;
            }
            if (!string.Equals(perm.Description, newDescription, StringComparison.Ordinal))
            {
                perm.Description = newDescription;
                changed = true;
            }
            if (changed)
            {
                perm.UpdatedAt = DateTime.UtcNow;
                permissionsToUpdate.Add(perm);
            }
        }

        if (permissionsToUpdate.Count > 0)
        {
            context.Permissions.UpdateRange(permissionsToUpdate.Distinct());
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 获取权限名称
    /// </summary>
    private static string GetPermissionName(string resourceName, string permissionType)
    {
        var typeName = GetPermissionTypeName(permissionType);
        return $"{resourceName} - {typeName}";
    }

    /// <summary>
    /// 获取权限类型中文名称
    /// </summary>
    private static string GetPermissionTypeName(string permissionType)
    {
        return permissionType switch
        {
            "view" => "查看",
            "create" => "新建",
            "update" => "编辑",
            "delete" => "删除",
            _ => permissionType
        };
    }
}


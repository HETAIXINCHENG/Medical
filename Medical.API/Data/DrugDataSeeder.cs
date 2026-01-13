using Medical.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Data;

/// <summary>
/// 药品数据种子（Drugs、DrugInventories、DrugStockInHeads、DrugStockInLines）
/// </summary>
public static class DrugDataSeeder
{
    /// <summary>
    /// 初始化药品相关数据
    /// </summary>
    public static async Task SeedAsync(MedicalDbContext context)
    {
        // 检查是否已有数据
        if (await context.Drugs.AnyAsync())
        {
            return; // 已有数据，不重复插入
        }

        // 获取所有药品分类（只获取叶子节点，即没有子分类的分类）
        var allCategories = await context.DrugCategories
            .Where(c => c.IsActive)
            .ToListAsync();

        // 获取所有有子分类的父分类
        var parentCategoryIds = await context.DrugCategories
            .Where(c => c.ParentId != null)
            .Select(c => c.ParentId!.Value)
            .Distinct()
            .ToListAsync();

        // 只使用叶子节点分类（没有子分类的分类）来创建药品
        var leafCategories = allCategories
            .Where(c => !parentCategoryIds.Contains(c.Id))
            .ToList();

        if (leafCategories.Count == 0)
        {
            return; // 没有可用的分类
        }

        // 获取 admin 用户作为操作员
        var adminUser = await context.Users
            .FirstOrDefaultAsync(u => u.Username == "admin");

        if (adminUser == null)
        {
            return; // 没有 admin 用户，无法创建入库单
        }

        var drugs = new List<Drug>();
        var inventories = new List<DrugInventory>();
        var stockInHeads = new List<DrugStockInHead>();
        var stockInLines = new List<DrugStockInLine>();

        // 为每个分类创建 2-3 个示例药品
        var random = new Random();
        int invoiceCounter = 1;
        var usedApprovalNumbers = new HashSet<string>(); // 用于确保批准文号唯一

        foreach (var category in leafCategories)
        {
            // 根据分类名称创建相应的示例药品
            var categoryDrugs = GenerateDrugsForCategory(category, random, usedApprovalNumbers);
            drugs.AddRange(categoryDrugs);

            // 为每个药品创建库存和入库记录
            foreach (var drug in categoryDrugs)
            {
                // 创建库存记录
                var inventory = new DrugInventory
                {
                    Id = Guid.NewGuid(),
                    DrugId = drug.Id,
                    WarehouseLocation = "主仓库",
                    CurrentQuantity = random.Next(50, 500), // 随机库存 50-500
                    LastUpdatedAt = DateTime.UtcNow
                };
                inventories.Add(inventory);

                // 创建入库单（每个药品创建一个入库单）
                var stockInHead = new DrugStockInHead
                {
                    Id = Guid.NewGuid(),
                    InvoiceNo = $"RK{DateTime.UtcNow:yyyyMMdd}{invoiceCounter:D4}",
                    SupplierName = GetRandomSupplier(random),
                    OperatorId = adminUser.Id,
                    OperationTime = DateTime.UtcNow.AddDays(-random.Next(1, 30)), // 过去30天内的随机日期
                    TotalAmount = 0, // 将在行项目中计算
                    Status = 1, // 已入库
                    Remarks = $"初始入库 - {drug.CommonName}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                stockInHeads.Add(stockInHead);

                // 创建入库单行
                var purchasePrice = (decimal)(random.Next(10, 200) + random.NextDouble()); // 10-200元
                var quantity = inventory.CurrentQuantity;
                var subtotal = purchasePrice * quantity;

                var stockInLine = new DrugStockInLine
                {
                    Id = Guid.NewGuid(),
                    HeadId = stockInHead.Id,
                    DrugId = drug.Id,
                    BatchNumber = $"BATCH{DateTime.UtcNow:yyyyMMdd}{random.Next(1000, 9999)}",
                    ProductionDate = DateTime.UtcNow.AddMonths(-random.Next(6, 24)), // 6-24个月前生产
                    ExpiryDate = DateTime.UtcNow.AddMonths(random.Next(12, 36)), // 12-36个月后过期
                    Quantity = quantity,
                    PurchasePrice = purchasePrice,
                    Subtotal = subtotal,
                    WarehouseLocation = "主仓库",
                    CreatedAt = DateTime.UtcNow
                };
                stockInLines.Add(stockInLine);

                // 更新入库单总金额
                stockInHead.TotalAmount = subtotal;

                invoiceCounter++;
            }
        }

        // 批量保存
        context.Drugs.AddRange(drugs);
        await context.SaveChangesAsync();

        context.DrugInventories.AddRange(inventories);
        context.DrugStockInHeads.AddRange(stockInHeads);
        context.DrugStockInLines.AddRange(stockInLines);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// 根据分类生成示例药品
    /// </summary>
    private static List<Drug> GenerateDrugsForCategory(DrugCategory category, Random random, HashSet<string> usedApprovalNumbers)
    {
        var drugs = new List<Drug>();
        var categoryName = category.CategoryName;

        // 根据分类名称生成相应的药品
        var drugTemplates = GetDrugTemplatesByCategory(categoryName);

        // 每个分类创建 2-3 个药品
        int drugCount = random.Next(2, 4);
        var selectedTemplates = drugTemplates.Take(drugCount).ToList();

        foreach (var template in selectedTemplates)
        {
            // 生成唯一的批准文号
            string approvalNumber;
            do
            {
                approvalNumber = GenerateApprovalNumber(random);
            } while (usedApprovalNumbers.Contains(approvalNumber));
            usedApprovalNumbers.Add(approvalNumber);

            var drug = new Drug
            {
                Id = Guid.NewGuid(),
                CommonName = template.CommonName,
                TradeName = template.TradeName,
                Specification = template.Specification,
                Manufacturer = template.Manufacturer,
                ApprovalNumber = approvalNumber,
                CategoryId = category.Id,
                Unit = template.Unit,
                StorageCondition = template.StorageCondition,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            drugs.Add(drug);
        }

        return drugs;
    }

    /// <summary>
    /// 根据分类获取药品模板
    /// </summary>
    private static List<DrugTemplate> GetDrugTemplatesByCategory(string categoryName)
    {
        // 药品模板库（覆盖主要分类）
        var allTemplates = new Dictionary<string, List<DrugTemplate>>();

        // 抗感染药类
        allTemplates["抗生素"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "阿莫西林", TradeName = "阿莫仙", Specification = "0.25g*24粒/盒", Manufacturer = "华北制药", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "头孢克肟", TradeName = "头孢克肟胶囊", Specification = "0.1g*12粒/盒", Manufacturer = "石药集团", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "阿奇霉素", TradeName = "希舒美", Specification = "0.25g*6片/盒", Manufacturer = "辉瑞制药", Unit = "盒", StorageCondition = "常温" }
        };

        allTemplates["抗病毒药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "奥司他韦", TradeName = "达菲", Specification = "75mg*10粒/盒", Manufacturer = "罗氏制药", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "利巴韦林", TradeName = "病毒唑", Specification = "0.1g*100片/瓶", Manufacturer = "齐鲁制药", Unit = "瓶", StorageCondition = "常温" }
        };

        allTemplates["抗真菌药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "氟康唑", TradeName = "大扶康", Specification = "50mg*7粒/盒", Manufacturer = "辉瑞制药", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "伊曲康唑", TradeName = "斯皮仁诺", Specification = "0.1g*14粒/盒", Manufacturer = "西安杨森", Unit = "盒", StorageCondition = "常温" }
        };

        // 心血管系统用药
        allTemplates["抗高血压药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "氨氯地平", TradeName = "络活喜", Specification = "5mg*7片/盒", Manufacturer = "辉瑞制药", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "缬沙坦", TradeName = "代文", Specification = "80mg*7粒/盒", Manufacturer = "诺华制药", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "美托洛尔", TradeName = "倍他乐克", Specification = "25mg*20片/盒", Manufacturer = "阿斯利康", Unit = "盒", StorageCondition = "常温" }
        };

        allTemplates["降血脂药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "阿托伐他汀", TradeName = "立普妥", Specification = "20mg*7片/盒", Manufacturer = "辉瑞制药", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "辛伐他汀", TradeName = "舒降之", Specification = "20mg*10片/盒", Manufacturer = "默克制药", Unit = "盒", StorageCondition = "常温" }
        };

        // 消化系统用药
        allTemplates["抗酸药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "奥美拉唑", TradeName = "洛赛克", Specification = "20mg*14粒/盒", Manufacturer = "阿斯利康", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "雷贝拉唑", TradeName = "波利特", Specification = "10mg*14粒/盒", Manufacturer = "卫材制药", Unit = "盒", StorageCondition = "常温" }
        };

        allTemplates["止泻药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "蒙脱石散", TradeName = "思密达", Specification = "3g*10袋/盒", Manufacturer = "博福-益普生", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "洛哌丁胺", TradeName = "易蒙停", Specification = "2mg*6粒/盒", Manufacturer = "西安杨森", Unit = "盒", StorageCondition = "常温" }
        };

        // 呼吸系统用药
        allTemplates["镇咳药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "右美沙芬", TradeName = "右美沙芬片", Specification = "15mg*12片/盒", Manufacturer = "三九制药", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "可待因", TradeName = "可待因片", Specification = "15mg*10片/盒", Manufacturer = "国药集团", Unit = "盒", StorageCondition = "常温" }
        };

        allTemplates["平喘药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "沙丁胺醇", TradeName = "万托林", Specification = "100μg*200揿/瓶", Manufacturer = "葛兰素史克", Unit = "瓶", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "布地奈德", TradeName = "普米克", Specification = "200μg*100揿/瓶", Manufacturer = "阿斯利康", Unit = "瓶", StorageCondition = "常温" }
        };

        // 神经系统用药
        allTemplates["镇痛药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "布洛芬", TradeName = "芬必得", Specification = "0.3g*20粒/盒", Manufacturer = "中美天津史克", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "对乙酰氨基酚", TradeName = "泰诺", Specification = "0.5g*10片/盒", Manufacturer = "强生制药", Unit = "盒", StorageCondition = "常温" }
        };

        allTemplates["镇静催眠药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "地西泮", TradeName = "安定", Specification = "2.5mg*20片/盒", Manufacturer = "国药集团", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "艾司唑仑", TradeName = "舒乐安定", Specification = "1mg*20片/盒", Manufacturer = "三九制药", Unit = "盒", StorageCondition = "常温" }
        };

        // 内分泌系统用药
        allTemplates["降糖药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "二甲双胍", TradeName = "格华止", Specification = "0.5g*20片/盒", Manufacturer = "默克制药", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "格列齐特", TradeName = "达美康", Specification = "80mg*30片/盒", Manufacturer = "施维雅制药", Unit = "盒", StorageCondition = "常温" }
        };

        // 维生素与矿物质
        allTemplates["维生素类"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "维生素C", TradeName = "维C片", Specification = "0.1g*100片/瓶", Manufacturer = "哈药集团", Unit = "瓶", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "维生素B1", TradeName = "维生素B1片", Specification = "10mg*100片/瓶", Manufacturer = "华北制药", Unit = "瓶", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "维生素D3", TradeName = "维生素D3胶囊", Specification = "400IU*30粒/盒", Manufacturer = "汤臣倍健", Unit = "盒", StorageCondition = "常温" }
        };

        allTemplates["矿物质类"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "碳酸钙", TradeName = "钙尔奇", Specification = "600mg*30片/盒", Manufacturer = "惠氏制药", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "硫酸亚铁", TradeName = "硫酸亚铁片", Specification = "0.3g*100片/瓶", Manufacturer = "华北制药", Unit = "瓶", StorageCondition = "常温" }
        };

        // 皮肤科用药
        allTemplates["抗真菌药（外用）"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "克霉唑", TradeName = "克霉唑软膏", Specification = "1%*10g/支", Manufacturer = "三九制药", Unit = "支", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "咪康唑", TradeName = "达克宁", Specification = "2%*15g/支", Manufacturer = "西安杨森", Unit = "支", StorageCondition = "常温" }
        };

        // 眼科用药
        allTemplates["抗感染眼药"] = new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = "左氧氟沙星", TradeName = "可乐必妥", Specification = "0.5%*5ml/支", Manufacturer = "参天制药", Unit = "支", StorageCondition = "常温" },
            new DrugTemplate { CommonName = "妥布霉素", TradeName = "托百士", Specification = "0.3%*5ml/支", Manufacturer = "爱尔康", Unit = "支", StorageCondition = "常温" }
        };

        // 如果找到匹配的分类，返回对应的模板
        if (allTemplates.ContainsKey(categoryName))
        {
            return allTemplates[categoryName];
        }

        // 如果没有匹配的，返回通用模板
        return new List<DrugTemplate>
        {
            new DrugTemplate { CommonName = $"{categoryName}示例药品1", TradeName = "示例商品名1", Specification = "10mg*20片/盒", Manufacturer = "示例药厂", Unit = "盒", StorageCondition = "常温" },
            new DrugTemplate { CommonName = $"{categoryName}示例药品2", TradeName = "示例商品名2", Specification = "5ml*10支/盒", Manufacturer = "示例药厂", Unit = "盒", StorageCondition = "常温" }
        };
    }

    /// <summary>
    /// 生成国药准字批准文号
    /// </summary>
    private static string GenerateApprovalNumber(Random random)
    {
        // 国药准字格式：国药准字H+8位数字（化学药品）
        // 示例：国药准字H20123456
        var number = random.Next(10000000, 99999999);
        return $"国药准字H{number}";
    }

    /// <summary>
    /// 获取随机供应商名称
    /// </summary>
    private static string GetRandomSupplier(Random random)
    {
        var suppliers = new[]
        {
            "国药集团",
            "华润医药",
            "上药集团",
            "九州通医药",
            "华东医药",
            "海王生物",
            "康美药业",
            "恒瑞医药"
        };
        return suppliers[random.Next(suppliers.Length)];
    }

    /// <summary>
    /// 药品模板类
    /// </summary>
    private class DrugTemplate
    {
        public string CommonName { get; set; } = string.Empty;
        public string? TradeName { get; set; }
        public string Specification { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string StorageCondition { get; set; } = "常温";
    }
}


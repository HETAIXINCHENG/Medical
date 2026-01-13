using Medical.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Data;

/// <summary>
/// 药品分类种子数据（西药分类）
/// </summary>
public static class DrugCategorySeeder
{
    /// <summary>
    /// 初始化西药分类数据
    /// </summary>
    public static async Task SeedAsync(MedicalDbContext context)
    {
        // 检查是否已有数据
        if (await context.DrugCategories.AnyAsync())
        {
            return; // 已有数据，不重复插入
        }

        var categories = new List<DrugCategory>();
        int sortOrder = 1;

        // 1. 抗感染药
        var antiInfective = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "抗感染药",
            Description = "用于治疗各种感染性疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(antiInfective);

        // 抗感染药子分类
        var antiInfectiveChildren = new[]
        {
            new DrugCategory { CategoryName = "抗生素", Description = "青霉素类、头孢菌素类、大环内酯类等", SortOrder = 1 },
            new DrugCategory { CategoryName = "抗病毒药", Description = "用于治疗病毒感染的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "抗真菌药", Description = "用于治疗真菌感染的药物", SortOrder = 3 },
            new DrugCategory { CategoryName = "抗寄生虫药", Description = "用于治疗寄生虫感染的药物", SortOrder = 4 },
            new DrugCategory { CategoryName = "抗结核药", Description = "用于治疗结核病的药物", SortOrder = 5 },
            new DrugCategory { CategoryName = "喹诺酮类", Description = "氟喹诺酮类抗菌药物", SortOrder = 6 },
            new DrugCategory { CategoryName = "磺胺类", Description = "磺胺类抗菌药物", SortOrder = 7 }
        };

        foreach (var child in antiInfectiveChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = antiInfective.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 2. 心血管系统用药
        var cardiovascular = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "心血管系统用药",
            Description = "用于治疗心血管疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(cardiovascular);

        var cardiovascularChildren = new[]
        {
            new DrugCategory { CategoryName = "抗高血压药", Description = "用于降低血压的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "抗心绞痛药", Description = "用于治疗心绞痛的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "抗心律失常药", Description = "用于治疗心律失常的药物", SortOrder = 3 },
            new DrugCategory { CategoryName = "强心药", Description = "用于增强心肌收缩力的药物", SortOrder = 4 },
            new DrugCategory { CategoryName = "抗血栓药", Description = "抗血小板聚集、抗凝药物", SortOrder = 5 },
            new DrugCategory { CategoryName = "降血脂药", Description = "用于降低血脂的药物", SortOrder = 6 },
            new DrugCategory { CategoryName = "血管扩张药", Description = "用于扩张血管的药物", SortOrder = 7 }
        };

        foreach (var child in cardiovascularChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = cardiovascular.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 3. 消化系统用药
        var digestive = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "消化系统用药",
            Description = "用于治疗消化系统疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(digestive);

        var digestiveChildren = new[]
        {
            new DrugCategory { CategoryName = "抗酸药", Description = "用于中和胃酸的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "胃黏膜保护药", Description = "保护胃黏膜的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "胃肠动力药", Description = "促进胃肠蠕动的药物", SortOrder = 3 },
            new DrugCategory { CategoryName = "止泻药", Description = "用于治疗腹泻的药物", SortOrder = 4 },
            new DrugCategory { CategoryName = "导泻药", Description = "用于治疗便秘的药物", SortOrder = 5 },
            new DrugCategory { CategoryName = "止吐药", Description = "用于止吐的药物", SortOrder = 6 },
            new DrugCategory { CategoryName = "肝胆疾病用药", Description = "用于治疗肝胆疾病的药物", SortOrder = 7 }
        };

        foreach (var child in digestiveChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = digestive.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 4. 呼吸系统用药
        var respiratory = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "呼吸系统用药",
            Description = "用于治疗呼吸系统疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(respiratory);

        var respiratoryChildren = new[]
        {
            new DrugCategory { CategoryName = "镇咳药", Description = "用于止咳的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "祛痰药", Description = "用于祛痰的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "平喘药", Description = "用于治疗哮喘的药物", SortOrder = 3 }
        };

        foreach (var child in respiratoryChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = respiratory.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 5. 神经系统用药
        var nervous = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "神经系统用药",
            Description = "用于治疗神经系统疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(nervous);

        var nervousChildren = new[]
        {
            new DrugCategory { CategoryName = "镇静催眠药", Description = "用于镇静和催眠的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "抗癫痫药", Description = "用于治疗癫痫的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "抗精神病药", Description = "用于治疗精神病的药物", SortOrder = 3 },
            new DrugCategory { CategoryName = "抗抑郁药", Description = "用于治疗抑郁症的药物", SortOrder = 4 },
            new DrugCategory { CategoryName = "抗焦虑药", Description = "用于治疗焦虑症的药物", SortOrder = 5 },
            new DrugCategory { CategoryName = "镇痛药", Description = "用于镇痛的药物", SortOrder = 6 },
            new DrugCategory { CategoryName = "抗帕金森病药", Description = "用于治疗帕金森病的药物", SortOrder = 7 }
        };

        foreach (var child in nervousChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = nervous.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 6. 内分泌系统用药
        var endocrine = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "内分泌系统用药",
            Description = "用于治疗内分泌系统疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(endocrine);

        var endocrineChildren = new[]
        {
            new DrugCategory { CategoryName = "降糖药", Description = "用于治疗糖尿病的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "甲状腺用药", Description = "用于治疗甲状腺疾病的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "肾上腺皮质激素", Description = "糖皮质激素、盐皮质激素", SortOrder = 3 },
            new DrugCategory { CategoryName = "性激素", Description = "雌激素、孕激素、雄激素", SortOrder = 4 },
            new DrugCategory { CategoryName = "促性腺激素", Description = "用于调节性腺功能的药物", SortOrder = 5 }
        };

        foreach (var child in endocrineChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = endocrine.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 7. 抗肿瘤药
        var antineoplastic = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "抗肿瘤药",
            Description = "用于治疗肿瘤的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(antineoplastic);

        var antineoplasticChildren = new[]
        {
            new DrugCategory { CategoryName = "烷化剂", Description = "用于肿瘤化疗的烷化剂", SortOrder = 1 },
            new DrugCategory { CategoryName = "抗代谢药", Description = "抗肿瘤代谢药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "抗肿瘤抗生素", Description = "用于抗肿瘤的抗生素", SortOrder = 3 },
            new DrugCategory { CategoryName = "植物类抗肿瘤药", Description = "从植物中提取的抗肿瘤药物", SortOrder = 4 },
            new DrugCategory { CategoryName = "激素类抗肿瘤药", Description = "用于抗肿瘤的激素类药物", SortOrder = 5 },
            new DrugCategory { CategoryName = "靶向抗肿瘤药", Description = "靶向治疗肿瘤的药物", SortOrder = 6 }
        };

        foreach (var child in antineoplasticChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = antineoplastic.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 8. 血液系统用药
        var hematologic = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "血液系统用药",
            Description = "用于治疗血液系统疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(hematologic);

        var hematologicChildren = new[]
        {
            new DrugCategory { CategoryName = "抗贫血药", Description = "用于治疗贫血的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "促凝血药", Description = "用于促进凝血的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "抗凝血药", Description = "用于抗凝血的药物", SortOrder = 3 },
            new DrugCategory { CategoryName = "促白细胞增生药", Description = "用于促进白细胞增生的药物", SortOrder = 4 }
        };

        foreach (var child in hematologicChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = hematologic.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 9. 免疫系统用药
        var immune = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "免疫系统用药",
            Description = "用于调节免疫系统的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(immune);

        var immuneChildren = new[]
        {
            new DrugCategory { CategoryName = "免疫抑制剂", Description = "用于抑制免疫反应的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "免疫增强剂", Description = "用于增强免疫功能的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "抗过敏药", Description = "用于抗过敏的药物", SortOrder = 3 }
        };

        foreach (var child in immuneChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = immune.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 10. 泌尿系统用药
        var urologic = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "泌尿系统用药",
            Description = "用于治疗泌尿系统疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(urologic);

        var urologicChildren = new[]
        {
            new DrugCategory { CategoryName = "利尿药", Description = "用于促进排尿的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "抗前列腺增生药", Description = "用于治疗前列腺增生的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "泌尿系统抗感染药", Description = "用于治疗泌尿系统感染的药物", SortOrder = 3 }
        };

        foreach (var child in urologicChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = urologic.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 11. 生殖系统用药
        var reproductive = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "生殖系统用药",
            Description = "用于治疗生殖系统疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(reproductive);

        var reproductiveChildren = new[]
        {
            new DrugCategory { CategoryName = "避孕药", Description = "用于避孕的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "促生育药", Description = "用于促进生育的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "妇科用药", Description = "用于治疗妇科疾病的药物", SortOrder = 3 }
        };

        foreach (var child in reproductiveChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = reproductive.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 12. 皮肤科用药
        var dermatologic = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "皮肤科用药",
            Description = "用于治疗皮肤疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(dermatologic);

        var dermatologicChildren = new[]
        {
            new DrugCategory { CategoryName = "抗真菌药（外用）", Description = "外用抗真菌药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "抗细菌药（外用）", Description = "外用抗细菌药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "抗病毒药（外用）", Description = "外用抗病毒药物", SortOrder = 3 },
            new DrugCategory { CategoryName = "激素类（外用）", Description = "外用激素类药物", SortOrder = 4 },
            new DrugCategory { CategoryName = "抗过敏药（外用）", Description = "外用抗过敏药物", SortOrder = 5 }
        };

        foreach (var child in dermatologicChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = dermatologic.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 13. 眼科用药
        var ophthalmic = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "眼科用药",
            Description = "用于治疗眼部疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(ophthalmic);

        var ophthalmicChildren = new[]
        {
            new DrugCategory { CategoryName = "抗感染眼药", Description = "用于眼部抗感染的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "抗炎眼药", Description = "用于眼部抗炎的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "降眼压药", Description = "用于降低眼压的药物", SortOrder = 3 },
            new DrugCategory { CategoryName = "散瞳药", Description = "用于散瞳的药物", SortOrder = 4 }
        };

        foreach (var child in ophthalmicChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = ophthalmic.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 14. 耳鼻喉科用药
        var ent = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "耳鼻喉科用药",
            Description = "用于治疗耳鼻喉疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(ent);

        var entChildren = new[]
        {
            new DrugCategory { CategoryName = "鼻用药物", Description = "用于治疗鼻部疾病的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "耳用药物", Description = "用于治疗耳部疾病的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "咽喉用药", Description = "用于治疗咽喉疾病的药物", SortOrder = 3 }
        };

        foreach (var child in entChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = ent.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 15. 骨科用药
        var orthopedic = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "骨科用药",
            Description = "用于治疗骨科疾病的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(orthopedic);

        var orthopedicChildren = new[]
        {
            new DrugCategory { CategoryName = "抗骨质疏松药", Description = "用于治疗骨质疏松的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "抗风湿药", Description = "用于治疗风湿性疾病的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "肌肉松弛药", Description = "用于肌肉松弛的药物", SortOrder = 3 }
        };

        foreach (var child in orthopedicChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = orthopedic.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 16. 麻醉药
        var anesthetic = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "麻醉药",
            Description = "用于麻醉的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(anesthetic);

        var anestheticChildren = new[]
        {
            new DrugCategory { CategoryName = "全身麻醉药", Description = "用于全身麻醉的药物", SortOrder = 1 },
            new DrugCategory { CategoryName = "局部麻醉药", Description = "用于局部麻醉的药物", SortOrder = 2 },
            new DrugCategory { CategoryName = "肌肉松弛药（麻醉用）", Description = "用于麻醉的肌肉松弛药物", SortOrder = 3 }
        };

        foreach (var child in anestheticChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = anesthetic.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 17. 解毒药
        var antidote = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "解毒药",
            Description = "用于解毒的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(antidote);

        // 18. 生物制品
        var biological = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "生物制品",
            Description = "生物技术药物和疫苗",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(biological);

        var biologicalChildren = new[]
        {
            new DrugCategory { CategoryName = "疫苗", Description = "预防性疫苗", SortOrder = 1 },
            new DrugCategory { CategoryName = "免疫血清", Description = "免疫血清和免疫球蛋白", SortOrder = 2 },
            new DrugCategory { CategoryName = "血液制品", Description = "血液制品", SortOrder = 3 },
            new DrugCategory { CategoryName = "基因工程药物", Description = "基因工程药物", SortOrder = 4 }
        };

        foreach (var child in biologicalChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = biological.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 19. 维生素与矿物质
        var vitamin = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "维生素与矿物质",
            Description = "维生素和矿物质补充剂",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(vitamin);

        var vitaminChildren = new[]
        {
            new DrugCategory { CategoryName = "维生素类", Description = "各种维生素", SortOrder = 1 },
            new DrugCategory { CategoryName = "矿物质类", Description = "钙、铁、锌等矿物质", SortOrder = 2 },
            new DrugCategory { CategoryName = "复合维生素", Description = "复合维生素制剂", SortOrder = 3 }
        };

        foreach (var child in vitaminChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = vitamin.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 20. 营养药
        var nutrition = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "营养药",
            Description = "营养支持药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(nutrition);

        var nutritionChildren = new[]
        {
            new DrugCategory { CategoryName = "氨基酸类", Description = "氨基酸制剂", SortOrder = 1 },
            new DrugCategory { CategoryName = "脂肪乳", Description = "脂肪乳制剂", SortOrder = 2 },
            new DrugCategory { CategoryName = "肠内营养", Description = "肠内营养制剂", SortOrder = 3 },
            new DrugCategory { CategoryName = "肠外营养", Description = "肠外营养制剂", SortOrder = 4 }
        };

        foreach (var child in nutritionChildren)
        {
            child.Id = Guid.NewGuid();
            child.ParentId = nutrition.Id;
            child.IsActive = true;
            child.CreatedAt = DateTime.UtcNow;
            child.UpdatedAt = DateTime.UtcNow;
            categories.Add(child);
        }

        // 21. 其他药物
        var other = new DrugCategory
        {
            Id = Guid.NewGuid(),
            CategoryName = "其他药物",
            Description = "其他未分类的药物",
            SortOrder = sortOrder++,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        categories.Add(other);

        context.DrugCategories.AddRange(categories);
        await context.SaveChangesAsync();
    }
}


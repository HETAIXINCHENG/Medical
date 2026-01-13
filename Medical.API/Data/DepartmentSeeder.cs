using Medical.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Data;

/// <summary>
/// 科室数据种子（三甲医院标准科室配置）
/// </summary>
public static class DepartmentSeeder
{
    /// <summary>
    /// 初始化三甲医院所有科室数据
    /// </summary>
    public static async Task SeedAsync(MedicalDbContext context)
    {
        // 检查是否已有科室数据
        var existingDepartmentCount = await context.Departments.CountAsync();

        if (existingDepartmentCount > 0)
        {
            return; // 已有科室数据，不重复插入
        }

        var departments = new List<Department>
        {
            // ========== 内科系统 ==========
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "心血管内科",
                Description = "诊治心血管疾病，包括冠心病、高血压、心律失常、心力衰竭等",
                SortOrder = 1,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "呼吸内科",
                Description = "诊治呼吸系统疾病，包括肺炎、哮喘、慢阻肺、肺癌等",
                SortOrder = 2,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "消化内科",
                Description = "诊治消化系统疾病，包括胃炎、胃溃疡、肝炎、肝硬化、胰腺炎等",
                SortOrder = 3,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "神经内科",
                Description = "诊治神经系统疾病，包括脑梗塞、脑出血、癫痫、帕金森病、痴呆等",
                SortOrder = 4,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "内分泌科",
                Description = "诊治内分泌疾病，包括糖尿病、甲状腺疾病、肾上腺疾病、骨质疏松等",
                SortOrder = 5,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "肾内科",
                Description = "诊治肾脏疾病，包括肾炎、肾病综合征、肾衰竭、血液透析等",
                SortOrder = 6,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "血液内科",
                Description = "诊治血液系统疾病，包括贫血、白血病、淋巴瘤、血小板疾病等",
                SortOrder = 7,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "风湿免疫科",
                Description = "诊治风湿免疫性疾病，包括类风湿关节炎、系统性红斑狼疮、强直性脊柱炎等",
                SortOrder = 8,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "感染科",
                Description = "诊治感染性疾病，包括病毒性肝炎、艾滋病、结核病、发热待查等",
                SortOrder = 9,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ========== 外科系统 ==========
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "普通外科",
                Description = "诊治普通外科疾病，包括胃肠、肝胆、甲状腺、乳腺等疾病的手术治疗",
                SortOrder = 10,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "骨科",
                Description = "诊治骨关节疾病，包括骨折、关节置换、脊柱疾病、运动损伤等",
                SortOrder = 11,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "神经外科",
                Description = "诊治神经系统外科疾病，包括脑肿瘤、脑外伤、脑血管病、脊柱脊髓疾病等",
                SortOrder = 12,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "胸外科",
                Description = "诊治胸部疾病，包括肺癌、食管癌、纵隔肿瘤、胸外伤等",
                SortOrder = 13,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "泌尿外科",
                Description = "诊治泌尿系统疾病，包括肾结石、前列腺疾病、膀胱肿瘤、男性不育等",
                SortOrder = 14,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "心脏外科",
                Description = "诊治心脏疾病，包括先天性心脏病、心脏瓣膜病、冠心病搭桥等",
                SortOrder = 15,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "血管外科",
                Description = "诊治血管疾病，包括动脉硬化、静脉曲张、主动脉瘤、血管畸形等",
                SortOrder = 16,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "整形外科",
                Description = "整形美容、烧伤修复、手外科、显微外科等",
                SortOrder = 17,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "烧伤科",
                Description = "诊治烧伤、烫伤、电击伤、化学烧伤等",
                SortOrder = 18,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ========== 妇产科 ==========
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "妇科",
                Description = "诊治妇科疾病，包括妇科肿瘤、妇科炎症、月经不调、不孕不育等",
                SortOrder = 19,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "产科",
                Description = "孕产期保健、高危妊娠、分娩、产后康复等",
                SortOrder = 20,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "计划生育科",
                Description = "计划生育技术服务、避孕指导、人工流产、节育手术等",
                SortOrder = 21,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ========== 儿科 ==========
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "小儿内科",
                Description = "诊治儿童内科疾病，包括呼吸、消化、神经、心血管等系统疾病",
                SortOrder = 22,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "小儿外科",
                Description = "诊治儿童外科疾病，包括先天性畸形、肿瘤、创伤等",
                SortOrder = 23,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "新生儿科",
                Description = "诊治新生儿疾病，包括早产儿、低出生体重儿、新生儿黄疸、呼吸窘迫等",
                SortOrder = 24,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ========== 五官科 ==========
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "眼科",
                Description = "诊治眼部疾病，包括白内障、青光眼、眼底病、屈光不正、眼外伤等",
                SortOrder = 25,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "耳鼻喉科",
                Description = "诊治耳鼻喉疾病，包括中耳炎、鼻炎、鼻窦炎、咽喉炎、听力障碍等",
                SortOrder = 26,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "口腔科",
                Description = "诊治口腔疾病，包括牙病、口腔黏膜病、口腔颌面外科、正畸等",
                SortOrder = 27,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ========== 其他临床科室 ==========
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "皮肤科",
                Description = "诊治皮肤疾病，包括湿疹、皮炎、银屑病、白癜风、性病等",
                SortOrder = 28,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "精神科",
                Description = "诊治精神心理疾病，包括抑郁症、焦虑症、精神分裂症、睡眠障碍等",
                SortOrder = 29,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "康复医学科",
                Description = "康复治疗，包括物理治疗、作业治疗、言语治疗、康复训练等",
                SortOrder = 30,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "中医科",
                Description = "中医诊疗，包括中医内科、中医外科、针灸、推拿、中药等",
                SortOrder = 31,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "急诊科",
                Description = "急诊急救，包括急危重症抢救、创伤急救、中毒救治等",
                SortOrder = 32,
                IsHot = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "重症医学科",
                Description = "重症监护，包括ICU、CCU、NICU等重症患者的监护和治疗",
                SortOrder = 33,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "麻醉科",
                Description = "麻醉服务，包括手术麻醉、疼痛治疗、无痛分娩、无痛胃肠镜等",
                SortOrder = 34,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ========== 医技科室 ==========
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "医学影像科",
                Description = "医学影像检查，包括X线、CT、MRI、超声、核医学等",
                SortOrder = 35,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "检验科",
                Description = "临床检验，包括血液检验、生化检验、免疫检验、微生物检验等",
                SortOrder = 36,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "病理科",
                Description = "病理诊断，包括组织病理、细胞病理、免疫病理、分子病理等",
                SortOrder = 37,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Department
            {
                Id = Guid.NewGuid(),
                Name = "药剂科",
                Description = "药品管理，包括药品采购、储存、调剂、临床药学服务等",
                SortOrder = 38,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();
    }
}


using Medical.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medical.API.Data;

/// <summary>
/// 医生数据种子（为每个科室创建至少5名医生）
/// </summary>
public static class DoctorSeeder
{
    /// <summary>
    /// 初始化医生数据
    /// </summary>
    public static async Task SeedAsync(MedicalDbContext context)
    {
        // 检查是否已有医生数据
        var existingDoctorCount = await context.Doctors.CountAsync();

        if (existingDoctorCount > 0)
        {
            return; // 已有医生数据，不重复插入
        }

        // 获取所有科室
        var departments = await context.Departments.ToListAsync();

        if (departments.Count == 0)
        {
            return; // 没有科室，无法创建医生
        }

        var random = new Random();
        var doctors = new List<Doctor>();

        // 中文姓氏
        var chineseSurnames = new[] { "张", "王", "李", "刘", "陈", "杨", "赵", "黄", "周", "吴", "徐", "孙", "胡", "朱", "高", "林", "何", "郭", "马", "罗", "梁", "宋", "郑", "谢", "韩", "唐", "冯", "于", "董", "萧", "程", "曹", "袁", "邓", "许", "傅", "沈", "曾", "彭", "吕" };

        // 中文名字（单字）
        var chineseGivenNamesSingle = new[] { "伟", "芳", "娜", "敏", "静", "丽", "强", "磊", "军", "洋", "勇", "艳", "杰", "娟", "涛", "明", "超", "秀兰", "霞", "平", "刚", "桂英", "建华", "文", "华", "建国", "红", "桂兰", "志强", "秀英", "秀华", "秀芳", "秀梅", "秀珍", "秀云", "秀霞", "秀红", "秀琴", "秀芳" };

        // 中文名字（双字）
        var chineseGivenNamesDouble = new[] { "志强", "建华", "建国", "国庆", "国强", "国华", "国明", "国平", "国军", "国伟", "国勇", "国华", "国辉", "国亮", "国峰", "国栋", "国梁", "国新", "国兴", "国盛", "秀英", "秀华", "秀芳", "秀梅", "秀珍", "秀云", "秀霞", "秀红", "秀琴", "秀芳", "秀兰", "秀莲", "秀菊", "秀花", "秀玉", "秀珠", "秀芬", "秀香", "秀美", "秀清" };

        // 职称列表
        var titles = new[] { "住院医师", "主治医师", "副主任医师", "主任医师", "教授", "副教授" };

        // 医院名称
        var hospitals = new[] { "市第一人民医院", "市中心医院", "市人民医院", "医科大学附属医院", "省人民医院" };

        // 擅长领域模板（根据科室类型）
        var specialtyTemplates = new Dictionary<string, string[]>
        {
            { "心血管内科", new[] { "冠心病", "高血压", "心律失常", "心力衰竭", "心肌病" } },
            { "呼吸内科", new[] { "慢性阻塞性肺疾病", "支气管哮喘", "肺炎", "肺癌", "间质性肺病" } },
            { "消化内科", new[] { "胃炎", "胃溃疡", "肝炎", "肝硬化", "胰腺炎" } },
            { "神经内科", new[] { "脑梗塞", "脑出血", "癫痫", "帕金森病", "痴呆" } },
            { "内分泌科", new[] { "糖尿病", "甲状腺疾病", "肾上腺疾病", "骨质疏松", "代谢综合征" } },
            { "普通外科", new[] { "胃肠肿瘤", "肝胆疾病", "甲状腺疾病", "乳腺疾病", "疝气" } },
            { "骨科", new[] { "关节置换", "脊柱疾病", "创伤骨科", "运动医学", "骨肿瘤" } },
            { "泌尿外科", new[] { "肾结石", "前列腺疾病", "膀胱肿瘤", "男性不育", "泌尿系感染" } },
            { "妇科", new[] { "妇科肿瘤", "妇科炎症", "月经不调", "不孕不育", "更年期综合征" } },
            { "产科", new[] { "高危妊娠", "产前诊断", "分娩", "产后康复", "围产期保健" } },
            { "小儿内科", new[] { "小儿呼吸", "小儿消化", "小儿神经", "小儿心血管", "小儿内分泌" } },
            { "眼科", new[] { "白内障", "青光眼", "眼底病", "屈光不正", "眼外伤" } },
            { "耳鼻喉科", new[] { "中耳炎", "鼻炎", "鼻窦炎", "咽喉炎", "听力障碍" } },
            { "口腔科", new[] { "牙体牙髓", "口腔颌面外科", "正畸", "种植牙", "口腔修复" } },
            { "皮肤科", new[] { "湿疹", "皮炎", "银屑病", "白癜风", "性病" } }
        };

        foreach (var department in departments)
        {
            // 每个科室创建5-7名医生（随机）
            int doctorCount = random.Next(5, 8);

            for (int i = 0; i < doctorCount; i++)
            {
                // 生成随机姓名（2~3个字）
                string name;
                if (random.Next(2) == 0)
                {
                    // 2个字：姓 + 单字名
                    var surname = chineseSurnames[random.Next(chineseSurnames.Length)];
                    var givenName = chineseGivenNamesSingle[random.Next(chineseGivenNamesSingle.Length)];
                    name = $"{surname}{givenName}";
                }
                else
                {
                    // 3个字：姓 + 双字名
                    var surname = chineseSurnames[random.Next(chineseSurnames.Length)];
                    var givenName = chineseGivenNamesDouble[random.Next(chineseGivenNamesDouble.Length)];
                    name = $"{surname}{givenName}";
                }

                // 随机职称（权重：住院医师20%，主治医师30%，副主任医师25%，主任医师15%，教授/副教授10%）
                string title;
                var titleRandom = random.Next(100);
                if (titleRandom < 20)
                    title = titles[0]; // 住院医师
                else if (titleRandom < 50)
                    title = titles[1]; // 主治医师
                else if (titleRandom < 75)
                    title = titles[2]; // 副主任医师
                else if (titleRandom < 90)
                    title = titles[3]; // 主任医师
                else
                    title = titles[random.Next(4, 6)]; // 教授或副教授

                // 随机医院
                var hospital = hospitals[random.Next(hospitals.Length)];

                // 生成擅长领域
                string? specialty = null;
                if (specialtyTemplates.ContainsKey(department.Name))
                {
                    var specialties = specialtyTemplates[department.Name];
                    var selectedSpecialties = specialties.OrderBy(x => random.Next()).Take(random.Next(2, 4));
                    specialty = string.Join("、", selectedSpecialties);
                }
                else
                {
                    // 如果没有模板，使用通用描述
                    specialty = $"{department.Name}常见疾病的诊治";
                }

                // 生成简介
                var introduction = $"{name}，{title}，从事{department.Name}临床工作{random.Next(5, 30)}年，擅长{specialty}。";

                // 随机评分（3.5-5.0）
                var rating = Math.Round((decimal)(random.Next(35, 51)) / 10, 1);

                // 随机咨询次数（0-500）
                var consultationCount = random.Next(0, 501);

                // 随机是否在线
                var isOnline = random.Next(10) < 3; // 30%概率在线

                // 随机是否推荐（20%概率）
                var isRecommended = random.Next(10) < 2;

                var doctor = new Doctor
                {
                    Id = Guid.NewGuid(),
                    DepartmentId = department.Id,
                    Name = name,
                    Title = title,
                    Specialty = specialty,
                    Hospital = hospital,
                    Introduction = introduction,
                    Rating = rating,
                    ConsultationCount = consultationCount,
                    IsOnline = isOnline,
                    IsRecommended = isRecommended,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                doctors.Add(doctor);
            }
        }

        context.Doctors.AddRange(doctors);
        await context.SaveChangesAsync();
    }
}


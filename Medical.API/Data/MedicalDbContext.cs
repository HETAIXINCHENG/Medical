using Microsoft.EntityFrameworkCore;
using Medical.API.Models.Entities;

namespace Medical.API.Data;

/// <summary>
/// 医疗系统数据库上下文
/// </summary>
public class MedicalDbContext : DbContext
{
    public MedicalDbContext(DbContextOptions<MedicalDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<UserTypeDictionary> UserTypeDictionaries { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<FamilyMember> FamilyMembers { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<DiseaseCategory> DiseaseCategories { get; set; }
    public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
    public DbSet<Consultation> Consultations { get; set; }
    public DbSet<ConsultationMessage> ConsultationMessages { get; set; }
    public DbSet<ConsultationPatient> ConsultationPatients { get; set; }
    public DbSet<HealthKnowledge> HealthKnowledge { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<DoctorReview> DoctorReviews { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionMedicine> PrescriptionMedicines { get; set; }
    public DbSet<Medicine> Medicines { get; set; }
    public DbSet<VisitRecord> VisitRecords { get; set; }
    public DbSet<ExaminationReport> ExaminationReports { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<MenuPermission> MenuPermissions { get; set; }
    public DbSet<PermissionTypeDictionary> PermissionTypeDictionaries { get; set; }
    public DbSet<DrugCategory> DrugCategories { get; set; }
    public DbSet<Drug> Drugs { get; set; }
    public DbSet<DrugInventory> DrugInventories { get; set; }
    public DbSet<DrugStockInHead> DrugStockInHeads { get; set; }
    public DbSet<DrugStockInLine> DrugStockInLines { get; set; }
    public DbSet<UserHealthKnowledgeFavorite> UserHealthKnowledgeFavorites { get; set; }
    public DbSet<DoctorPatient> DoctorPatients { get; set; }
    public DbSet<UserDoctorSubscription> UserDoctorSubscriptions { get; set; }
    public DbSet<PatientSupportGroup> PatientSupportGroups { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostComment> PostComments { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<GroupRules> GroupRules { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductSpec> ProductSpecs { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<ShipCompany> ShipCompanies { get; set; }
    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<ShipmentTrack> ShipmentTracks { get; set; }
    public DbSet<Refund> Refunds { get; set; }
    public DbSet<FinancialReceivable> FinancialReceivables { get; set; }
    public DbSet<FinancialPayable> FinancialPayables { get; set; }
    public DbSet<FinancialFee> FinancialFees { get; set; }
    public DbSet<FinancialSettlement> FinancialSettlements { get; set; }
    public DbSet<TertiaryHospital> TertiaryHospitals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置索引
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Consultation>()
            .HasIndex(c => c.DoctorId);


        // 配置关系
        modelBuilder.Entity<Patient>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Patient>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        modelBuilder.Entity<Patient>()
            .HasMany(p => p.FamilyMembers)
            .WithOne(fm => fm.Patient)
            .HasForeignKey(fm => fm.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Patient ↔ MedicalRecord 一对多
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(mr => mr.Patient)
            .WithMany()
            .HasForeignKey(mr => mr.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Doctor ↔ MedicalRecord 一对多（可选）
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(mr => mr.Doctor)
            .WithMany()
            .HasForeignKey(mr => mr.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        // 配置 Relationship 枚举存储为整数
        modelBuilder.Entity<FamilyMember>()
            .Property(fm => fm.Relationship)
            .HasConversion<int>();

        // 多对多关系：Doctor ↔ Patient
        modelBuilder.Entity<DoctorPatient>()
            .HasIndex(dp => new { dp.DoctorId, dp.PatientId })
            .IsUnique();

        modelBuilder.Entity<DoctorPatient>()
            .HasOne(dp => dp.Doctor)
            .WithMany(d => d.DoctorPatients)
            .HasForeignKey(dp => dp.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DoctorPatient>()
            .HasOne(dp => dp.Patient)
            .WithMany(p => p.DoctorPatients)
            .HasForeignKey(dp => dp.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // 多对多关系：Consultation ↔ Patient (通过 ConsultationPatient)
        modelBuilder.Entity<ConsultationPatient>()
            .HasIndex(cp => new { cp.ConsultationId, cp.PatientId })
            .IsUnique();

        modelBuilder.Entity<ConsultationPatient>()
            .HasOne(cp => cp.Consultation)
            .WithMany(c => c.ConsultationPatients)
            .HasForeignKey(cp => cp.ConsultationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConsultationPatient>()
            .HasOne(cp => cp.Patient)
            .WithMany()
            .HasForeignKey(cp => cp.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // 一对多关系：Patient → Question
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Patient)
            .WithMany(p => p.Questions)
            .HasForeignKey(q => q.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // 一对多关系：Patient → Answer
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Answers)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Doctor>()
            .HasMany(d => d.Schedules)
            .WithOne(s => s.Doctor)
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Consultation>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Consultation)
            .HasForeignKey(m => m.ConsultationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Question>()
            .HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);


        modelBuilder.Entity<DoctorReview>()
            .HasOne(dr => dr.Patient)
            .WithMany(p => p.DoctorReviews)
            .HasForeignKey(dr => dr.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // 一对多关系：Patient → VisitRecord
        modelBuilder.Entity<VisitRecord>()
            .HasOne(v => v.Patient)
            .WithMany(p => p.VisitRecords)
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VisitRecord>()
            .HasOne(v => v.Doctor)
            .WithMany()
            .HasForeignKey(v => v.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // 一对多关系：Patient → ExaminationReport
        modelBuilder.Entity<ExaminationReport>()
            .HasOne(e => e.Patient)
            .WithMany(p => p.ExaminationReports)
            .HasForeignKey(e => e.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ExaminationReport>()
            .HasOne(e => e.Doctor)
            .WithMany()
            .HasForeignKey(e => e.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
        // 一对多关系：Patient → Prescription
        modelBuilder.Entity<Prescription>()
            .HasOne(p => p.Patient)
            .WithMany(pat => pat.Prescriptions)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
        // 开始配置 Prescription 实体与 Doctor 实体之间的关系
        modelBuilder.Entity<Prescription>()
            // 指定一个 Prescription (处方) 实体关联到 一个 Doctor (医生) 实体
            // p => p.Doctor 表示 Prescription 类中有一个名为 Doctor 的导航属性
            .HasOne(p => p.Doctor)
            // 指定一个 Doctor (医生) 实体可以关联到 多个 Prescription (处方) 实体
            // 注意：这里使用无参的 WithMany()，表示不关心 Doctor 实体中是否有反向导航属性集合
            // （即 Doctor 类中可能没有 Prescriptions 这样的集合属性，或者我们不打算在模型中映射它）
            .WithMany()
            // 指定 Prescription 实体中的 DoctorId 属性作为外键，关联到 Doctor 实体的主键
            .HasForeignKey(p => p.DoctorId)
            // 设置删除行为为 Restrict（限制）：当尝试删除一个 Doctor 时，如果该医生有关联的 Prescription，
            // 则禁止删除该医生，以防止出现孤儿记录（外键约束违规）
            .OnDelete(DeleteBehavior.Restrict);

        // 角色和权限关系配置
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Code)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RolePermission>()
            .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique();

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        // 权限类型字典
        modelBuilder.Entity<PermissionTypeDictionary>()
            .HasIndex(pt => pt.Code)
            .IsUnique();

        // 菜单权限配置
        // MenuKey 和 RoleCode 的组合唯一索引（同一菜单键可以对应多个角色）
        modelBuilder.Entity<MenuPermission>()
            .HasIndex(mp => new { mp.MenuKey, mp.RoleCode })
            .IsUnique();

        modelBuilder.Entity<MenuPermission>()
            .HasIndex(mp => mp.RoleCode);

        // 药品分类自引用关系
        modelBuilder.Entity<DrugCategory>()
            .HasIndex(c => c.CategoryName)
            .IsUnique();

        modelBuilder.Entity<DrugCategory>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DrugCategory>()
            .HasIndex(c => c.ParentId);

        // 药品信息
        modelBuilder.Entity<Drug>()
            .HasIndex(d => d.ApprovalNumber)
            .IsUnique();

        modelBuilder.Entity<Drug>()
            .HasIndex(d => d.CommonName);

        modelBuilder.Entity<Drug>()
            .HasIndex(d => d.Manufacturer);

        modelBuilder.Entity<Drug>()
            .HasOne(d => d.Category)
            .WithMany(c => c.Drugs)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // 药品库存
        modelBuilder.Entity<DrugInventory>()
            .HasIndex(i => new { i.DrugId, i.WarehouseLocation })
            .IsUnique();

        modelBuilder.Entity<DrugInventory>()
            .HasIndex(i => i.DrugId);

        modelBuilder.Entity<DrugInventory>()
            .HasOne(i => i.Drug)
            .WithMany(d => d.Inventories)
            .HasForeignKey(i => i.DrugId)
            .OnDelete(DeleteBehavior.Restrict);

        // 药品入库单头
        modelBuilder.Entity<DrugStockInHead>()
            .HasIndex(h => h.InvoiceNo)
            .IsUnique();

        modelBuilder.Entity<DrugStockInHead>()
            .HasOne(h => h.Operator)
            .WithMany()
            .HasForeignKey(h => h.OperatorId)
            .OnDelete(DeleteBehavior.Restrict);

        // 药品入库单行
        modelBuilder.Entity<DrugStockInLine>()
            .HasIndex(l => l.HeadId);

        modelBuilder.Entity<DrugStockInLine>()
            .HasIndex(l => l.DrugId);

        modelBuilder.Entity<DrugStockInLine>()
            .HasOne(l => l.Head)
            .WithMany(h => h.Lines)
            .HasForeignKey(l => l.HeadId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DrugStockInLine>()
            .HasOne(l => l.Drug)
            .WithMany(d => d.StockInLines)
            .HasForeignKey(l => l.DrugId)
            .OnDelete(DeleteBehavior.Restrict);

        // 用户健康知识收藏
        modelBuilder.Entity<UserHealthKnowledgeFavorite>()
            .HasIndex(f => new { f.UserId, f.HealthKnowledgeId })
            .IsUnique();

        modelBuilder.Entity<UserHealthKnowledgeFavorite>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserHealthKnowledgeFavorite>()
            .HasOne(f => f.HealthKnowledge)
            .WithMany()
            .HasForeignKey(f => f.HealthKnowledgeId)
            .OnDelete(DeleteBehavior.Cascade);

        // 用户-医生订阅关系（多对多）
        modelBuilder.Entity<UserDoctorSubscription>()
            .HasIndex(s => new { s.UserId, s.DoctorId })
            .IsUnique();

        modelBuilder.Entity<UserDoctorSubscription>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserDoctorSubscription>()
            .HasOne(s => s.Doctor)
            .WithMany()
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // 患友会相关实体配置
        modelBuilder.Entity<PatientSupportGroup>()
            .HasOne(g => g.Doctor)
            .WithMany(d => d.PatientSupportGroups)
            .HasForeignKey(g => g.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PatientSupportGroup>()
            .HasOne(g => g.Rules)
            .WithOne(r => r.PatientSupportGroup)
            .HasForeignKey<GroupRules>(r => r.PatientSupportGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasOne(p => p.PatientSupportGroup)
            .WithMany(g => g.Posts)
            .HasForeignKey(p => p.PatientSupportGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PostComment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostComment>()
            .HasOne(c => c.User)
            .WithMany(u => u.PostComments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PostComment>()
            .HasOne(c => c.ParentComment)
            .WithMany(pc => pc.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PostLike>()
            .HasIndex(l => new { l.PostId, l.UserId })
            .IsUnique();

        modelBuilder.Entity<PostLike>()
            .HasOne(l => l.Post)
            .WithMany(p => p.Likes)
            .HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PostLike>()
            .HasOne(l => l.User)
            .WithMany(u => u.PostLikes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Feedback实体配置
        modelBuilder.Entity<Feedback>()
            .HasIndex(f => f.Status);

        modelBuilder.Entity<Feedback>()
            .HasIndex(f => f.CreatedAt);

        // 省份和城市配置
        modelBuilder.Entity<Province>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<Province>()
            .HasIndex(p => p.Name);

        modelBuilder.Entity<City>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<City>()
            .HasIndex(c => c.Name);

        modelBuilder.Entity<City>()
            .HasIndex(c => c.ProvinceId);

        modelBuilder.Entity<City>()
            .HasOne(c => c.Province)
            .WithMany(p => p.Cities)
            .HasForeignKey(c => c.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        // 商品分类
        modelBuilder.Entity<ProductCategory>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<ProductCategory>()
            .HasIndex(c => c.Name);

        // 商品
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // 商品规格
        modelBuilder.Entity<ProductSpec>()
            .HasIndex(s => new { s.ProductId, s.SpecName })
            .IsUnique();

        modelBuilder.Entity<ProductSpec>()
            .HasOne(s => s.Product)
            .WithMany(p => p.Specs)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // 购物车
        modelBuilder.Entity<Cart>()
            .HasIndex(c => c.UserId);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.ProductSpec)
            .WithMany(s => s.CartItems)
            .HasForeignKey(ci => ci.ProductSpecId)
            .OnDelete(DeleteBehavior.Restrict);

        // 收货地址
        modelBuilder.Entity<UserAddress>()
            .HasIndex(a => a.UserId);

        // 订单
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNo)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Payments)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Shipments)
            .WithOne(s => s.Order)
            .HasForeignKey(s => s.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Refunds)
            .WithOne(r => r.Order)
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // 订单明细
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.ProductSpec)
            .WithMany(s => s.OrderItems)
            .HasForeignKey(oi => oi.ProductSpecId)
            .OnDelete(DeleteBehavior.Restrict);

        // 支付
        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.OrderId);

        // 物流公司
        modelBuilder.Entity<ShipCompany>()
            .HasIndex(c => c.Code)
            .IsUnique();

        // 发货单
        modelBuilder.Entity<Shipment>()
            .HasIndex(s => s.TrackingNo);

        modelBuilder.Entity<Shipment>()
            .HasOne(s => s.ShipCompany)
            .WithMany(c => c.Shipments)
            .HasForeignKey(s => s.ShipCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // 物流轨迹
        modelBuilder.Entity<ShipmentTrack>()
            .HasIndex(t => t.ShipmentId);

        modelBuilder.Entity<ShipmentTrack>()
            .HasOne(t => t.Shipment)
            .WithMany(s => s.Tracks)
            .HasForeignKey(t => t.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // 退款
        modelBuilder.Entity<Refund>()
            .HasIndex(r => r.OrderId);

        modelBuilder.Entity<Refund>()
            .HasOne(r => r.OrderItem)
            .WithMany()
            .HasForeignKey(r => r.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // 三甲医院
        modelBuilder.Entity<TertiaryHospital>()
            .HasIndex(h => h.Name);

        modelBuilder.Entity<TertiaryHospital>()
            .HasIndex(h => h.ProvinceId);

        modelBuilder.Entity<TertiaryHospital>()
            .HasIndex(h => h.CityId);

        modelBuilder.Entity<TertiaryHospital>()
            .HasIndex(h => new { h.ProvinceId, h.CityId });

        modelBuilder.Entity<TertiaryHospital>()
            .HasOne(h => h.Province)
            .WithMany()
            .HasForeignKey(h => h.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TertiaryHospital>()
            .HasOne(h => h.City)
            .WithMany()
            .HasForeignKey(h => h.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


# 医疗咨询系统 API

基于 ASP.NET Core 9.0 和 EF Core 9.0.10 构建的 RESTful Web API，使用 PostgreSQL 18 作为数据库。

## 功能特性

- ✅ RESTful API 设计
- ✅ Swagger/OpenAPI 文档
- ✅ JWT 身份认证和授权
- ✅ 数据加密存储（敏感信息）
- ✅ 基于角色的权限控制
- ✅ EF Core 9.0.10 数据访问
- ✅ PostgreSQL 18 数据库支持

## 技术栈

- **框架**: ASP.NET Core 9.0
- **ORM**: Entity Framework Core 9.0.10
- **数据库**: PostgreSQL 18
- **认证**: JWT Bearer Token
- **加密**: AES 加密 + BCrypt 密码哈希
- **API 文档**: Swagger/OpenAPI

## 数据库模型

根据 Medical.UI 的 HTML 文件设计，包含以下主要实体：

- **User** - 用户（患者、医生、管理员）
- **Doctor** - 医生信息
- **Department** - 科室
- **DiseaseCategory** - 疾病分类
- **Consultation** - 咨询订单
- **ConsultationMessage** - 咨询消息
- **HealthRecord** - 健康记录
- **HealthReport** - 健康报告
- **HealthKnowledge** - 健康知识文章
- **Question** - 问题
- **Answer** - 回答
- **Activity** - 活动
- **DoctorReview** - 医生评价
- **Prescription** - 处方
- **Medicine** - 药品
- **FamilyMember** - 家庭成员

## 配置说明

### 1. 数据库配置

在 `appsettings.json` 中配置 PostgreSQL 连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MedicalDB;Username=postgres;Password=postgres"
  }
}
```

### 2. JWT 配置

```json
{
  "Jwt": {
    "SecretKey": "你的密钥（至少32个字符）",
    "Issuer": "MedicalAPI",
    "Audience": "MedicalClient",
    "ExpirationMinutes": "1440"
  }
}
```

### 3. 加密配置

```json
{
  "Encryption": {
    "Key": "你的加密密钥（至少32个字符）"
  }
}
```

## API 端点

### 认证相关

- `POST /api/auth/register` - 用户注册
- `POST /api/auth/login` - 用户登录

### 医生相关

- `GET /api/doctors` - 获取医生列表
- `GET /api/doctors/{id}` - 获取医生详情
- `GET /api/doctors/search?keyword={keyword}` - 搜索医生

### 科室相关

- `GET /api/departments` - 获取所有科室
- `GET /api/departments/{id}` - 获取科室详情
- `GET /api/departments/{id}/diseases` - 获取科室的疾病分类

### 健康知识

- `GET /api/healthknowledge` - 获取健康知识列表
- `GET /api/healthknowledge/{id}` - 获取健康知识详情
- `GET /api/healthknowledge/search?keyword={keyword}` - 搜索健康知识

### 问题与回答

- `GET /api/questions` - 获取问题列表
- `GET /api/questions/{id}` - 获取问题详情
- `POST /api/questions` - 创建问题（需要认证）
- `POST /api/questions/{questionId}/answers` - 回答问题（需要认证）

### 咨询相关

- `GET /api/consultations` - 获取用户的咨询列表（需要认证）
- `GET /api/consultations/{id}` - 获取咨询详情（需要认证）
- `POST /api/consultations` - 创建咨询（需要认证）
- `POST /api/consultations/{consultationId}/messages` - 发送咨询消息（需要认证）

### 健康记录

- `GET /api/healthrecords` - 获取健康记录（需要认证）
- `POST /api/healthrecords` - 创建健康记录（需要认证）
- `GET /api/healthrecords/reports` - 获取健康报告（需要认证）

### 活动

- `GET /api/activities` - 获取活动列表
- `GET /api/activities/{id}` - 获取活动详情

## 使用示例

### 1. 用户注册

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "password": "password123",
  "phoneNumber": "13800138000",
  "email": "test@example.com",
  "role": "Patient"
}
```

### 2. 用户登录

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "password123"
}
```

响应：
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "guid",
    "username": "testuser",
    "role": "Patient"
  }
}
```

### 3. 使用 Token 访问受保护的 API

```http
GET /api/consultations
Authorization: Bearer {token}
```

## 安全特性

1. **密码加密**: 使用 BCrypt 进行密码哈希
2. **敏感数据加密**: 手机号、邮箱、身份证等敏感信息使用 AES 加密存储
3. **JWT 认证**: 所有受保护的 API 都需要有效的 JWT Token
4. **基于角色的授权**: 支持 Patient、Doctor、Admin 三种角色
5. **数据访问控制**: 用户只能访问自己的数据

## 运行项目

1. 确保已安装 PostgreSQL 18
2. 创建数据库：
   ```sql
   CREATE DATABASE MedicalDB;
   ```
3. 更新 `appsettings.json` 中的连接字符串
4. 运行项目：
   ```bash
   dotnet run
   ```
5. 访问 Swagger UI: `https://localhost:5001` 或 `http://localhost:5000`

## 数据库迁移

如果需要使用 EF Core Migrations：

```bash
# 安装 EF Core 工具（如果未安装）
dotnet tool install --global dotnet-ef
dotnet tool install --global dotnet-ef --version 9.0.0
# 创建迁移
dotnet ef migrations add InitialCreate

# 应用迁移
dotnet ef database update
```

## 注意事项

1. 生产环境请修改默认的 JWT SecretKey 和 Encryption Key
2. 建议使用环境变量或密钥管理服务存储敏感配置
3. 数据库连接字符串应使用安全的凭据
4. 生产环境应禁用 CORS 的 AllowAll 策略，配置具体的允许来源

## 开发团队

医疗系统开发团队


两种数据库管理方式
    方式1：使用 EnsureCreated()（当前项目使用）
        位置：Program.cs 第 260 行
        工作原理：程序启动时自动检查数据库是否存在，不存在则根据实体模型创建所有表
    优点：
        简单，无需迁移命令
        适合开发环境
    缺点：
        不会更新已有数据库的结构
        如果数据库已存在但缺少新表，需要手动删除数据库或手动添加表
        使用方式：直接运行程序即可，不需要执行 dotnet ef database update 等迁移命令

方式2：使用迁移（Migration）执行数据库迁移
    请打开 Visual Studio的“程序包管理器控制台”（可以通过 工具 -> NuGet包管理器 -> 程序包管理器控制台 打开）
    # 1. 生成初始迁移文件（创建施工图纸）
    Add-Migration InitialCreate（InitialCreate 随便起名，以后修改了什么属性可以用属性名）
    # 2. 在数据库中创建所有表（按图纸施工）
    Update-Database

    后续修改（比如给 User 表增加一个 PhoneNumber 字段）
    # 1. 生成新增字段的迁移文件（生成增建图纸）
    Add-Migration MedicalRecord ()

    # 2. 执行变更，为已存在的User表添加PhoneNumber列（进行增建施工）
    Update-Database


    1. Add-Migration ExtendedFeatures
    使用环境：Visual Studio 的 Package Manager Console
    命令类型：PowerShell cmdlet（Entity Framework Core Tools）
    前提：在 Visual Studio 中打开项目，并在 Package Manager Console 中执行
    优点：
    自动识别项目上下文
    无需指定项目路径
    集成在 IDE 中
    2. dotnet ef migrations add ExtendedFeatures
    使用环境：命令行（PowerShell、CMD、Terminal、VS Code 终端等）
    命令类型：.NET CLI 命令
    前提：已安装 dotnet-ef 工具
    优点：
    跨平台（Windows、Linux、macOS）
    不依赖 Visual Studio
    适合 CI/CD 和自动化脚本

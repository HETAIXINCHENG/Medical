# 权限标识添加完成情况

## 已完成的 Controller

### 1. UsersController ✅
- `[RequirePermission("user.view")]` - GET /api/users
- `[RequirePermission("user.view")]` - GET /api/users/{id}
- `[RequirePermission("user.create")]` - POST /api/users
- `[RequirePermission("user.update")]` - PUT /api/users/{id}
- `[RequirePermission("user.delete")]` - DELETE /api/users/{id}

### 2. RolesController ✅
- `[RequirePermission("role.view")]` - GET /api/roles
- `[RequirePermission("role.view")]` - GET /api/roles/all
- `[RequirePermission("role.view")]` - GET /api/roles/{id}
- `[RequirePermission("role.view")]` - GET /api/roles/{id}/permissions
- `[RequirePermission("role.create")]` - POST /api/roles
- `[RequirePermission("role.update")]` - PUT /api/roles/{id}
- `[RequirePermission("role.update")]` - POST /api/roles/{id}/permissions
- `[RequirePermission("role.delete")]` - DELETE /api/roles/{id}

### 3. PermissionsController ✅
- `[RequirePermission("permission.view")]` - GET /api/permissions
- `[RequirePermission("permission.view")]` - GET /api/permissions/all
- `[RequirePermission("permission.view")]` - GET /api/permissions/{id}
- `[RequirePermission("permission.create")]` - POST /api/permissions
- `[RequirePermission("permission.update")]` - PUT /api/permissions/{id}
- `[RequirePermission("permission.delete")]` - DELETE /api/permissions/{id}

### 4. DoctorsController ✅
- `[RequirePermission("doctor.view")]` - GET /api/doctors
- `[RequirePermission("doctor.view")]` - GET /api/doctors/{id}
- `[RequirePermission("doctor.view")]` - GET /api/doctors/search
- `[RequirePermission("doctor.create")]` - POST /api/doctors
- `[RequirePermission("doctor.update")]` - PUT /api/doctors/{id}
- `[RequirePermission("doctor.delete")]` - DELETE /api/doctors/{id}

### 5. PatientsController ✅
- `[RequirePermission("patient.view")]` - GET /api/patients
- `[RequirePermission("patient.view")]` - GET /api/patients/{id}
- `[RequirePermission("patient.view")]` - GET /api/patients/by-user/{userId}
- `[RequirePermission("patient.create")]` - POST /api/patients
- `[RequirePermission("patient.update")]` - PUT /api/patients/{id}
- `[RequirePermission("patient.delete")]` - DELETE /api/patients/{id}

### 6. ConsultationsController ✅
- `[RequirePermission("consultation.view")]` - GET /api/consultations
- `[RequirePermission("consultation.view")]` - GET /api/consultations/{id}
- `[RequirePermission("consultation.create")]` - POST /api/consultations
- `[RequirePermission("consultation.update")]` - PUT /api/consultations/{id}
- `[RequirePermission("consultation.delete")]` - DELETE /api/consultations/{id}

### 7. MenuPermissionsController ✅
- `[RequirePermission("menupermission.view")]` - GET /api/menupermissions
- `[RequirePermission("menupermission.create")]` - POST /api/menupermissions
- `[RequirePermission("menupermission.delete")]` - DELETE /api/menupermissions/{id}

### 8. PermissionTypeDictionariesController ✅
- `[RequirePermission("permissiontype.view")]` - GET /api/permissiontypedictionaries
- `[RequirePermission("permissiontype.view")]` - GET /api/permissiontypedictionaries/all

### 9. DashboardController ✅
- `[RequirePermission("dashboard.view")]` - GET /api/dashboard/patient-count
- `[RequirePermission("dashboard.view")]` - GET /api/dashboard/department-patient-distribution
- `[RequirePermission("dashboard.view")]` - GET /api/dashboard/doctor-consultation-ranking
- `[RequirePermission("dashboard.view")]` - GET /api/dashboard/doctor-activity-ranking

### 10. HealthRecordsController ✅
- `[RequirePermission("healthrecord.view")]` - GET /api/healthrecords
- `[RequirePermission("healthrecord.view")]` - GET /api/healthrecords/reports
- `[RequirePermission("healthrecord.create")]` - POST /api/healthrecords
- `[RequirePermission("healthrecord.update")]` - PUT /api/healthrecords/{id}
- `[RequirePermission("healthrecord.delete")]` - DELETE /api/healthrecords/{id}

### 11. HealthKnowledgeController ✅
- `[RequirePermission("healthknowledge.view")]` - GET /api/healthknowledge
- `[RequirePermission("healthknowledge.view")]` - GET /api/healthknowledge/{id}
- `[RequirePermission("healthknowledge.view")]` - GET /api/healthknowledge/search
- `[RequirePermission("healthknowledge.create")]` - POST /api/healthknowledge
- `[RequirePermission("healthknowledge.update")]` - PUT /api/healthknowledge/{id}
- `[RequirePermission("healthknowledge.delete")]` - DELETE /api/healthknowledge/{id}

### 12. QuestionsController ✅
- `[RequirePermission("question.create")]` - POST /api/questions
- `[RequirePermission("question.update")]` - PUT /api/questions/{id}
- `[RequirePermission("question.delete")]` - DELETE /api/questions/{id}

### 13. ActivitiesController ✅
- `[RequirePermission("activity.view")]` - GET /api/activities
- `[RequirePermission("activity.view")]` - GET /api/activities/{id}
- `[RequirePermission("activity.create")]` - POST /api/activities
- `[RequirePermission("activity.update")]` - PUT /api/activities/{id}
- `[RequirePermission("activity.delete")]` - DELETE /api/activities/{id}

### 14. DepartmentsController ✅
- `[RequirePermission("department.view")]` - GET /api/departments
- `[RequirePermission("department.view")]` - GET /api/departments/{id}
- `[RequirePermission("department.view")]` - GET /api/departments/{id}/diseases
- `[RequirePermission("department.create")]` - POST /api/departments
- `[RequirePermission("department.update")]` - PUT /api/departments/{id}
- `[RequirePermission("department.delete")]` - DELETE /api/departments/{id}

### 15. DrugsController ✅
- `[RequirePermission("drug.view")]` - GET /api/drugs
- `[RequirePermission("drug.view")]` - GET /api/drugs/{id}
- `[RequirePermission("drug.create")]` - POST /api/drugs
- `[RequirePermission("drug.update")]` - PUT /api/drugs/{id}
- `[RequirePermission("drug.delete")]` - DELETE /api/drugs/{id}

### 16. DrugCategoriesController ✅
- `[RequirePermission("drugcategory.view")]` - GET /api/drugcategories
- `[RequirePermission("drugcategory.view")]` - GET /api/drugcategories/tree
- `[RequirePermission("drugcategory.view")]` - GET /api/drugcategories/{id}
- `[RequirePermission("drugcategory.create")]` - POST /api/drugcategories
- `[RequirePermission("drugcategory.update")]` - PUT /api/drugcategories/{id}
- `[RequirePermission("drugcategory.delete")]` - DELETE /api/drugcategories/{id}

### 17. DrugInventoriesController ✅
- `[RequirePermission("druginventory.view")]` - GET /api/druginventories

### 18. DrugStockInsController ✅
- `[RequirePermission("drugstockin.view")]` - GET /api/drugstockins
- `[RequirePermission("drugstockin.view")]` - GET /api/drugstockins/{id}
- `[RequirePermission("drugstockin.create")]` - POST /api/drugstockins

### 19. PrescriptionsController ✅
- `[RequirePermission("prescription.view")]` - GET /api/prescriptions
- `[RequirePermission("prescription.view")]` - GET /api/prescriptions/{id}
- `[RequirePermission("prescription.create")]` - POST /api/prescriptions
- `[RequirePermission("prescription.update")]` - PUT /api/prescriptions/{id}
- `[RequirePermission("prescription.delete")]` - DELETE /api/prescriptions/{id}

### 20. MedicinesController ✅
- `[RequirePermission("medicine.view")]` - GET /api/medicines
- `[RequirePermission("medicine.view")]` - GET /api/medicines/{id}
- `[RequirePermission("medicine.create")]` - POST /api/medicines
- `[RequirePermission("medicine.update")]` - PUT /api/medicines/{id}
- `[RequirePermission("medicine.delete")]` - DELETE /api/medicines/{id}

### 21. ConsultationMessagesController ✅
- `[RequirePermission("consultationmessage.view")]` - GET /api/consultationmessages
- `[RequirePermission("consultationmessage.view")]` - GET /api/consultationmessages/{id}
- `[RequirePermission("consultationmessage.create")]` - POST /api/consultationmessages
- `[RequirePermission("consultationmessage.update")]` - PUT /api/consultationmessages/{id}
- `[RequirePermission("consultationmessage.delete")]` - DELETE /api/consultationmessages/{id}

### 22. ExaminationReportsController ✅
- `[RequirePermission("examinationreport.view")]` - GET /api/examinationreports
- `[RequirePermission("examinationreport.view")]` - GET /api/examinationreports/{id}
- `[RequirePermission("examinationreport.create")]` - POST /api/examinationreports
- `[RequirePermission("examinationreport.update")]` - PUT /api/examinationreports/{id}
- `[RequirePermission("examinationreport.delete")]` - DELETE /api/examinationreports/{id}

### 23. VisitRecordsController ✅
- `[RequirePermission("visitrecord.view")]` - GET /api/visitrecords
- `[RequirePermission("visitrecord.view")]` - GET /api/visitrecords/{id}
- `[RequirePermission("visitrecord.create")]` - POST /api/visitrecords
- `[RequirePermission("visitrecord.update")]` - PUT /api/visitrecords/{id}
- `[RequirePermission("visitrecord.delete")]` - DELETE /api/visitrecords/{id}

### 24. UploadController ✅
- `[RequirePermission("upload.create")]` - POST /api/upload/image

## 不需要权限控制的 Controller

以下 Controller 是公开接口，不需要权限控制：

1. **AuthController** - 用户注册和登录（公开接口）
2. **AdminAuthController** - 管理员登录（公开接口）

## 权限代码命名规范

- 资源名称：从 Controller 名称推断（去掉 Controller 后缀，转小写）
- 操作类型：
  - `view` - 查看（GET）
  - `create` - 创建（POST）
  - `update` - 更新（PUT）
  - `delete` - 删除（DELETE）

## 使用示例

```csharp
[HttpGet]
[RequirePermission("resource.view")]
public async Task<ActionResult> GetResources() { }

[HttpPost]
[RequirePermission("resource.create")]
public async Task<ActionResult> CreateResource() { }

[HttpPut("{id}")]
[RequirePermission("resource.update")]
public async Task<ActionResult> UpdateResource(Guid id) { }

[HttpDelete("{id}")]
[RequirePermission("resource.delete")]
public async Task<ActionResult> DeleteResource(Guid id) { }
```


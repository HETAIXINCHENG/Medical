import React from 'react';

const resourceConfig = [
  {
    key: 'users',
    title: '用户管理',
    basePath: '/api/users',
    primaryKey: 'id',
    columns: [
      { title: '用户名', dataIndex: 'username' },
      { title: '角色', dataIndex: 'role' },
      { title: '邮箱', dataIndex: 'email' },
      { title: '手机号', dataIndex: 'phoneNumber' },
      { title: '状态', dataIndex: 'isActive', valueType: 'boolean' }
    ],
    formFields: [
      { name: 'username', label: '用户名', component: 'input', rules: [{ required: true }] },
      { name: 'password', label: '密码', component: 'password', placeholder: '新建时填写' },
      {
        name: 'role',
        label: '角色',
        component: 'select',
        options: [
          { label: '患者', value: 'Patient' },
          { label: '医生', value: 'Doctor' },
          { label: '管理员', value: 'Admin' }
        ],
        rules: [{ required: true }]
      },
      { name: 'email', label: '邮箱', component: 'input' },
      { name: 'phoneNumber', label: '手机号', component: 'input' },
      { name: 'isActive', label: '启用', component: 'switch', initialValue: true }
    ]
  },
  // 患者信息（显示患者记录）
  {
    key: 'patients',
    title: '患者信息',
    basePath: '/api/patients',
    primaryKey: 'id',
    columns: [
      { title: '用户名', dataIndex: ['user', 'username'], render: (_, record) => record?.user?.username || '-' },
      { title: '真实姓名', dataIndex: 'realName', render: (text) => text || '-' },
      { title: '性别', dataIndex: 'gender', render: (text) => text || '-' },
      { title: '手机号', dataIndex: 'phoneNumber', render: (text) => text || '-' },
      { title: '邮箱', dataIndex: 'email', render: (text) => text || '-' }
    ],
    formFields: [
      { 
        name: 'patientId', 
        label: '用户', 
        component: 'select', 
        rules: [{ required: true, message: '请选择用户' }], 
        options: [], // 动态加载
        loadOptionsFrom: '/api/users', // 指定加载选项的API
        loadOptionsParams: { role: 'Patient' }, // 只加载患者角色的用户
        componentProps: { placeholder: '请选择用户' },
        span: 24 
      },
      { name: 'realName', label: '真实姓名', component: 'input', rules: [{ required: true }] },
      { 
        name: 'gender', 
        label: '性别', 
        component: 'select', 
        options: [
          { label: '男', value: 'Male' },
          { label: '女', value: 'Female' }
        ],
        span: 12 
      },
      { name: 'dateOfBirth', label: '出生日期', component: 'date', span: 12 },
      { name: 'phoneNumber', label: '手机号', component: 'input', span: 12 },
      { name: 'email', label: '邮箱', component: 'input', span: 12 },
      { name: 'idCardNumber', label: '身份证号', component: 'input', span: 24 },
      { name: 'address', label: '地址', component: 'input', span: 24 }
    ]
  },
  // 患者信息库（存储患者详细信息和过敏史）
  {
    key: 'patient-info',
    title: '患者信息库',
    basePath: '/api/patients',
    primaryKey: 'id',
    columns: [
      { title: 'resource.patient-info.column.username', dataIndex: ['user', 'username'], render: (_, record) => record?.user?.username || '-' },
      { title: 'resource.patient-info.column.realName', dataIndex: 'realName', render: (text) => text || '-' },
      { title: 'resource.patient-info.column.gender', dataIndex: 'gender', render: (text) => text || '-' },
      { title: 'resource.patient-info.column.dateOfBirth', dataIndex: 'dateOfBirth', render: (value) => value ? new Date(value).toLocaleDateString('zh-CN') : '-' },
      { title: 'resource.patient-info.column.idCardNumber', dataIndex: 'idCardNumber', render: (text) => text || '-' },
      { title: 'resource.patient-info.column.phoneNumber', dataIndex: 'phoneNumber', render: (text) => text || '-' },
      { title: 'resource.patient-info.column.email', dataIndex: 'email', render: (text) => text || '-' },
      { title: 'resource.patient-info.column.bloodType', dataIndex: 'bloodType', render: (text) => text || '-' },
      { title: 'resource.patient-info.column.allergyHistory', dataIndex: 'allergyHistory', ellipsis: true, width: 150, render: (text) => text || '-' }
    ],
    formFields: [
      { 
        name: 'id', 
        label: '', 
        component: 'input', 
        componentProps: { style: { display: 'none' } },
        span: 0 
      },
      { 
        name: 'username', 
        label: 'resource.patient-info.field.username.label', 
        component: 'input', 
        rules: [{ required: true, message: 'resource.patient-info.field.username.rule.required' }], 
        componentProps: { 
          placeholder: 'resource.patient-info.field.username.placeholder',
          readOnly: false // 新建时可编辑，编辑时通过代码设置为只读
        },
        span: 12 
      },
      { 
        name: 'patientId', 
        label: '', 
        component: 'input', 
        componentProps: { style: { display: 'none' } },
        span: 0 
      },
      { 
        name: 'realName', 
        label: '真实姓名', 
        component: 'input', 
        rules: [{ required: true, message: '请输入真实姓名' }],
        span: 12 
      },
      { 
        name: 'dateOfBirth', 
        label: '出生日期', 
        component: 'date',
        componentProps: { style: { width: '100%' } },
        span: 12 
      },
      { 
        name: 'gender', 
        label: '性别', 
        component: 'select',
        options: [
          { label: '男', value: 'Male' },
          { label: '女', value: 'Female' },
          { label: '其他', value: 'Other' }
        ],
        componentProps: { placeholder: '请选择性别' },
        span: 12 
      },
      { 
        name: 'idCardNumber', 
        label: 'resource.patient-info.field.idCardNumber.label', 
        component: 'input',
        componentProps: { placeholder: 'resource.patient-info.field.idCardNumber.placeholder' },
        rules: [
          {
            pattern: /^[1-9]\d{5}(18|19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}[\dXx]$/,
            message: 'resource.patient-info.field.idCardNumber.rule.pattern'
          }
        ],
        span: 12 
      },
      { 
        name: 'phoneNumber', 
        label: '手机号', 
        component: 'input',
        rules: [
          {
            pattern: /^1[3-9]\d{9}$/,
            message: '请输入正确的手机号码（11位，以1开头）'
          }
        ],
        span: 12 
      },
      { 
        name: 'email', 
        label: '邮箱', 
        component: 'input',
        span: 24 
      },
      { 
        name: 'address', 
        label: '地址', 
        component: 'textarea',
        componentProps: { rows: 2 },
        span: 24 
      },
      { 
        name: 'bloodType', 
        label: '血型', 
        component: 'select',
        options: [
          { label: 'A型', value: 'A' },
          { label: 'B型', value: 'B' },
          { label: 'AB型', value: 'AB' },
          { label: 'O型', value: 'O' },
          { label: '未知', value: 'Unknown' }
        ],
        componentProps: { placeholder: '请选择血型' },
        span: 12 
      },
      { 
        name: 'height', 
        label: '身高(cm)', 
        component: 'number',
        componentProps: { style: { width: '100%' }, precision: 1 },
        span: 12 
      },
      { 
        name: 'weight', 
        label: '体重(kg)', 
        component: 'number',
        componentProps: { style: { width: '100%' }, precision: 1 },
        span: 12 
      },
      { 
        name: 'bmi', 
        label: 'BMI', 
        component: 'number',
        componentProps: { 
          style: { width: '100%' }, 
          precision: 2,
          readOnly: true,
          disabled: true
        },
        span: 12 
      },
      {
        name: 'allergyHistory',
        label: '过敏史',
        component: 'textarea',
        componentProps: { rows: 4, placeholder: '请输入过敏史，如：青霉素过敏、海鲜过敏等' },
        span: 24
      },
      {
        name: 'medicalHistory',
        label: '既往病史',
        component: 'textarea',
        componentProps: { rows: 4, placeholder: '请输入既往病史' },
        span: 24
      },
      {
        name: 'familyHistory',
        label: '家族病史',
        component: 'textarea',
        componentProps: { rows: 4, placeholder: '请输入家族病史' },
        span: 24
      },
      { 
        name: 'emergencyContactName', 
        label: 'resource.patient-info.field.emergencyContactName.label', 
        component: 'input',
        span: 12 
      },
      { 
        name: 'emergencyContactPhone', 
        label: 'resource.patient-info.field.emergencyContactPhone.label', 
        component: 'input',
        span: 12 
      },
      { 
        name: 'emergencyContactRelation', 
        label: 'resource.patient-info.field.emergencyContactRelation.label', 
        component: 'select',
        options: [
          { label: '配偶', value: 'Spouse' },
          { label: '子女', value: 'Child' },
          { label: '父母', value: 'Parent' },
          { label: '其他', value: 'Other' }
        ],
        componentProps: { placeholder: 'resource.patient-info.field.emergencyContactRelation.placeholder' },
        span: 12 
      },
      { 
        name: 'emergencyContactGender', 
        label: 'resource.patient-info.field.emergencyContactGender.label', 
        component: 'select',
        options: [
          { label: '男', value: 'Male' },
          { label: '女', value: 'Female' },
          { label: '其他', value: 'Other' }
        ],
        componentProps: { placeholder: 'resource.patient-info.field.emergencyContactGender.placeholder' },
        span: 12 
      },
      { 
        name: 'emergencyContactDateOfBirth', 
        label: 'resource.patient-info.field.emergencyContactDateOfBirth.label', 
        component: 'date',
        componentProps: { style: { width: '100%' } },
        span: 12 
      },
      { 
        name: 'emergencyContactIdCardNumber', 
        label: 'resource.patient-info.field.emergencyContactIdCardNumber.label', 
        component: 'input',
        componentProps: { placeholder: 'resource.patient-info.field.emergencyContactIdCardNumber.placeholder' },
        span: 12 
      },
      {
        name: 'notes',
        label: '备注',
        component: 'textarea',
        componentProps: { rows: 3 },
        span: 24
      }
    ]
  },
  {
    key: 'departments',
    title: '科室管理',
    basePath: '/api/departments',
    primaryKey: 'id',
    columns: [
      { title: '科室名称', dataIndex: 'name' },
      { title: '描述', dataIndex: 'description' },
      { title: '排序', dataIndex: 'sortOrder' },
      { title: '热门', dataIndex: 'isHot', valueType: 'boolean' }
    ],
    formFields: [
      { name: 'name', label: '科室名称', component: 'input', rules: [{ required: true }], span: 24 },
      { name: 'description', label: '描述', component: 'textarea', componentProps: { rows: 4 }, span: 24 },
      {
        name: 'sortOrder',
        label: '排序值',
        component: 'number',
        initialValue: 0,
        componentProps: { style: { width: '100%' } },
        span: 24
      },
      { name: 'isHot', label: '热门', component: 'switch', initialValue: false, span: 24 }
    ]
  },
  {
    key: 'doctors',
    title: '医生管理',
    basePath: '/api/doctors',
    primaryKey: 'id',
    columns: [
      { title: '姓名', dataIndex: 'name' },
      { 
        title: '头像', 
        dataIndex: 'avatarUrl',
        width: 80,
        render: (text, record) => {
          const avatarUrl = text || record?.avatarUrl || record?.AvatarUrl || null;
          
          if (!avatarUrl || avatarUrl === null || avatarUrl === undefined || (typeof avatarUrl === 'string' && avatarUrl.trim() === '')) {
            return React.createElement('span', {}, '-');
          }
          
          const baseURL = import.meta?.env?.VITE_API_BASE_URL ?? 'http://localhost:5000';
          let imageUrl;
          
          if (typeof avatarUrl === 'string') {
            if (avatarUrl.startsWith('http://') || avatarUrl.startsWith('https://')) {
              imageUrl = avatarUrl;
            } else if (avatarUrl.startsWith('/')) {
              imageUrl = `${baseURL}${avatarUrl}`;
            } else {
              imageUrl = `${baseURL}/${avatarUrl}`;
            }
          } else {
            return React.createElement('span', {}, '-');
          }
          
          return React.createElement('img', { 
            src: imageUrl, 
            style: { width: 40, height: 40, borderRadius: '50%', objectFit: 'cover', cursor: 'pointer', display: 'block' },
            alt: '头像',
            onClick: () => {
              window.open(imageUrl, '_blank');
            },
            onError: (e) => {
              e.target.style.display = 'none';
              const parent = e.target.parentElement;
              if (parent) {
                if (parent.firstChild) {
                  parent.replaceChild(document.createTextNode('-'), parent.firstChild);
                }
              }
            }
          });
        }
      },
      { title: '职称', dataIndex: 'title' },
      { title: '医院', dataIndex: 'hospital' },
      { title: '科室', dataIndex: 'departmentName' },
      { title: '推荐', dataIndex: 'isRecommended', valueType: 'boolean' }
    ],
    formFields: [
      { name: 'name', label: '姓名', component: 'input', rules: [{ required: true }], span: 12 },
      { name: 'title', label: '职称', component: 'input', rules: [{ required: true }], span: 12 },
      { 
        name: 'hospitalId', 
        label: '医院', 
        component: 'select', 
        loadOptionsFrom: '/api/tertiaryhospitals',
        componentProps: { placeholder: '请选择医院', showSearch: true },
        span: 12 
      },
      { 
        name: 'departmentId', 
        label: '科室', 
        component: 'select', 
        rules: [{ required: true, message: '请选择科室' }],
        options: [], // 动态加载
        loadOptionsFrom: '/api/departments', // 指定加载选项的API
        componentProps: { placeholder: '请选择科室' },
        span: 12
      },
      {
        name: 'avatarUrl',
        label: '头像',
        component: 'upload',
        componentProps: { 
          listType: 'picture-card',
          maxCount: 1,
          multiple: false,
          accept: 'image/*'
        },
        span: 12
      },
      { 
        name: 'specialty', 
        label: '擅长', 
        component: 'textarea', 
        componentProps: { rows: 4, maxLength: 2000, showCount: true },
        rules: [{ max: 2000, message: '擅长领域最多2000个字符' }],
        span: 24
      },
      { 
        name: 'introduction', 
        label: '个人简介', 
        component: 'textarea', 
        componentProps: { rows: 6, maxLength: 6000, showCount: true },
        rules: [{ max: 6000, message: '个人简介最多6000个字符' }],
        span: 24
      },
      { name: 'isRecommended', label: '推荐', component: 'switch', initialValue: false, span: 12 }
    ]
  },
  {
    key: 'patient-support-groups',
    title: '医生的患友会',
    basePath: '/api/patientsupportgroups',
    primaryKey: 'id',
    // 患友会自动生成，不允许新建，但允许编辑
    readOnly: false,
    disableCreate: true,
    columns: [
      { title: '患友会名称', dataIndex: 'name' },
      { 
        title: '医生', 
        dataIndex: 'doctorName',
        render: (text, record) => `${record.doctorName || '-'} ${record.doctorTitle || ''}`.trim()
      },
      { title: '医院', dataIndex: 'doctorHospital' },
      { title: '科室', dataIndex: 'doctorDepartmentName' },
      { title: '发布人数', dataIndex: 'postCount' },
      { title: '阅读总数', dataIndex: 'totalReadCount' },
      { title: '启用状态', dataIndex: 'isEnabled', valueType: 'boolean' },
      { 
        title: '创建时间', 
        dataIndex: 'createdAt', 
        render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' 
      }
    ],
    formFields: [
      { 
        name: 'description', 
        label: '描述', 
        component: 'textarea', 
        componentProps: { rows: 4, maxLength: 1000, showCount: true },
        rules: [{ max: 1000, message: '描述不能超过1000个字符' }],
        span: 24 
      },
      { name: 'isEnabled', label: '启用', component: 'switch', initialValue: true, span: 12 }
    ]
  },
  {
    key: 'activities',
    title: '活动管理',
    basePath: '/api/activities',
    primaryKey: 'id',
    columns: [
      { title: '标题', dataIndex: 'title' },
      { title: '类型', dataIndex: 'activityType' },
      { title: '排序', dataIndex: 'sortOrder' },
      { title: '热门', dataIndex: 'isHot', valueType: 'boolean' }
    ],
    formFields: [
      { name: 'title', label: '标题', component: 'input', rules: [{ required: true }], span: 12 },
      { name: 'subtitle', label: '副标题', component: 'input', span: 12 },
      { 
        name: 'activityType', 
        label: '活动类型', 
        component: 'select', 
        rules: [{ required: true, message: '请选择活动类型' }],
        options: [
          { label: '优惠', value: 'Discount' },
          { label: '知识', value: 'Knowledge' },
          { label: '产品', value: 'Product' }
        ],
        componentProps: { placeholder: '请选择活动类型' },
        span: 12
      },
          {
            name: 'coverImageUrl',
            label: '封面图片',
            component: 'upload',
            componentProps: { 
              listType: 'picture-card',
              maxCount: 1,
              accept: 'image/*'
            },
            span: 12
          },
      { 
        name: 'description', 
        label: '描述', 
        component: 'textarea', 
        componentProps: { rows: 4, maxLength: 1000, showCount: true },
        rules: [{ max: 1000, message: '描述最多1000个字符' }],
        span: 24
      },
      { name: 'discountInfo', label: '优惠信息', component: 'input', span: 12 },
      {
        name: 'sortOrder',
        label: '排序值',
        component: 'number',
        initialValue: 0,
        componentProps: { style: { width: '100%' } },
        span: 12
      },
      { name: 'isHot', label: '热门', component: 'switch', initialValue: false, span: 12 },
      { name: 'isLargeCard', label: '大卡片显示', component: 'switch', initialValue: false, span: 12 },
      { 
        name: 'startTime', 
        label: '开始时间', 
        component: 'date', 
        componentProps: { 
          showTime: true, 
          style: { width: '100%' },
          format: 'YYYY-MM-DD HH:mm:ss'
        },
        span: 12
      },
      { 
        name: 'endTime', 
        label: '结束时间', 
        component: 'date', 
        componentProps: { 
          showTime: true, 
          style: { width: '100%' },
          format: 'YYYY-MM-DD HH:mm:ss'
        },
        span: 12
      }
    ]
  },
  {
    key: 'knowledge',
    title: '健康知识',
    basePath: '/api/healthknowledge',
    primaryKey: 'id',
    columns: [
      { title: '标题', dataIndex: 'title' },
      { title: '分类', dataIndex: 'category' },
      { title: '作者', dataIndex: 'author' },
      { title: '阅读量', dataIndex: 'readCount' },
      { title: '收藏量', dataIndex: 'favoriteCount' },
      { title: '热门', dataIndex: 'isHot', valueType: 'boolean' },
      { title: '推荐', dataIndex: 'isRecommended', valueType: 'boolean' }
    ],
    formFields: [
      { name: 'title', label: '标题', component: 'input', rules: [{ required: true }], span: 12 },
      { 
        name: 'category', 
        label: '分类', 
        component: 'select', 
        options: [
          { label: '热门推荐', value: '热门推荐' },
          { label: '养生课堂', value: '养生课堂' },
          { label: '精彩专题', value: '精彩专题' },
          { label: '专家讲堂', value: '专家讲堂' },
          { label: '健康', value: '健康' }
        ],
        componentProps: { placeholder: '请选择分类' },
        span: 12 
      },
      { 
        name: 'author', 
        label: '作者', 
        component: 'select', 
        options: [], // 动态加载
        loadOptionsFrom: '/api/doctors', // 指定加载选项的API
        componentProps: { placeholder: '请选择医生', showSearch: true },
        span: 12 
      },
      { name: 'source', label: '来源', component: 'input', span: 12 },
      {
        name: 'coverImageUrl',
        label: '封面图片',
        component: 'upload',
        componentProps: { 
          listType: 'picture-card',
          maxCount: 1,
          accept: 'image/*'
        },
        span: 12
      },
      {
        name: 'summary',
        label: '摘要',
        component: 'textarea',
        componentProps: { rows: 3, maxLength: 500, showCount: true },
        rules: [{ max: 500, message: '摘要最多500个字符' }],
        span: 24
      },
      {
        name: 'content',
        label: '内容',
        component: 'textarea',
        componentProps: { rows: 6 },
        rules: [{ required: true }],
        span: 24
      },
      {
        name: 'readCount',
        label: '阅读量',
        component: 'number',
        initialValue: 0,
        componentProps: { style: { width: '100%' } },
        span: 12
      },
      {
        name: 'favoriteCount',
        label: '收藏量',
        component: 'number',
        initialValue: 0,
        componentProps: { style: { width: '100%' } },
        span: 12
      },
      { name: 'isHot', label: '热门', component: 'switch', initialValue: false, span: 12 },
      { name: 'isRecommended', label: '推荐', component: 'switch', initialValue: false, span: 12 }
    ]
  },
  {
    key: 'questions',
    title: '提问管理',
    basePath: '/api/questions',
    primaryKey: 'id',
    columns: [
      { title: '标题', dataIndex: 'title' },
      { title: '患者姓名', dataIndex: ['patient', 'realName'], render: (_, record) => record?.patient?.realName || '-' },
      { title: '分类', dataIndex: 'category' },
      { title: '回答数', dataIndex: 'answerCount' },
      { title: '收藏数', dataIndex: 'favoriteCount' },
      { title: '查看数', dataIndex: 'viewCount' },
      { title: '热门', dataIndex: 'isHot', valueType: 'boolean' }
    ],
    formFields: [
      { 
        name: 'patientId', 
        label: '患者', 
        component: 'select', 
        rules: [{ required: true, message: '请选择患者' }], 
        options: [], // 动态加载
        loadOptionsFrom: '/api/patients', // 指定加载选项的API
        componentProps: { placeholder: '请选择患者' },
        span: 12 
      },
      { name: 'title', label: '标题', component: 'input', rules: [{ required: true }], span: 12 },
      { 
        name: 'category', 
        label: '分类', 
        component: 'select', 
        options: [
          { label: '热门', value: '热门' },
          { label: '养生', value: '养生' },
          { label: '问诊', value: '问诊' },
          { label: '保健', value: '保健' },
          { label: '减肥', value: '减肥' },
          { label: '育儿类', value: '育儿类' },
          { label: '失眠类', value: '失眠类' }
        ],
        componentProps: { placeholder: '请选择分类' },
        span: 12 
      },
      { name: 'tags', label: '标签', component: 'input', span: 12 },
      {
        name: 'content',
        label: '内容',
        component: 'textarea',
        componentProps: { rows: 6 },
        rules: [{ required: true }],
        span: 24
      },
      {
        name: 'favoriteCount',
        label: '收藏数',
        component: 'number',
        initialValue: 0,
        componentProps: { style: { width: '100%' } },
        span: 12
      },
      {
        name: 'answerCount',
        label: '回答数',
        component: 'number',
        initialValue: 0,
        componentProps: { style: { width: '100%' } },
        span: 12
      },
      {
        name: 'viewCount',
        label: '查看数',
        component: 'number',
        initialValue: 0,
        componentProps: { style: { width: '100%' } },
        span: 12
      },
      { name: 'isHot', label: '热门', component: 'switch', initialValue: false, span: 12 }
    ]
  },
  {
    key: 'consultations',
    title: '咨询管理',
    basePath: '/api/consultations',
    primaryKey: 'id',
    columns: [
      { title: '医生', dataIndex: ['doctor', 'name'], render: (_, record) => record?.doctor?.name || '-' },
      { title: '类型', dataIndex: 'consultationType' },
      { title: '价格', dataIndex: 'price' }
    ],
    formFields: [
      { 
        name: 'doctorId', 
        label: '医生', 
        component: 'select', 
        rules: [{ required: true, message: '请选择医生' }], 
        options: [], // 动态加载
        loadOptionsFrom: '/api/doctors', // 指定加载选项的API
        componentProps: { placeholder: '请选择医生' },
        span: 24 
      },
      { 
        name: 'consultationType', 
        label: '类型', 
        component: 'select', 
        rules: [{ required: true, message: '请选择类型' }],
        options: [
          { label: '图文', value: 'Text' },
          { label: '电话', value: 'Phone' },
          { label: '视频', value: 'Video' }
        ],
        componentProps: { placeholder: '请选择类型' },
        span: 24 
      },
      { 
        name: 'price', 
        label: '价格', 
        component: 'number', 
        initialValue: 0,
        componentProps: { style: { width: '100%' } },
        span: 24 
      }
    ]
  },
  // 咨询消息管理（基于 ConsultationMessage 实体）
  {
    key: 'consultation-messages',
    title: '咨询消息',
    basePath: '/api/consultationmessages',
    primaryKey: 'id',
    columns: [
      { title: '咨询ID', dataIndex: 'consultationId' },
      { title: '发送者ID', dataIndex: 'senderId' },
      { title: '是否医生发送', dataIndex: 'isFromDoctor', valueType: 'boolean' },
      { title: '内容', dataIndex: 'content', ellipsis: true },
      { title: '类型', dataIndex: 'messageType' },
      { title: '状态', dataIndex: 'status' },
      { title: '创建时间', dataIndex: 'createdAt', valueType: 'datetime' }
    ],
    formFields: [
      { name: 'consultationId', label: '咨询ID', component: 'input', rules: [{ required: true }], span: 24 },
      { name: 'content', label: '内容', component: 'textarea', rules: [{ required: true }], span: 24 },
      { 
        name: 'messageType', 
        label: '类型', 
        component: 'select', 
        options: [
          { label: '文本', value: 'Text' },
          { label: '图片', value: 'Image' },
          { label: '语音', value: 'Voice' },
          { label: '视频', value: 'Video' },
          { label: '文件', value: 'File' }
        ],
        rules: [{ required: true }],
        span: 24 
      },
      { name: 'attachmentUrl', label: '附件URL', component: 'input', span: 24 },
      { name: 'isRead', label: '已读', component: 'switch', initialValue: false, span: 12 },
      { 
        name: 'status', 
        label: '状态', 
        component: 'select',
        options: [
          { label: '已发送', value: 'Sent' },
          { label: '已送达', value: 'Delivered' },
          { label: '已读', value: 'Read' },
          { label: '发送失败', value: 'Failed' }
        ],
        initialValue: 'Sent',
        rules: [{ required: true, message: '请选择状态' }],
        span: 12
      }
    ]
  },
  {
    key: 'health-records',
    title: '健康记录',
    basePath: '/api/healthrecords',
    primaryKey: 'id',
    columns: [
      { title: '患者', dataIndex: 'patientId' },
      { title: '类型', dataIndex: 'recordType' },
      { title: '记录时间', dataIndex: 'recordTime' },
      { title: '备注', dataIndex: 'notes' }
    ],
    formFields: [
      { name: 'patientId', label: '患者ID', component: 'input', rules: [{ required: true }] },
      { name: 'recordType', label: '类型', component: 'input', rules: [{ required: true }] },
      {
        name: 'recordValue',
        label: '记录值(JSON)',
        component: 'textarea',
        componentProps: { rows: 4 },
        rules: [{ required: true }]
      },
      { name: 'recordTime', label: '记录时间', component: 'date', rules: [{ required: true }] },
      { name: 'notes', label: '备注', component: 'textarea' }
    ]
  },
  {
    key: 'medicines',
    title: '药品管理',
    basePath: '/api/medicines',
    primaryKey: 'id',
    columns: [
      { title: '药品名称', dataIndex: 'name' },
      { title: '规格', dataIndex: 'specification' },
      { title: '厂家', dataIndex: 'manufacturer' },
      { title: '价格', dataIndex: 'price' },
      { title: '库存', dataIndex: 'stock' }
    ],
    formFields: [
      { name: 'name', label: '药品名称', component: 'input', rules: [{ required: true }] },
      { name: 'specification', label: '规格', component: 'input' },
      { name: 'manufacturer', label: '厂家', component: 'input' },
      { name: 'price', label: '价格', component: 'number', rules: [{ required: true }] },
      { name: 'stock', label: '库存', component: 'number', rules: [{ required: true }] }
    ]
  },
  // 就诊记录
  {
    key: 'visit-records',
    title: '就诊记录',
    basePath: '/api/visitrecords',
    primaryKey: 'id',
    columns: [
      { title: '患者姓名', dataIndex: ['patient', 'realName'], render: (_, record) => record?.patient?.realName || '-' },
      { title: '医生', dataIndex: ['doctor', 'name'], render: (_, record) => record?.doctor?.name || '-' },
      { title: '就诊日期', dataIndex: 'visitDate', render: (value) => value ? new Date(value).toLocaleDateString('zh-CN') : '-' },
      { title: '就诊类型', dataIndex: 'visitType' },
      { title: '主诉', dataIndex: 'chiefComplaint', ellipsis: true, width: 150 },
      { title: '诊断', dataIndex: 'diagnosis', ellipsis: true, width: 150 },
      { title: '状态', dataIndex: 'status' }
    ],
    formFields: [
      { 
        name: 'patientId', 
        label: '患者', 
        component: 'select', 
        rules: [{ required: true, message: '请选择患者' }], 
        options: [], // 动态加载
        loadOptionsFrom: '/api/patients', // 指定加载选项的API
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择患者' },
        span: 12 
      },
      { 
        name: 'doctorId', 
        label: '医生', 
        component: 'select', 
        rules: [{ required: true, message: '请选择医生' }], 
        options: [], // 动态加载
        loadOptionsFrom: '/api/doctors', // 指定加载选项的API
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择医生' },
        span: 12 
      },
      { 
        name: 'consultationId', 
        label: '咨询', 
        component: 'select', 
        options: [], // 动态加载
        loadOptionsFrom: '/api/consultations', // 指定加载选项的API
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择咨询（可选）' },
        span: 24 
      },
      { 
        name: 'visitDate', 
        label: '就诊日期', 
        component: 'date', 
        rules: [{ required: true, message: '请选择就诊日期' }],
        componentProps: { style: { width: '100%' } },
        span: 24 
      },
      { 
        name: 'visitType', 
        label: '就诊类型', 
        component: 'select', 
        rules: [{ required: true, message: '请选择就诊类型' }],
        options: [
          { label: '门诊', value: 'Outpatient' },
          { label: '住院', value: 'Inpatient' },
          { label: '急诊', value: 'Emergency' },
          { label: '复诊', value: 'FollowUp' }
        ],
        componentProps: { placeholder: '请选择就诊类型' },
        span: 12 
      },
      { 
        name: 'status', 
        label: '状态', 
        component: 'select',
        options: [
          { label: '已完成', value: 'Completed' },
          { label: '进行中', value: 'InProgress' },
          { label: '已取消', value: 'Cancelled' }
        ],
        componentProps: { placeholder: '请选择状态' },
        span: 12 
      },
      {
        name: 'chiefComplaint',
        label: '主诉',
        component: 'textarea',
        componentProps: { rows: 3, maxLength: 1000, showCount: true },
        span: 24
      },
      {
        name: 'presentIllness',
        label: '现病史',
        component: 'textarea',
        componentProps: { rows: 4 },
        span: 24
      },
      {
        name: 'diagnosis',
        label: '诊断结果',
        component: 'textarea',
        componentProps: { rows: 3, maxLength: 1000, showCount: true },
        span: 24
      },
      {
        name: 'treatmentPlan',
        label: '治疗方案',
        component: 'textarea',
        componentProps: { rows: 4 },
        span: 24
      },
      {
        name: 'medicalAdvice',
        label: '医嘱',
        component: 'textarea',
        componentProps: { rows: 4 },
        span: 24
      },
      {
        name: 'notes',
        label: '备注',
        component: 'textarea',
        componentProps: { rows: 3, maxLength: 1000, showCount: true },
        span: 24
      }
    ]
  },
  // 病历信息
  {
    key: 'medical-records',
    title: '病历信息',
    basePath: '/api/medicalrecords',
    primaryKey: 'id',
    columns: [
      { title: '患者姓名', dataIndex: ['patient', 'realName'], render: (_, record) => record?.patient?.realName || '-' },
      { title: '医生', dataIndex: ['doctor', 'name'], render: (_, record) => record?.doctor?.name || '-' },
      { title: '疾病名称/症状', dataIndex: 'diseaseName', ellipsis: true },
      { title: '创建时间', dataIndex: 'createdAt', valueType: 'datetime' }
    ],
    formFields: [
      {
        name: 'patientId',
        label: '患者',
        component: 'select',
        rules: [{ required: true, message: '请选择患者' }],
        options: [],
        loadOptionsFrom: '/api/patients',
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择患者' },
        span: 24
      },
      {
        name: 'doctorId',
        label: '医生',
        component: 'select',
        options: [],
        loadOptionsFrom: '/api/doctors',
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择医生（可选）', allowClear: true },
        span: 24
      },
      {
        name: 'consultationId',
        label: '咨询',
        component: 'input',
        componentProps: { placeholder: '关联的咨询ID（可选）' },
        span: 24
      },
      { name: 'height', label: '身高(cm)', component: 'number', span: 12 },
      { name: 'weight', label: '体重(kg)', component: 'number', span: 12 },
      {
        name: 'diseaseDuration',
        label: '本次患病多久',
        component: 'input',
        componentProps: { placeholder: '如：2天、一周、半年等' },
        span: 24
      },
      {
        name: 'diseaseName',
        label: '疾病名称/症状',
        component: 'input',
        span: 24
      },
      {
        name: 'hasVisitedHospital',
        label: '是否已去医院就诊',
        component: 'switch',
        span: 12
      },
      {
        name: 'currentMedications',
        label: '当前用药',
        component: 'textarea',
        componentProps: { rows: 3 },
        span: 24
      },
      {
        name: 'isPregnant',
        label: '当前是否怀孕',
        component: 'switch',
        span: 12
      },
      {
        name: 'majorTreatmentHistory',
        label: '手术/放化疗经历及慢性病史',
        component: 'textarea',
        componentProps: { rows: 3 },
        span: 24
      },
      {
        name: 'allergyHistory',
        label: '药物过敏史',
        component: 'textarea',
        componentProps: { rows: 3 },
        span: 24
      },
      {
        name: 'diseaseDescription',
        label: '病情描述',
        component: 'textarea',
        componentProps: { rows: 4 },
        span: 24
      },
      {
        name: 'pastMedicalHistory',
        label: '既往病史',
        component: 'textarea',
        componentProps: { rows: 3 },
        span: 24
      },
      {
        name: 'additionalNotes',
        label: '其他补充说明',
        component: 'textarea',
        componentProps: { rows: 3 },
        span: 24
      }
    ]
  },
  // 检查报告
  {
    key: 'examination-reports',
    title: '检查报告',
    basePath: '/api/examinationreports',
    primaryKey: 'id',
    columns: [
      { title: '患者姓名', dataIndex: ['patient', 'realName'], render: (_, record) => record?.patient?.realName || '-' },
      { title: '医生', dataIndex: ['doctor', 'name'], render: (_, record) => record?.doctor?.name || '-' },
      { title: '报告编号', dataIndex: 'reportNumber' },
      { title: '检查类型', dataIndex: 'examinationType' },
      { title: '检查项目', dataIndex: 'examinationName' },
      { title: '检查日期', dataIndex: 'examinationDate', render: (value) => value ? new Date(value).toLocaleDateString('zh-CN') : '-' },
      { title: '结论', dataIndex: 'conclusion', ellipsis: true, width: 150 },
      { title: '状态', dataIndex: 'status' }
    ],
    formFields: [
      { 
        name: 'patientId', 
        label: '患者', 
        component: 'select', 
        rules: [{ required: true, message: '请选择患者' }], 
        options: [], // 动态加载
        loadOptionsFrom: '/api/patients', // 指定加载选项的API
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择患者' },
        span: 12 
      },
      { 
        name: 'doctorId', 
        label: '医生', 
        component: 'select', 
        rules: [{ required: true, message: '请选择医生' }], 
        options: [], // 动态加载
        loadOptionsFrom: '/api/doctors', // 指定加载选项的API
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择医生' },
        span: 12 
      },
      { 
        name: 'visitRecordId', 
        label: '就诊记录', 
        component: 'select', 
        options: [], // 动态加载
        loadOptionsFrom: '/api/visitrecords', // 指定加载选项的API
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择就诊记录（可选）' },
        span: 12 
      },
      { 
        name: 'consultationId', 
        label: '咨询', 
        component: 'select', 
        options: [], // 动态加载
        loadOptionsFrom: '/api/consultations', // 指定加载选项的API
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择咨询（可选）' },
        span: 12 
      },
      { 
        name: 'reportNumber', 
        label: '报告编号', 
        component: 'input', 
        rules: [{ required: true, message: '请输入报告编号' }],
        span: 12 
      },
      { 
        name: 'examinationType', 
        label: '检查类型', 
        component: 'select', 
        rules: [{ required: true, message: '请选择检查类型' }],
        options: [
          { label: '血常规', value: 'BloodTest' },
          { label: '尿常规', value: 'UrineTest' },
          { label: 'X光', value: 'XRay' },
          { label: 'CT', value: 'CT' },
          { label: '核磁共振', value: 'MRI' },
          { label: 'B超', value: 'Ultrasound' },
          { label: '心电图', value: 'ECG' },
          { label: '其他', value: 'Other' }
        ],
        componentProps: { placeholder: '请选择检查类型' },
        span: 12 
      },
      { 
        name: 'examinationName', 
        label: '检查项目名称', 
        component: 'input', 
        rules: [{ required: true, message: '请输入检查项目名称' }],
        span: 24 
      },
      { 
        name: 'examinationDate', 
        label: '检查日期', 
        component: 'date', 
        rules: [{ required: true, message: '请选择检查日期' }],
        componentProps: { style: { width: '100%' } },
        span: 24 
      },
      { 
        name: 'reportDate', 
        label: '报告日期', 
        component: 'date', 
        rules: [{ required: true, message: '请选择报告日期' }],
        componentProps: { style: { width: '100%' } },
        span: 24 
      },
      {
        name: 'results',
        label: '检查结果',
        component: 'textarea',
        rules: [{ required: true, message: '请输入检查结果' }],
        componentProps: { rows: 6, placeholder: '请输入检查结果（JSON格式或文本）' },
        span: 24
      },
      {
        name: 'conclusion',
        label: '检查结论',
        component: 'textarea',
        componentProps: { rows: 3, maxLength: 1000, showCount: true },
        span: 24
      },
      {
        name: 'recommendations',
        label: '建议',
        component: 'textarea',
        componentProps: { rows: 4 },
        span: 24
      },
      {
        name: 'reportFileUrl',
        label: '报告文件',
        component: 'upload',
        componentProps: { 
          listType: 'text',
          maxCount: 1,
          accept: '.pdf,.jpg,.jpeg,.png'
        },
        span: 24
      },
      { 
        name: 'status', 
        label: '状态', 
        component: 'select',
        options: [
          { label: '待检查', value: 'Pending' },
          { label: '已完成', value: 'Completed' },
          { label: '已取消', value: 'Cancelled' }
        ],
        componentProps: { placeholder: '请选择状态' },
        span: 12 
      },
      {
        name: 'notes',
        label: '备注',
        component: 'textarea',
        componentProps: { rows: 3, maxLength: 1000, showCount: true },
        span: 24
      }
    ]
  },
  // 处方生成
  {
    key: 'prescriptions',
    title: '处方生成',
    basePath: '/api/prescriptions',
    primaryKey: 'id',
    columns: [
      { title: '患者姓名', dataIndex: ['patient', 'realName'], render: (_, record) => record?.patient?.realName || '-' },
      { title: '医生', dataIndex: ['doctor', 'name'], render: (_, record) => record?.doctor?.name || '-' },
      { title: '处方编号', dataIndex: 'prescriptionNumber' },
      { title: '诊断', dataIndex: 'diagnosis', ellipsis: true, width: 150 },
      { title: '状态', dataIndex: 'status' },
      { title: '创建时间', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' }
    ],
    formFields: [
      { 
        name: 'patientId', 
        label: '患者', 
        component: 'select', 
        rules: [{ required: true, message: '请选择患者' }], 
        options: [], // 动态加载
        loadOptionsFrom: '/api/patients', // 指定加载选项的API
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择患者' },
        span: 12 
      },
      { 
        name: 'doctorId', 
        label: '医生', 
        component: 'select', 
        rules: [{ required: true, message: '请选择医生' }], 
        options: [], // 动态加载
        loadOptionsFrom: '/api/doctors', // 指定加载选项的API
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择医生' },
        span: 12 
      },
      { 
        name: 'consultationId', 
        label: '咨询', 
        component: 'select', 
        options: [], // 动态加载
        loadOptionsFrom: '/api/consultations', // 指定加载选项的API
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: '请选择咨询（可选）' },
        span: 12 
      },
      { 
        name: 'prescriptionNumber', 
        label: '处方编号', 
        component: 'input', 
        rules: [{ required: true, message: '请输入处方编号' }],
        span: 12 
      },
      {
        name: 'diagnosis',
        label: '诊断结果',
        component: 'textarea',
        componentProps: { rows: 3, maxLength: 1000, showCount: true },
        span: 24
      },
      {
        name: 'prescriptionContent',
        label: '处方内容',
        component: 'textarea',
        rules: [{ required: true, message: '请输入处方内容' }],
        componentProps: { rows: 6, placeholder: '请输入处方内容（JSON格式或文本）' },
        span: 24
      },
      { 
        name: 'status', 
        label: '状态', 
        component: 'select',
        rules: [{ required: true, message: '请选择状态' }],
        options: [
          { label: '草稿', value: 'Draft' },
          { label: '已开具', value: 'Issued' },
          { label: '已取药', value: 'Filled' },
          { label: '已取消', value: 'Cancelled' }
        ],
        componentProps: { placeholder: '请选择状态' },
        span: 12 
      }
    ]
  },
  // 我的处方（患者查看自己的处方）
  {
    key: 'my-prescriptions',
    title: '我的处方',
    basePath: '/api/prescriptions',
    primaryKey: 'id',
    columns: [
      { title: '医生', dataIndex: ['doctor', 'name'], render: (_, record) => record?.doctor?.name || '-' },
      { title: '处方编号', dataIndex: 'prescriptionNumber' },
      { title: '诊断', dataIndex: 'diagnosis', ellipsis: true, width: 150 },
      { title: '状态', dataIndex: 'status' },
      { title: '创建时间', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' }
    ],
    formFields: [] // 我的处方只读，不允许创建和编辑
  },
  // 系统管理 - 用户管理（只显示Admin角色）
  {
    key: 'system-users',
    title: '用户管理',
    basePath: '/api/users',
    primaryKey: 'id',
    columns: [
      { title: '用户名', dataIndex: 'username' },
      { 
        title: '头像', 
        dataIndex: 'avatarUrl',
        width: 80,
        render: (text, record) => {
          // Ant Design Table 的 render 函数签名: (text, record, index)
          // text 是 dataIndex 对应的值，即 record.avatarUrl
          // 但为了兼容性，我们也从 record 中直接获取
          // 尝试多种可能的字段名
          const avatarUrl = text || 
                           record?.avatarUrl || 
                           record?.AvatarUrl || 
                           record?.avatar_url ||
                           record?.avatar ||
                           null;
          
          console.log('[头像列] render调用:', { 
            text,
            'record.avatarUrl': record?.avatarUrl,
            'record.AvatarUrl': record?.AvatarUrl,
            'record.avatar_url': record?.avatar_url,
            'record.avatar': record?.avatar,
            avatarUrl,
            'avatarUrl类型': typeof avatarUrl,
            'avatarUrl值': avatarUrl,
            'record keys': Object.keys(record || {}),
            '完整record': JSON.stringify(record, null, 2)
          });
          
          if (!avatarUrl || avatarUrl === null || avatarUrl === undefined || (typeof avatarUrl === 'string' && avatarUrl.trim() === '')) {
            console.log('[头像列] 头像URL为空，显示 "-"');
            return React.createElement('span', {}, '-');
          }
          
          // 构建完整的图片URL
          const baseURL = import.meta?.env?.VITE_API_BASE_URL ?? 'http://localhost:5000';
          let imageUrl;
          
          if (typeof avatarUrl === 'string') {
            if (avatarUrl.startsWith('http://') || avatarUrl.startsWith('https://')) {
              imageUrl = avatarUrl;
            } else if (avatarUrl.startsWith('/')) {
              imageUrl = `${baseURL}${avatarUrl}`;
            } else {
              imageUrl = `${baseURL}/${avatarUrl}`;
            }
          } else {
            console.warn('[头像列] avatarUrl 不是字符串:', typeof avatarUrl, avatarUrl);
            return React.createElement('span', {}, '-');
          }
          
          console.log('[头像列] 构建的图片URL:', { avatarUrl, baseURL, imageUrl });
          
          return React.createElement('img', { 
            src: imageUrl, 
            style: { width: 40, height: 40, borderRadius: '50%', objectFit: 'cover', cursor: 'pointer', display: 'block' },
            alt: '头像',
            onClick: () => {
              // 点击头像可以查看大图
              window.open(imageUrl, '_blank');
            },
            onError: (e) => {
              console.error('[头像列] 图片加载失败:', imageUrl, '错误:', e);
              e.target.style.display = 'none';
              const parent = e.target.parentElement;
              if (parent) {
                const span = React.createElement('span', {}, '-');
                // 使用 ReactDOM 替换
                if (parent.firstChild) {
                  parent.replaceChild(document.createTextNode('-'), parent.firstChild);
                }
              }
            },
            onLoad: () => {
              console.log('[头像列] 图片加载成功:', imageUrl);
            }
          });
        }
      },
      { title: '邮箱', dataIndex: 'email' },
      { title: '手机号', dataIndex: 'phoneNumber' },
      {
        title: '角色',
        dataIndex: 'role',
        render: (value, record) => {
          // 优先显示 RoleName（角色的 Name），如果没有则显示 Role（Code）
          return record?.roleName || value || '-';
        }
      },
      { title: '状态', dataIndex: 'isActive', valueType: 'boolean' },
      { title: '创建时间', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' }
    ],
    formFields: [
      { name: 'username', label: '用户名', component: 'input', rules: [{ required: true, message: '请输入用户名' }], span: 12 },
      { name: 'password', label: '密码', component: 'password', placeholder: '不填则不修改密码（新建时填写，不填则默认为123456）', span: 12 },
      { name: 'email', label: '邮箱', component: 'input', span: 12 },
      { name: 'phoneNumber', label: '手机号', component: 'input', span: 12 },
      { 
        name: 'role', 
        label: '角色', 
        component: 'select',
        options: [], // 动态加载
        loadOptionsFrom: '/api/roles', // 从角色表加载
        componentProps: { placeholder: '请选择角色' },
        span: 12 
      },
      {
        name: 'avatarUrl',
        label: '头像',
        component: 'upload',
        componentProps: { 
          listType: 'picture-card',
          maxCount: 1,
          accept: 'image/*'
        },
        span: 12
      },
      { name: 'isActive', label: '启用', component: 'switch', initialValue: true, span: 12 }
    ],
    // 默认筛选条件：只显示System类型的用户（UserTypeId = 1）
    defaultParams: { userTypeId: 1 },
    // 创建时自动设置角色为Admin
    onCreate: (payload) => {
      return { ...payload, role: payload.role || 'Admin' };
    }
  },
  // 系统管理 - 部门管理
  {
    key: 'system-departments',
    title: '部门管理',
    basePath: '/api/departments',
    primaryKey: 'id',
    columns: [
      { title: '科室名称', dataIndex: 'name' },
      { title: '描述', dataIndex: 'description', ellipsis: true, width: 200 },
      { title: '排序', dataIndex: 'sortOrder' },
      { title: '热门', dataIndex: 'isHot', valueType: 'boolean' },
      { title: '创建时间', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' }
    ],
    formFields: [
      { name: 'name', label: '科室名称', component: 'input', rules: [{ required: true, message: '请输入科室名称' }], span: 24 },
      { name: 'description', label: '描述', component: 'textarea', componentProps: { rows: 4 }, span: 24 },
      {
        name: 'sortOrder',
        label: '排序值',
        component: 'number',
        initialValue: 0,
        componentProps: { style: { width: '100%' } },
        span: 12
      },
      { name: 'isHot', label: '热门', component: 'switch', initialValue: false, span: 12 }
    ]
  },
  // 系统管理 - 角色管理
  {
    key: 'system-roles',
    title: '角色管理',
    basePath: '/api/roles',
    primaryKey: 'id',
    columns: [
      { title: '角色名称', dataIndex: 'name' },
      { title: '角色代码', dataIndex: 'code' },
      { title: '描述', dataIndex: 'description', ellipsis: true, width: 200 },
      { title: '启用', dataIndex: 'isActive', valueType: 'boolean' },
      { title: '排序', dataIndex: 'sortOrder' },
      { title: '创建时间', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' }
    ],
    formFields: [
      { name: 'name', label: '角色名称', component: 'input', rules: [{ required: true, message: '请输入角色名称' }], span: 12 },
      { name: 'code', label: '角色代码', component: 'input', rules: [{ required: true, message: '请输入角色代码' }], span: 12 },
      { name: 'description', label: '描述', component: 'textarea', componentProps: { rows: 3 }, span: 24 },
      { name: 'isActive', label: '启用', component: 'switch', initialValue: true, span: 12 },
      {
        name: 'sortOrder',
        label: '排序值',
        component: 'number',
        initialValue: 0,
        componentProps: { style: { width: '100%' } },
        span: 12
      }
    ]
  },
  // 药品管理 - 药品分类
  {
    key: 'drug-categories',
    title: '药品分类',
    basePath: '/api/drugcategories',
    primaryKey: 'id',
    columns: [
      { title: '分类名称', dataIndex: 'categoryName' },
      { title: '父分类', dataIndex: ['parent', 'categoryName'], render: (_, record) => record?.parent?.categoryName || '顶级分类' },
      { title: '描述', dataIndex: 'description', ellipsis: true, width: 200 },
      { title: '排序', dataIndex: 'sortOrder' },
      { title: '启用', dataIndex: 'isActive', valueType: 'boolean' },
      { title: '创建时间', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' }
    ],
    formFields: [
      { name: 'categoryName', label: '分类名称', component: 'input', rules: [{ required: true, message: '请输入分类名称' }], span: 12 },
      { 
        name: 'parentId', 
        label: '父分类', 
        component: 'select', 
        options: [], // 动态加载
        loadOptionsFrom: '/api/drugcategories',
        componentProps: { placeholder: '请选择父分类（留空则为顶级分类）', allowClear: true },
        span: 12 
      },
      { name: 'description', label: '描述', component: 'textarea', componentProps: { rows: 3, maxLength: 500, showCount: true }, span: 24 },
      { name: 'isActive', label: '启用', component: 'switch', initialValue: true, span: 12 },
      {
        name: 'sortOrder',
        label: '排序值',
        component: 'number',
        initialValue: 0,
        componentProps: { style: { width: '100%' } },
        span: 12
      }
    ]
  },
  // 药品管理 - 药品信息
  {
    key: 'drugs',
    title: '药品信息',
    basePath: '/api/drugs',
    primaryKey: 'id',
    columns: [
      { title: '通用名', dataIndex: 'commonName' },
      { title: '商品名', dataIndex: 'tradeName' },
      { title: '规格', dataIndex: 'specification' },
      { title: '生产厂家', dataIndex: 'manufacturer' },
      { title: '批准文号', dataIndex: 'approvalNumber' },
      { title: '分类', dataIndex: ['category', 'categoryName'], render: (_, record) => record?.category?.categoryName || '-' },
      { title: '单位', dataIndex: 'unit' },
      { title: '启用', dataIndex: 'isActive', valueType: 'boolean' }
    ],
    formFields: [
      { name: 'commonName', label: '药品通用名', component: 'input', rules: [{ required: true, message: '请输入药品通用名' }], span: 12 },
      { name: 'tradeName', label: '药品商品名', component: 'input', span: 12 },
      { name: 'specification', label: '药品规格', component: 'input', rules: [{ required: true, message: '请输入药品规格' }], span: 12 },
      { name: 'unit', label: '药品单位', component: 'input', rules: [{ required: true, message: '请输入药品单位' }], span: 12 },
      { name: 'manufacturer', label: '生产厂家', component: 'input', rules: [{ required: true, message: '请输入生产厂家' }], span: 12 },
      { name: 'approvalNumber', label: '国药准字批准文号', component: 'input', rules: [{ required: true, message: '请输入批准文号' }], span: 12 },
      { 
        name: 'categoryId', 
        label: '所属分类', 
        component: 'select', 
        rules: [{ required: true, message: '请选择分类' }],
        options: [], // 动态加载
        loadOptionsFrom: '/api/drugcategories',
        componentProps: { placeholder: '请选择分类' },
        span: 12 
      },
      { 
        name: 'storageCondition', 
        label: '储存条件', 
        component: 'select',
        options: [
          { label: '常温', value: '常温' },
          { label: '阴凉', value: '阴凉' },
          { label: '冷藏', value: '冷藏' },
          { label: '冷冻', value: '冷冻' }
        ],
        componentProps: { placeholder: '请选择储存条件' },
        span: 12 
      },
      { name: 'isActive', label: '启用', component: 'switch', initialValue: true, span: 12 }
    ]
  },
  // 药品管理 - 药品库存
  {
    key: 'drug-inventories',
    title: '药品库存',
    basePath: '/api/druginventories',
    primaryKey: 'id',
    columns: [
      { title: '药品通用名', dataIndex: ['drug', 'commonName'], render: (_, record) => record?.drug?.commonName || '-' },
      { title: '商品名', dataIndex: ['drug', 'tradeName'], render: (_, record) => record?.drug?.tradeName || '-' },
      { title: '规格', dataIndex: ['drug', 'specification'], render: (_, record) => record?.drug?.specification || '-' },
      { title: '生产厂家', dataIndex: ['drug', 'manufacturer'], render: (_, record) => record?.drug?.manufacturer || '-' },
      { title: '分类', dataIndex: ['drug', 'category', 'categoryName'], render: (_, record) => record?.drug?.category?.categoryName || '-' },
      { title: '库位', dataIndex: 'warehouseLocation' },
      { title: '当前库存', dataIndex: 'currentQuantity', render: (value) => value ?? 0 },
      { title: '最后更新时间', dataIndex: 'lastUpdatedAt', render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' }
    ],
    formFields: [] // 库存只读，通过入库单自动更新
  },
  // 药品管理 - 药品入库
  {
    key: 'drug-stock-ins',
    title: '药品入库',
    basePath: '/api/drugstockins',
    primaryKey: 'id',
    columns: [
      { title: '入库单号', dataIndex: 'invoiceNo' },
      { title: '供应商', dataIndex: 'supplierName' },
      { title: '操作员', dataIndex: ['operator', 'username'], render: (_, record) => record?.operator?.username || '-' },
      { title: '入库时间', dataIndex: 'operationTime', render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' },
      { title: '总金额', dataIndex: 'totalAmount', render: (value) => `¥${(value ?? 0).toFixed(2)}` },
      { title: '状态', dataIndex: 'status', render: (value) => value === 1 ? '已入库' : '已取消' },
      { title: '明细数量', dataIndex: 'lineCount', render: (_, record) => record?.lines?.length || 0 }
    ],
    formFields: [
      { name: 'invoiceNo', label: '入库单号', component: 'input', rules: [{ required: true, message: '请输入入库单号' }], span: 12 },
      { name: 'supplierName', label: '供应商名称', component: 'input', rules: [{ required: true, message: '请输入供应商名称' }], span: 12 },
      { name: 'operationTime', label: '入库操作时间', component: 'date', rules: [{ required: true, message: '请选择入库操作时间' }], span: 24 },
      { name: 'remarks', label: '备注', component: 'textarea', componentProps: { rows: 3 }, span: 24 },
      { 
        name: 'status', 
        label: '状态', 
        component: 'select',
        options: [
          { label: '已入库', value: 1 },
          { label: '已取消', value: 0 }
        ],
        componentProps: { placeholder: '请选择状态' },
        span: 12 
      }
      // 注意：入库明细行（lines）需要在前端特殊处理，不能直接使用标准表单
    ]
  },
  // 财务管理
  {
    key: 'financial-receivables',
    title: '应收管理',
    basePath: '/api/financialreceivables',
    primaryKey: 'id',
    columns: [
      { title: '参考号', dataIndex: 'referenceNo' },
      { title: '渠道', dataIndex: 'channel' },
      { title: '应收金额', dataIndex: 'amount' },
      { title: '已收金额', dataIndex: 'receivedAmount' },
      { title: '待收金额', dataIndex: 'pendingAmount' },
      { title: '状态', dataIndex: 'status' },
      { title: '币种', dataIndex: 'currency' },
      { title: '创建时间', dataIndex: 'createdAt', valueType: 'datetime' }
    ],
    formFields: [
      { name: 'referenceNo', label: '参考号', component: 'input', span: 12 },
      { name: 'channel', label: '渠道', component: 'input', span: 12 },
      { name: 'amount', label: '应收金额', component: 'number', componentProps: { style: { width: '100%' } }, rules: [{ required: true }], span: 12 },
      { name: 'receivedAmount', label: '已收金额', component: 'number', componentProps: { style: { width: '100%' } }, span: 12 },
      { name: 'pendingAmount', label: '待收金额', component: 'number', componentProps: { style: { width: '100%' } }, span: 12 },
      { 
        name: 'status', 
        label: '状态', 
        component: 'select', 
        options: [
          { label: '待收', value: 'pending' },
          { label: '部分收', value: 'partial' },
          { label: '已收', value: 'received' },
          { label: '已关闭', value: 'closed' }
        ],
        componentProps: { style: { width: '100%' } },
        span: 12
      },
      { 
        name: 'currency', 
        label: '币种', 
        component: 'select', 
        initialValue: 'CNY', 
        options: [
          { label: 'CNY', value: 'CNY' },
          { label: 'USD', value: 'USD' },
          { label: 'EUR', value: 'EUR' },
          { label: 'GBP', value: 'GBP' },
          { label: 'JPY', value: 'JPY' }
        ],
        componentProps: { style: { width: '100%' } },
        span: 12 
      },
      { name: 'remark', label: '备注', component: 'textarea', span: 24 }
    ]
  },
  {
    key: 'financial-payables',
    title: '应付管理',
    basePath: '/api/financialpayables',
    primaryKey: 'id',
    columns: [
      { title: '参考号', dataIndex: 'referenceNo' },
      { title: '结算对象', dataIndex: 'vendorName' },
      { title: '费用类型', dataIndex: 'expenseType' },
      { title: '应付金额', dataIndex: 'amount' },
      { title: '已付金额', dataIndex: 'paidAmount' },
      { title: '待付金额', dataIndex: 'pendingAmount' },
      { title: '状态', dataIndex: 'status' },
      { title: '币种', dataIndex: 'currency' },
      { title: '创建时间', dataIndex: 'createdAt', valueType: 'datetime' }
    ],
    formFields: [
      { name: 'referenceNo', label: '参考号', component: 'input', span: 12 },
      { name: 'vendorName', label: '结算对象', component: 'input', span: 12 },
      { name: 'expenseType', label: '费用类型', component: 'input', span: 12 },
      { name: 'amount', label: '应付金额', component: 'number', componentProps: { style: { width: '100%' } }, rules: [{ required: true }], span: 12 },
      { name: 'paidAmount', label: '已付金额', component: 'number', componentProps: { style: { width: '100%' } }, span: 12 },
      { name: 'pendingAmount', label: '待付金额', component: 'number', componentProps: { style: { width: '100%' } }, span: 12 },
      { 
        name: 'status', 
        label: '状态', 
        component: 'select', 
        options: [
          { label: '待付', value: 'pending' },
          { label: '部分付', value: 'partial' },
          { label: '已付', value: 'paid' },
          { label: '已关闭', value: 'closed' }
        ],
        componentProps: { style: { width: '100%' } },
        span: 12
      },
      { 
        name: 'currency', 
        label: '币种', 
        component: 'select', 
        initialValue: 'CNY', 
        options: [
          { label: 'CNY', value: 'CNY' },
          { label: 'USD', value: 'USD' },
          { label: 'EUR', value: 'EUR' },
          { label: 'GBP', value: 'GBP' },
          { label: 'JPY', value: 'JPY' }
        ],
        componentProps: { style: { width: '100%' } },
        span: 12 
      },
      { name: 'remark', label: '备注', component: 'textarea', span: 24 }
    ]
  },
  {
    key: 'financial-fees',
    title: '费用管理',
    basePath: '/api/financialfees',
    primaryKey: 'id',
    columns: [
      { title: '参考号', dataIndex: 'referenceNo' },
      { title: '费用类型', dataIndex: 'feeType' },
      { title: '金额', dataIndex: 'amount' },
      { title: '备注', dataIndex: 'remark' },
      { title: '创建时间', dataIndex: 'createdAt', valueType: 'datetime' }
    ],
    formFields: [
      { name: 'referenceNo', label: '参考号', component: 'input', span: 12 },
      { name: 'feeType', label: '费用类型', component: 'input', componentProps: { style: { width: '100%' } }, span: 12 },
      { name: 'amount', label: '金额', component: 'number', componentProps: { style: { width: '100%' } }, rules: [{ required: true }], span: 12 },
      { name: 'remark', label: '备注', component: 'textarea', span: 24 }
    ]
  },
  {
    key: 'financial-settlements',
    title: '结算管理',
    basePath: '/api/financialsettlements',
    primaryKey: 'id',
    columns: [
      { title: '结算名称', dataIndex: 'name' },
      { title: '开始日期', dataIndex: 'periodStart', valueType: 'datetime' },
      { title: '结束日期', dataIndex: 'periodEnd', valueType: 'datetime' },
      { title: '应收合计', dataIndex: 'totalReceivable' },
      { title: '应付合计', dataIndex: 'totalPayable' },
      { title: '净额', dataIndex: 'netAmount' },
      { title: '状态', dataIndex: 'status' },
      { title: '备注', dataIndex: 'remark' },
      { title: '创建时间', dataIndex: 'createdAt', valueType: 'datetime' }
    ],
    formFields: [
      { name: 'name', label: '结算名称', component: 'input', componentProps: { style: { width: '100%' } }, rules: [{ required: true }], span: 12 },
      { name: 'periodStart', label: '开始日期', component: 'date', componentProps: { style: { width: '100%' } }, span: 12 },
      { name: 'periodEnd', label: '结束日期', component: 'date', componentProps: { style: { width: '100%' } }, span: 12 },
      { name: 'totalReceivable', label: '应收合计', component: 'number', componentProps: { style: { width: '100%' } }, span: 12 },
      { name: 'totalPayable', label: '应付合计', component: 'number', componentProps: { style: { width: '100%' } }, span: 12 },
      { name: 'netAmount', label: '净额', component: 'number', componentProps: { style: { width: '100%' } }, span: 12 },
      { 
        name: 'status', 
        label: '状态', 
        component: 'select', 
        options: [
          { label: '待结算', value: 'pending' },
          { label: '结算中', value: 'processing' },
          { label: '已完成', value: 'done' },
          { label: '已关闭', value: 'closed' }
        ],
        span: 12
      },
      { name: 'remark', label: '备注', component: 'textarea', span: 24 }
    ]
  },
  // 系统管理 - 权限管理
  {
    key: 'system-permissions',
    title: '权限管理',
    basePath: '/api/permissions',
    primaryKey: 'id',
    columns: [
      { title: '权限名称', dataIndex: 'name' },
      { title: '权限代码', dataIndex: 'code' },
      { title: '菜单URL', dataIndex: 'menuUrl', render: (value) => value || '-' },
      { title: '权限类型', dataIndex: 'permissionType', render: (value) => value || '-' },
      { title: '是否启用', dataIndex: 'isActive', valueType: 'boolean' },
      { title: '排序', dataIndex: 'sortOrder' },
      { title: '创建时间', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' }
    ],
    formFields: [
      { name: 'name', label: '权限名称', component: 'input', rules: [{ required: true, message: '请输入权限名称' }], span: 12 },
      { name: 'code', label: '权限代码', component: 'input', rules: [{ required: true, message: '请输入权限代码' }], span: 12 },
      { name: 'description', label: '权限描述', component: 'textarea', componentProps: { rows: 3 }, span: 24 },
      { 
        name: 'menuUrl', 
        label: '菜单URL', 
        component: 'select',
        loadOptionsFrom: '/api/permissions/menu-urls',
        options: [],
        componentProps: { 
          mode: undefined,
          showSearch: true,
          placeholder: '请选择或输入菜单URL',
          allowClear: true,
          filterOption: (input, option) => 
            (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
        },
        span: 12 
      },
      { 
        name: 'permissionType', 
        label: '权限类型', 
        component: 'select',
        loadOptionsFrom: '/api/permissiontypedictionaries/all',
        options: [],
        componentProps: { placeholder: '请选择权限类型' },
        span: 12 
      },
      { name: 'sortOrder', label: '排序', component: 'number', initialValue: 0, span: 12 },
      { name: 'isActive', label: '启用', component: 'switch', initialValue: true, span: 12 }
    ]
  },
  // 发帖管理 - 发帖信息
  {
    key: 'posts',
    title: '发帖信息',
    basePath: '/api/posts',
    primaryKey: 'id',
    columns: [
      { title: '标题', dataIndex: 'title', ellipsis: true },
      { 
        title: '患友会', 
        dataIndex: 'groupName',
        render: (text, record) => record.groupName || '-'
      },
      { 
        title: '医生', 
        dataIndex: 'doctorName',
        render: (text, record) => record.doctorName || '-'
      },
      { title: '标签', dataIndex: 'tag' },
      { title: '作者', dataIndex: 'authorName' },
      { title: '阅读数', dataIndex: 'readCount' },
      { title: '评论数', dataIndex: 'commentCount' },
      { title: '点赞数', dataIndex: 'likeCount' },
      { 
        title: '是否置顶', 
        dataIndex: 'isPinned', 
        valueType: 'boolean' 
      },
      { 
        title: '是否删除', 
        dataIndex: 'isDeleted', 
        valueType: 'boolean' 
      },
      { 
        title: '创建时间', 
        dataIndex: 'createdAt', 
        render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' 
      }
    ],
    formFields: [
      { name: 'title', label: '标题', component: 'input', rules: [{ required: true, max: 200 }], span: 24 },
      { 
        name: 'content', 
        label: '内容', 
        component: 'textarea', 
        componentProps: { rows: 6, maxLength: 10000, showCount: true },
        rules: [{ required: true, max: 10000 }],
        span: 24 
      },
      { 
        name: 'tag', 
        label: '标签', 
        component: 'select',
        options: [
          { label: '求助', value: '求助' },
          { label: '分享', value: '分享' },
          { label: '讨论', value: '讨论' }
        ],
        span: 12 
      },
      { name: 'isPinned', label: '是否置顶', component: 'switch', initialValue: false, span: 12 },
      { name: 'isDeleted', label: '是否删除', component: 'switch', initialValue: false, span: 12 }
    ]
  },
  // 发帖管理 - 评论信息
  {
    key: 'post-comments',
    title: '评论信息',
    basePath: '/api/postcomments',
    primaryKey: 'id',
    columns: [
      { 
        title: '帖子标题', 
        dataIndex: 'postTitle',
        ellipsis: true,
        render: (text, record) => record.postTitle || '-'
      },
      { 
        title: '患友会', 
        dataIndex: 'groupName',
        render: (text, record) => record.groupName || '-'
      },
      { title: '评论内容', dataIndex: 'content', ellipsis: true },
      { title: '作者', dataIndex: 'authorName' },
      { 
        title: '父评论ID', 
        dataIndex: 'parentCommentId',
        render: (value) => value || '-'
      },
      { 
        title: '是否删除', 
        dataIndex: 'isDeleted', 
        valueType: 'boolean' 
      },
      { 
        title: '创建时间', 
        dataIndex: 'createdAt', 
        render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' 
      }
    ],
    formFields: [
      { 
        name: 'content', 
        label: '评论内容', 
        component: 'textarea', 
        componentProps: { rows: 4, maxLength: 2000, showCount: true },
        rules: [{ required: true, max: 2000 }],
        span: 24 
      },
      { name: 'isDeleted', label: '是否删除', component: 'switch', initialValue: false, span: 12 }
    ]
  },
  // 发帖管理 - 点赞信息
  {
    key: 'post-likes',
    title: '点赞信息',
    basePath: '/api/postlikes',
    primaryKey: 'id',
    columns: [
      { 
        title: '帖子标题', 
        dataIndex: 'postTitle',
        ellipsis: true,
        render: (text, record) => record.postTitle || '-'
      },
      { 
        title: '患友会', 
        dataIndex: 'groupName',
        render: (text, record) => record.groupName || '-'
      },
      { title: '用户名', dataIndex: 'userName' },
      { 
        title: '点赞时间', 
        dataIndex: 'createdAt', 
        render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' 
      }
    ],
    formFields: [] // 点赞信息只读，不允许编辑
  },
  {
    key: 'feedbacks',
    title: '反馈投诉',
    basePath: '/api/feedbacks',
    primaryKey: 'id',
    columns: [
      { title: '标题', dataIndex: 'title', ellipsis: true },
      { title: '内容', dataIndex: 'content', ellipsis: true },
      { 
        title: '状态', 
        dataIndex: 'status',
        render: (text) => {
          const statusMap = {
            'Pending': '待处理',
            'Processing': '处理中',
            'Resolved': '已解决',
            'Closed': '已关闭'
          };
          return statusMap[text] || text;
        }
      },
      { title: '处理人', dataIndex: 'processorName', render: (text) => text || '-' },
      { 
        title: '提交时间', 
        dataIndex: 'createdAt', 
        render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' 
      },
      { 
        title: '处理时间', 
        dataIndex: 'processedAt', 
        render: (value) => value ? new Date(value).toLocaleString('zh-CN') : '-' 
      }
    ],
    formFields: [
      { name: 'title', label: '标题', component: 'input', rules: [{ required: true }], span: 24 },
      { name: 'content', label: '内容', component: 'textarea', rules: [{ required: true }], componentProps: { rows: 6 }, span: 24 },
      {
        name: 'status',
        label: '状态',
        component: 'select',
        options: [
          { label: '待处理', value: 'Pending' },
          { label: '处理中', value: 'Processing' },
          { label: '已解决', value: 'Resolved' },
          { label: '已关闭', value: 'Closed' }
        ],
        span: 12
      },
      { name: 'processNote', label: '处理备注', component: 'textarea', componentProps: { rows: 4 }, span: 24 }
    ]
  },
  // 养生馆 - 商品分类
  {
    key: 'product-categories',
    title: 'resource.product-categories.title',
    basePath: '/api/productcategories',
    primaryKey: 'id',
    columns: [
      { title: 'resource.product-categories.column.name', dataIndex: 'name' },
      { title: 'resource.product-categories.column.code', dataIndex: 'code' },
      { title: 'resource.product-categories.column.sortOrder', dataIndex: 'sortOrder' },
      { title: 'resource.product-categories.column.isEnabled', dataIndex: 'isEnabled', valueType: 'boolean' },
      { title: 'resource.product-categories.column.createdAt', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString() : '-' },
      { title: 'resource.product-categories.column.updatedAt', dataIndex: 'updatedAt', render: (value) => value ? new Date(value).toLocaleString() : '-' }
    ],
    formFields: [
      {
        name: 'name',
        label: 'resource.product-categories.field.name.label',
        component: 'input',
        rules: [{ required: true, message: 'resource.product-categories.field.name.rule.required' }],
        componentProps: { placeholder: 'resource.product-categories.field.name.placeholder' },
        span: 12
      },
      {
        name: 'code',
        label: 'resource.product-categories.field.code.label',
        component: 'input',
        rules: [{ required: true, message: 'resource.product-categories.field.code.rule.required' }],
        componentProps: { placeholder: 'resource.product-categories.field.code.placeholder' },
        span: 12
      },
      {
        name: 'sortOrder',
        label: 'resource.product-categories.field.sortOrder.label',
        component: 'number',
        initialValue: 0,
        componentProps: { placeholder: 'resource.product-categories.field.sortOrder.placeholder', min: 0 },
        span: 12
      },
      {
        name: 'isEnabled',
        label: 'resource.product-categories.field.isEnabled.label',
        component: 'switch',
        initialValue: true,
        span: 12
      }
    ]
  },
  // 养生馆 - 商品信息
  {
    key: 'products',
    title: 'resource.products.title',
    basePath: '/api/products',
    primaryKey: 'id',
    columns: [
      { title: 'resource.products.column.name', dataIndex: 'name' },
      { title: 'resource.products.column.code', dataIndex: 'code' },
      { title: 'resource.products.column.price', dataIndex: 'price' },
      { title: 'resource.products.column.marketPrice', dataIndex: 'marketPrice' },
      { title: 'resource.products.column.isEnabled', dataIndex: 'isEnabled', valueType: 'boolean' },
      { title: 'resource.products.column.isVirtual', dataIndex: 'isVirtual', valueType: 'boolean' },
      { title: 'resource.products.column.createdAt', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString() : '-' },
      { title: 'resource.products.column.updatedAt', dataIndex: 'updatedAt', render: (value) => value ? new Date(value).toLocaleString() : '-' }
    ],
    formFields: [
      {
        name: 'categoryId',
        label: 'resource.products.field.categoryId.label',
        component: 'select',
        rules: [{ required: true, message: 'resource.products.field.categoryId.rule.required' }],
        options: [],
        loadOptionsFrom: '/api/productcategories',
        loadOptionsParams: { pageSize: 1000 },
        componentProps: { placeholder: 'resource.products.field.categoryId.placeholder' },
        span: 12
      },
      {
        name: 'name',
        label: 'resource.products.field.name.label',
        component: 'input',
        rules: [{ required: true, message: 'resource.products.field.name.rule.required' }],
        componentProps: { placeholder: 'resource.products.field.name.placeholder' },
        span: 12
      },
      {
        name: 'code',
        label: 'resource.products.field.code.label',
        component: 'input',
        rules: [{ required: true, message: 'resource.products.field.code.rule.required' }],
        componentProps: { placeholder: 'resource.products.field.code.placeholder' },
        span: 12
      },
      {
        name: 'price',
        label: 'resource.products.field.price.label',
        component: 'number',
        rules: [{ required: true, message: 'resource.products.field.price.rule.required' }],
        componentProps: { placeholder: 'resource.products.field.price.placeholder', min: 0, precision: 2 },
        span: 12
      },
      {
        name: 'marketPrice',
        label: 'resource.products.field.marketPrice.label',
        component: 'number',
        componentProps: { placeholder: 'resource.products.field.marketPrice.placeholder', min: 0, precision: 2 },
        span: 12
      },
      {
        name: 'description',
        label: 'resource.products.field.description.label',
        component: 'textarea',
        componentProps: { rows: 4, maxLength: 500, showCount: true, placeholder: 'resource.products.field.description.placeholder' },
        span: 24
      },
      {
        name: 'coverUrl',
        label: 'resource.products.field.coverUrl.label',
        component: 'upload',
        componentProps: {
          listType: 'picture-card',
          maxCount: 10,
          accept: 'image/*',
          multiple: true
        },
        span: 24
      },
      {
        name: 'isEnabled',
        label: 'resource.products.field.isEnabled.label',
        component: 'switch',
        initialValue: true,
        span: 12
      },
      {
        name: 'isVirtual',
        label: 'resource.products.field.isVirtual.label',
        component: 'switch',
        initialValue: false,
        span: 12
      }
    ]
  },
  // 商品管理 - 购物车信息（只读）
  {
    key: 'carts',
    title: 'resource.carts.title',
    basePath: '/api/carts',
    primaryKey: 'id',
    readOnly: true, // 设置为只读
    columns: [
      { title: 'resource.carts.column.username', dataIndex: 'username' },
      { title: 'resource.carts.column.realName', dataIndex: 'realName' },
      { title: 'resource.carts.column.phoneNumber', dataIndex: 'phoneNumber' },
      { title: 'resource.carts.column.itemCount', dataIndex: 'items', render: (items) => items ? items.length : 0 },
      { title: 'resource.carts.column.totalAmount', dataIndex: 'items', render: (items) => {
        if (!items || items.length === 0) return '0.00';
        const total = items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
        return total.toFixed(2);
      }},
      { title: 'resource.carts.column.createdAt', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString() : '-' },
      { title: 'resource.carts.column.updatedAt', dataIndex: 'updatedAt', render: (value) => value ? new Date(value).toLocaleString() : '-' }
    ],
    formFields: [] // 只读，不提供表单字段
  },
  // 养生馆 - 订单信息
  {
    key: 'orders',
    title: 'resource.orders.title',
    basePath: '/api/orders',
    primaryKey: 'id',
    columns: [
      { title: 'resource.orders.column.orderNo', dataIndex: 'orderNo' },
      { title: 'resource.orders.column.status', dataIndex: 'status' },
      { title: 'resource.orders.column.payStatus', dataIndex: 'payStatus' },
      { title: 'resource.orders.column.payMethod', dataIndex: 'payMethod' },
      { title: 'resource.orders.column.totalAmount', dataIndex: 'totalAmount' },
      { title: 'resource.orders.column.payAmount', dataIndex: 'payAmount' },
      { title: 'resource.orders.column.consignee', dataIndex: 'consignee' },
      { title: 'resource.orders.column.phone', dataIndex: 'phone' },
      { title: 'resource.orders.column.address', dataIndex: 'addressLine' },
      { title: 'resource.orders.column.createdAt', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString() : '-' },
      { title: 'resource.orders.column.updatedAt', dataIndex: 'updatedAt', render: (value) => value ? new Date(value).toLocaleString() : '-' }
    ],
    formFields: [] // 先只展示列表
  },
  // 养生馆 - 物流信息（发货单）
  {
    key: 'shipments',
    title: 'resource.shipments.title',
    basePath: '/api/shipments',
    primaryKey: 'id',
    columns: [
      { title: 'resource.shipments.column.trackingNo', dataIndex: 'trackingNo' },
      { title: 'resource.shipments.column.shipCompany', dataIndex: ['shipCompany', 'name'] },
      { title: 'resource.shipments.column.packageIndex', dataIndex: 'packageIndex' },
      { title: 'resource.shipments.column.status', dataIndex: 'status' },
      { title: 'resource.shipments.column.shippedAt', dataIndex: 'shippedAt', render: (value) => value ? new Date(value).toLocaleString() : '-' },
      { title: 'resource.shipments.column.deliveredAt', dataIndex: 'deliveredAt', render: (value) => value ? new Date(value).toLocaleString() : '-' },
      { title: 'resource.shipments.column.createdAt', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString() : '-' },
      { title: 'resource.shipments.column.updatedAt', dataIndex: 'updatedAt', render: (value) => value ? new Date(value).toLocaleString() : '-' }
    ],
    formFields: [] // 先只展示列表
  },
  // 疾病分类
  {
    key: 'disease-categories',
    title: 'resource.disease-categories.title',
    basePath: '/api/diseasecategories',
    primaryKey: 'id',
    columns: [
      { title: 'resource.disease-categories.column.departmentName', dataIndex: 'departmentName' },
      { title: 'resource.disease-categories.column.name', dataIndex: 'name' },
      { title: 'resource.disease-categories.column.symptoms', dataIndex: 'symptoms', ellipsis: true, width: 300 },
      { title: 'resource.disease-categories.column.createdAt', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString() : '-' }
    ],
    formFields: [
      { 
        name: 'departmentId', 
        label: 'resource.disease-categories.field.departmentId.label', 
        component: 'select', 
        componentProps: { placeholder: 'resource.disease-categories.field.departmentId.placeholder' }, 
        rules: [{ required: true, message: 'resource.disease-categories.field.departmentId.rule.required' }], 
        loadOptionsFrom: '/api/departments',
        span: 12 
      },
      { 
        name: 'name', 
        label: 'resource.disease-categories.field.name.label', 
        component: 'input', 
        componentProps: { placeholder: 'resource.disease-categories.field.name.placeholder' }, 
        rules: [{ required: true, message: 'resource.disease-categories.field.name.rule.required' }], 
        span: 12 
      },
      { 
        name: 'symptoms', 
        label: 'resource.disease-categories.field.symptoms.label', 
        component: 'textarea', 
        componentProps: { placeholder: 'resource.disease-categories.field.symptoms.placeholder', rows: 4 }, 
        span: 24 
      }
    ]
  },
  // 物流公司（枚举表）
  {
    key: 'ship-companies',
    title: 'resource.ship-companies.title',
    basePath: '/api/shipcompanies',
    primaryKey: 'id',
    columns: [
      { title: 'resource.ship-companies.column.name', dataIndex: 'name' },
      { title: 'resource.ship-companies.column.code', dataIndex: 'code' },
      { title: 'resource.ship-companies.column.contactUrl', dataIndex: 'contactUrl' },
      { title: 'resource.ship-companies.column.phone', dataIndex: 'phone' },
      { title: 'resource.ship-companies.column.isEnabled', dataIndex: 'isEnabled', valueType: 'boolean' },
      { title: 'resource.ship-companies.column.createdAt', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString() : '-' },
      { title: 'resource.ship-companies.column.updatedAt', dataIndex: 'updatedAt', render: (value) => value ? new Date(value).toLocaleString() : '-' }
    ],
    formFields: [
      { name: 'name', label: 'resource.ship-companies.field.name.label', component: 'input', componentProps: { placeholder: 'resource.ship-companies.field.name.placeholder' }, rules: [{ required: true, message: 'resource.ship-companies.field.name.rule.required' }], span: 12 },
      { name: 'code', label: 'resource.ship-companies.field.code.label', component: 'input', componentProps: { placeholder: 'resource.ship-companies.field.code.placeholder' }, rules: [{ required: true, message: 'resource.ship-companies.field.code.rule.required' }], span: 12 },
      { name: 'contactUrl', label: 'resource.ship-companies.field.contactUrl.label', component: 'input', componentProps: { placeholder: 'resource.ship-companies.field.contactUrl.placeholder' }, rules: [{ validator: (_, value) => { if (!value || value.trim() === '') return Promise.resolve(); const urlPattern = /^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/i; if (urlPattern.test(value.trim())) return Promise.resolve(); return Promise.reject(new Error('resource.ship-companies.field.contactUrl.rule.pattern')); } }], span: 12 },
      { name: 'phone', label: 'resource.ship-companies.field.phone.label', component: 'input', componentProps: { placeholder: 'resource.ship-companies.field.phone.placeholder' }, span: 12 },
      { name: 'isEnabled', label: 'resource.ship-companies.field.isEnabled.label', component: 'switch', initialValue: true, span: 12 }
    ]
  },
  // 三甲医院
  {
    key: 'tertiary-hospitals',
    title: 'resource.tertiary-hospitals.title',
    basePath: '/api/tertiaryhospitals',
    primaryKey: 'id',
    columns: [
      { title: 'resource.tertiary-hospitals.column.name', dataIndex: 'name' },
      { title: 'resource.tertiary-hospitals.column.provinceName', dataIndex: 'provinceName' },
      { title: 'resource.tertiary-hospitals.column.cityName', dataIndex: 'cityName' },
      { title: 'resource.tertiary-hospitals.column.address', dataIndex: 'address', ellipsis: true, width: 200 },
      { title: 'resource.tertiary-hospitals.column.level', dataIndex: 'level' },
      { title: 'resource.tertiary-hospitals.column.type', dataIndex: 'type' },
      { title: 'resource.tertiary-hospitals.column.phone', dataIndex: 'phone' },
      { title: 'resource.tertiary-hospitals.column.website', dataIndex: 'website', ellipsis: true, width: 200 },
      { title: 'resource.tertiary-hospitals.column.isEnabled', dataIndex: 'isEnabled', valueType: 'boolean' },
      { title: 'resource.tertiary-hospitals.column.createdAt', dataIndex: 'createdAt', render: (value) => value ? new Date(value).toLocaleString() : '-' }
    ],
    formFields: [
      { name: 'name', label: 'resource.tertiary-hospitals.field.name.label', component: 'input', componentProps: { placeholder: 'resource.tertiary-hospitals.field.name.placeholder' }, rules: [{ required: true, message: 'resource.tertiary-hospitals.field.name.rule.required' }], span: 12 },
      { name: 'provinceId', label: 'resource.tertiary-hospitals.field.provinceId.label', component: 'select', loadOptionsFrom: '/api/provinces', componentProps: { placeholder: 'resource.tertiary-hospitals.field.provinceId.placeholder' }, rules: [{ required: true, message: 'resource.tertiary-hospitals.field.provinceId.rule.required' }], span: 12 },
      { name: 'cityId', label: 'resource.tertiary-hospitals.field.cityId.label', component: 'select', componentProps: { placeholder: 'resource.tertiary-hospitals.field.cityId.placeholder', disabled: true }, rules: [{ required: true, message: 'resource.tertiary-hospitals.field.cityId.rule.required' }], span: 12 },
      { name: 'address', label: 'resource.tertiary-hospitals.field.address.label', component: 'input', componentProps: { placeholder: 'resource.tertiary-hospitals.field.address.placeholder' }, span: 12 },
      { name: 'level', label: 'resource.tertiary-hospitals.field.level.label', component: 'input', componentProps: { placeholder: 'resource.tertiary-hospitals.field.level.placeholder' }, initialValue: '三甲', span: 12 },
      { name: 'type', label: 'resource.tertiary-hospitals.field.type.label', component: 'input', componentProps: { placeholder: 'resource.tertiary-hospitals.field.type.placeholder' }, span: 12 },
      { name: 'phone', label: 'resource.tertiary-hospitals.field.phone.label', component: 'input', componentProps: { placeholder: 'resource.tertiary-hospitals.field.phone.placeholder' }, span: 12 },
      { name: 'website', label: 'resource.tertiary-hospitals.field.website.label', component: 'input', componentProps: { placeholder: 'resource.tertiary-hospitals.field.website.placeholder' }, rules: [{ validator: (_, value) => { if (!value || value.trim() === '') return Promise.resolve(); const urlPattern = /^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$/i; if (urlPattern.test(value.trim())) return Promise.resolve(); return Promise.reject(new Error('resource.tertiary-hospitals.field.website.rule.pattern')); } }], span: 12 },
      { name: 'latitude', label: 'resource.tertiary-hospitals.field.latitude.label', component: 'number', componentProps: { placeholder: 'resource.tertiary-hospitals.field.latitude.placeholder', step: 0.0000001, precision: 7, style: { width: '100%' } }, span: 24 },
      { name: 'longitude', label: 'resource.tertiary-hospitals.field.longitude.label', component: 'number', componentProps: { placeholder: 'resource.tertiary-hospitals.field.longitude.placeholder', step: 0.0000001, precision: 7, style: { width: '100%' } }, span: 24 },
      { name: 'sortOrder', label: 'resource.tertiary-hospitals.field.sortOrder.label', component: 'number', componentProps: { placeholder: 'resource.tertiary-hospitals.field.sortOrder.placeholder', min: 0 }, span: 12 },
      { name: 'isEnabled', label: 'resource.tertiary-hospitals.field.isEnabled.label', component: 'switch', initialValue: true, span: 12 }
    ]
  }
];

export default resourceConfig;


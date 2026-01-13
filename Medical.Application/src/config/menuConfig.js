import {
  AppstoreOutlined,
  DeploymentUnitOutlined,
  FileTextOutlined,
  MedicineBoxOutlined,
  QuestionCircleOutlined,
  ReconciliationOutlined,
  TeamOutlined,
  UserOutlined,
  MessageOutlined,
  SafetyCertificateOutlined,
  CommentOutlined,
  LikeOutlined,
  MoneyCollectOutlined,
  FundOutlined
} from '@ant-design/icons';
import resourceConfig from './resourceConfig.js';

const iconMap = {
  dashboard: AppstoreOutlined,
  users: UserOutlined,
  doctors: TeamOutlined,
  departments: DeploymentUnitOutlined,
  activities: ReconciliationOutlined,
  knowledge: FileTextOutlined,
  questions: QuestionCircleOutlined,
  consultations: AppstoreOutlined,
  'health-records': ReconciliationOutlined,
  medicines: MedicineBoxOutlined,
  'consultation-messages': MessageOutlined,
  'post-management': FileTextOutlined,
  posts: FileTextOutlined,
  'post-comments': CommentOutlined,
  'post-likes': LikeOutlined,
  feedbacks: MessageOutlined
};

// 查找资源配置
const findResource = (key) => resourceConfig.find(r => r.key === key);

const menuItems = [
  {
    key: 'dashboard',
    label: '首页',
    path: '/dashboard',
    icon: iconMap.dashboard
  },
  // 基础管理
  {
    key: 'basic-management',
    label: '基础管理',
    icon: DeploymentUnitOutlined,
    children: [
      {
        key: 'province-city',
        label: '省市信息',
        path: '/province-city',
        icon: DeploymentUnitOutlined
      },
      {
        key: 'disease-categories',
        label: '疾病分类',
        path: '/disease-categories-tree',
        icon: DeploymentUnitOutlined
      },
      {
        key: 'ship-companies',
        label: '物流公司',
        path: '/resources/ship-companies',
        icon: DeploymentUnitOutlined
      },
      {
        key: 'tertiary-hospitals',
        label: '三甲医院',
        path: '/resources/tertiary-hospitals',
        icon: DeploymentUnitOutlined
      }
    ]
  },
  // 患者管理
  {
    key: 'patient-management',
    label: '患者管理',
    icon: UserOutlined,
    children: [
      {
        key: 'patients',
        label: '患者信息',
        path: '/resources/patients',
        icon: UserOutlined
      },
      {
        key: 'patient-info',
        label: '患者信息库',
        path: '/resources/patient-info',
        icon: FileTextOutlined
      }
    ]
  },
  // 医生管理
  {
    key: 'doctor-management',
    label: '医生管理',
    icon: TeamOutlined,
    children: [
      {
        key: 'doctor-info',
        label: '医生信息',
        path: '/resources/doctors',
        icon: TeamOutlined
      },
      {
        key: 'patient-support-groups',
        label: '医生的患友会',
        path: '/resources/patient-support-groups',
        icon: TeamOutlined
      }
    ]
  },
  // 科室管理
  {
    key: 'department-management',
    label: '科室管理',
    icon: DeploymentUnitOutlined,
    children: [
      {
        key: 'department-info',
        label: '科室信息',
        path: '/resources/departments',
        icon: DeploymentUnitOutlined
      }
    ]
  },
  // 知识活动
  {
    key: 'knowledge-activity',
    label: '知识活动',
    icon: FileTextOutlined,
    children: [
      {
        key: 'activities',
        label: '活动管理',
        path: '/resources/activities',
        icon: ReconciliationOutlined
      },
      {
        key: 'health-knowledge',
        label: '健康知识',
        path: '/resources/knowledge',
        icon: FileTextOutlined
      }
    ]
  },
  // 发帖管理
  {
    key: 'post-management',
    label: '发帖管理',
    icon: FileTextOutlined,
    children: [
      {
        key: 'posts',
        label: '发帖信息',
        path: '/resources/posts',
        icon: FileTextOutlined
      },
      {
        key: 'post-comments',
        label: '评论信息',
        path: '/resources/post-comments',
        icon: CommentOutlined
      },
      {
        key: 'post-likes',
        label: '点赞信息',
        path: '/resources/post-likes',
        icon: LikeOutlined
      }
    ]
  },
  // 问诊咨询
  {
    key: 'consultation',
    label: '问诊咨询',
    icon: QuestionCircleOutlined,
    children: [
      {
        key: 'questions',
        label: '提问管理',
        path: '/resources/questions',
        icon: QuestionCircleOutlined
      },
      {
        key: 'consultations',
        label: '咨询管理',
        path: '/resources/consultations',
        icon: AppstoreOutlined
      },
      // 咨询消息管理
      {
        key: 'consultation-messages',
        label: '咨询消息',
        path: '/resources/consultation-messages',
        icon: MessageOutlined
      }
    ]
  },
  // 诊疗管理
  {
    key: 'treatment-management',
    label: '诊疗管理',
    icon: MedicineBoxOutlined,
    children: [
      {
        key: 'medical-records',
        label: '病历信息',
        path: '/resources/medical-records',
        icon: FileTextOutlined
      },
      {
        key: 'visit-records',
        label: '就诊记录',
        path: '/resources/visit-records',
        icon: FileTextOutlined
      },
      {
        key: 'examination-reports',
        label: '检查报告',
        path: '/resources/examination-reports',
        icon: FileTextOutlined
      },
      {
        key: 'prescriptions',
        label: '处方生成',
        path: '/resources/prescriptions',
        icon: MedicineBoxOutlined
      },
      {
        key: 'my-prescriptions',
        label: '我的处方',
        path: '/resources/my-prescriptions',
        icon: MedicineBoxOutlined
      }
    ]
  },
  // 药品管理
  {
    key: 'drug-management',
    label: '药品管理',
    icon: MedicineBoxOutlined,
    children: [
      {
        key: 'drug-categories',
        label: '药品分类',
        path: '/resources/drug-categories',
        icon: FileTextOutlined
      },
      {
        key: 'drugs',
        label: '药品信息',
        path: '/resources/drugs',
        icon: MedicineBoxOutlined
      },
      {
        key: 'drug-inventories',
        label: '药品库存',
        path: '/resources/drug-inventories',
        icon: ReconciliationOutlined
      },
      {
        key: 'drug-stock-ins',
        label: '药品入库',
        path: '/resources/drug-stock-ins',
        icon: FileTextOutlined
      }
    ]
  },
  // 商品管理
  {
    key: 'mall',
    label: '商品管理',
    icon: AppstoreOutlined,
    children: [
      {
        key: 'product-categories',
        label: '商品分类',
        path: '/resources/product-categories',
        icon: FileTextOutlined
      },
      {
        key: 'products',
        label: '商品信息',
        path: '/resources/products',
        icon: AppstoreOutlined
      },
      {
        key: 'cart-tree',
        label: '购物车',
        path: '/cart-tree',
        icon: FileTextOutlined
      },
      {
        key: 'orders',
        label: '订单信息',
        path: '/resources/orders',
        icon: ReconciliationOutlined
      },
      {
        key: 'shipments',
        label: '物流信息',
        path: '/resources/shipments',
        icon: DeploymentUnitOutlined
      }
    ]
  },
  // 财务管理
  {
    key: 'financial-management',
    label: '财务管理',
    icon: MoneyCollectOutlined,
    children: [
      {
        key: 'financial-receivables',
        label: '应收管理',
        path: '/resources/financial-receivables',
        icon: FundOutlined
      },
      {
        key: 'financial-payables',
        label: '应付管理',
        path: '/resources/financial-payables',
        icon: FundOutlined
      },
      {
        key: 'financial-fees',
        label: '费用管理',
        path: '/resources/financial-fees',
        icon: FileTextOutlined
      },
      {
        key: 'financial-settlements',
        label: '结算管理',
        path: '/resources/financial-settlements',
        icon: ReconciliationOutlined
      }
    ]
  },
  // 系统管理
  {
    key: 'system-management',
    label: '系统管理',
    icon: AppstoreOutlined,
    children: [
      {
        key: 'system-users',
        label: '用户管理',
        path: '/resources/system-users',
        icon: UserOutlined
      },
      {
        key: 'system-roles',
        label: '角色管理',
        path: '/resources/system-roles',
        icon: TeamOutlined
      },
      {
        key: 'permission-config',
        label: '权限配置',
        path: '/permission-config',
        icon: DeploymentUnitOutlined
      },
      {
        key: 'system-permissions',
        label: '权限管理',
        path: '/resources/system-permissions',
        icon: SafetyCertificateOutlined
      },
      {
        key: 'feedbacks',
        label: '反馈投诉',
        path: '/resources/feedbacks',
        icon: MessageOutlined
      }
    ]
  }
];

export default menuItems;

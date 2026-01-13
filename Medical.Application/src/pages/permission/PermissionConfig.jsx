import { useState, useEffect, useMemo } from 'react';
import { Table, Button, Modal, Form, Select, Input, message, Space, Tag, Tree, Row, Col, Checkbox, Card } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, SearchOutlined } from '@ant-design/icons';
import httpClient from '../../api/httpClient.js';
import menuItems from '../../config/menuConfig.js';
import { useLanguage } from '../../contexts/LanguageContext.jsx';

const { Option } = Select;

function PermissionConfig() {
  const { t } = useLanguage();
  
  // 权限操作类型（使用翻译）
  const PERMISSION_ACTIONS = [
    { key: 'view', label: t('permission.view') },
    { key: 'create', label: t('permission.create') },
    { key: 'update', label: t('permission.update') },
    { key: 'delete', label: t('permission.delete') }
  ];
  const [rolePermissions, setRolePermissions] = useState([]);
  const [filteredRolePermissions, setFilteredRolePermissions] = useState([]);
  const [roles, setRoles] = useState([]);
  const [loading, setLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState(null);
  const [form] = Form.useForm();
  const [searchKeyword, setSearchKeyword] = useState('');

  // 构建三级树形数据结构（菜单->子菜单->权限操作）
  const buildPermissionTree = () => {
    const treeData = [];
    
    // 菜单键到翻译键的映射（与AdminLayout中的一致）
    const menuKeyToTranslation = {
      'dashboard': 'menu.dashboard',
      'basic-management': 'menu.basicManagement',
      'province-city': 'menu.provinceCity',
      'patient-management': 'menu.patientManagement',
      'patients': 'menu.patientInfo',
      'patient-info': 'menu.patientInfoLib',
      'doctor-management': 'menu.doctorManagement',
      'doctor-info': 'menu.doctorInfo',
      'patient-support-groups': 'menu.patientSupportGroups',
      'department-management': 'menu.departmentManagement',
      'department-info': 'menu.departmentInfo',
      'knowledge-activity': 'menu.knowledgeActivity',
      'activities': 'menu.activities',
      'health-knowledge': 'menu.healthKnowledge',
      'post-management': 'menu.postManagement',
      'posts': 'menu.posts',
      'post-comments': 'menu.postComments',
      'post-likes': 'menu.postLikes',
      'consultation': 'menu.consultation',
      'questions': 'menu.questions',
      'consultations': 'menu.consultations',
      'treatment-management': 'menu.treatmentManagement',
      'visit-records': 'menu.visitRecords',
      'examination-reports': 'menu.examinationReports',
      'prescriptions': 'menu.prescriptions',
      'my-prescriptions': 'menu.myPrescriptions',
      'drug-management': 'menu.drugManagement',
      'drug-categories': 'menu.drugCategories',
      'drugs': 'menu.drugs',
      'drug-inventories': 'menu.drugInventories',
      'drug-stock-ins': 'menu.drugStockIns',
      'system-management': 'menu.systemManagement',
      'system-users': 'menu.systemUsers',
      'system-roles': 'menu.systemRoles',
      'permission-config': 'menu.permissionConfig',
      'system-permissions': 'menu.systemPermissions',
      'feedbacks': 'menu.feedbacks'
    };
    
    const getMenuLabel = (key, defaultLabel) => {
      const translationKey = menuKeyToTranslation[key];
      return translationKey ? t(translationKey) : defaultLabel;
    };
    
    menuItems.forEach(item => {
      if (item.path) {
        // 一级菜单（没有子菜单）
        const permissionChildren = PERMISSION_ACTIONS.map(action => ({
          title: action.label,
          key: `${item.key}.${action.key}`, // 格式：menuKey.action
          isLeaf: true
        }));
        
        treeData.push({
          title: getMenuLabel(item.key, item.label),
          key: item.key,
          children: permissionChildren,
          isLeaf: false
        });
      } else if (item.children && item.children.length > 0) {
        // 一级菜单（有子菜单）
        const menuChildren = item.children.map(child => {
          const permissionChildren = PERMISSION_ACTIONS.map(action => ({
            title: action.label,
            key: `${child.key}.${action.key}`, // 格式：menuKey.action
            isLeaf: true
          }));
          
          return {
            title: getMenuLabel(child.key, child.label),
            key: child.key,
            children: permissionChildren,
            isLeaf: false
          };
        });
        
        treeData.push({
          title: getMenuLabel(item.key, item.label),
          key: item.key,
          children: menuChildren,
          isLeaf: false
        });
      }
    });
    
    return treeData;
  };

  const permissionTreeData = useMemo(() => buildPermissionTree(), [PERMISSION_ACTIONS, t]);

  // 获取所有叶子节点（权限代码）
  const getAllPermissionKeys = (treeData) => {
    const keys = [];
    const traverse = (nodes) => {
      nodes.forEach(node => {
        if (node.isLeaf) {
          keys.push(node.key);
        }
        if (node.children) {
          traverse(node.children);
        }
      });
    };
    traverse(treeData);
    return keys;
  };

  const allPermissionKeys = useMemo(() => getAllPermissionKeys(permissionTreeData), [permissionTreeData]);

  // 全选状态
  const [checkedKeys, setCheckedKeys] = useState([]);
  const [checkAll, setCheckAll] = useState(false);

  // 获取角色权限配置列表（从 RolePermissions 表获取）
  const fetchRolePermissions = async () => {
    setLoading(true);
    try {
      // 获取所有角色及其权限
      const rolesData = await httpClient.get('/api/roles/all');
      const permissionsData = [];
      
      for (const role of rolesData || []) {
        try {
          const permissionCodes = await httpClient.get(`/api/menupermissions/role-permissions/${role.code}`);
          if (permissionCodes && permissionCodes.length > 0) {
            permissionsData.push({
              id: role.id,
              roleCode: role.code,
              roleName: role.name,
              permissionCount: permissionCodes.length,
              permissionCodes: permissionCodes
            });
          } else {
            permissionsData.push({
              id: role.id,
              roleCode: role.code,
              roleName: role.name,
              permissionCount: 0,
              permissionCodes: []
            });
          }
        } catch (error) {
          console.error(`获取角色 ${role.code} 的权限失败:`, error);
          permissionsData.push({
            id: role.id,
            roleCode: role.code,
            roleName: role.name,
            permissionCount: 0,
            permissionCodes: []
          });
        }
      }
      
      setRolePermissions(permissionsData);
      setFilteredRolePermissions(permissionsData);
    } catch (error) {
      message.error(t('permission.loadFailed'));
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  // 处理搜索
  const handleSearch = (value) => {
    const keyword = value || searchKeyword;
    if (!keyword || keyword.trim() === '') {
      setFilteredRolePermissions(rolePermissions);
      return;
    }

    const searchTerm = keyword.toLowerCase().trim();
    const filtered = rolePermissions.filter(record => {
      return (
        (record.roleCode && record.roleCode.toLowerCase().includes(searchTerm)) ||
        (record.roleName && record.roleName.toLowerCase().includes(searchTerm))
      );
    });
    setFilteredRolePermissions(filtered);
  };

  // 清空搜索
  const handleClearSearch = () => {
    setSearchKeyword('');
    setFilteredRolePermissions(rolePermissions);
  };

  // 获取所有角色列表
  const fetchRoles = async () => {
    try {
      const data = await httpClient.get('/api/roles/all');
      setRoles(data || []);
    } catch (error) {
      message.error(t('permission.loadFailed'));
      console.error(error);
    }
  };

  useEffect(() => {
    fetchRolePermissions();
    fetchRoles();
  }, []);

  // 当 rolePermissions 或 searchKeyword 变化时，更新过滤后的数据
  useEffect(() => {
    if (!searchKeyword || searchKeyword.trim() === '') {
      setFilteredRolePermissions(rolePermissions);
    } else {
      const keyword = searchKeyword.toLowerCase().trim();
      const filtered = rolePermissions.filter(record => {
        return (
          (record.roleCode && record.roleCode.toLowerCase().includes(keyword)) ||
          (record.roleName && record.roleName.toLowerCase().includes(keyword))
        );
      });
      setFilteredRolePermissions(filtered);
    }
  }, [rolePermissions, searchKeyword]);

  const handleAdd = () => {
    setEditingRecord(null);
    setCheckedKeys([]);
    setCheckAll(false);
    form.resetFields();
    setModalOpen(true);
  };

  // 处理全选
  const handleCheckAll = (e) => {
    const checked = e.target.checked;
    setCheckAll(checked);
    setCheckedKeys(checked ? allPermissionKeys : []);
  };

  // 处理树节点选择
  const handleTreeCheck = (checkedKeysValue) => {
    // Tree 组件的 checkedKeys 可能是数组或对象 {checked: [], halfChecked: []}
    const checked = Array.isArray(checkedKeysValue) 
      ? checkedKeysValue 
      : checkedKeysValue.checked || [];
    setCheckedKeys(checked);
    setCheckAll(checked.length === allPermissionKeys.length);
  };

  const handleEdit = async (record) => {
    setEditingRecord(record);
    
    // 获取该角色的所有权限代码
    try {
      const permissionCodes = await httpClient.get(`/api/menupermissions/role-permissions/${record.roleCode}`);
      setCheckedKeys(permissionCodes || []);
      setCheckAll((permissionCodes || []).length === allPermissionKeys.length);
    } catch (error) {
      console.error('获取角色权限失败:', error);
      setCheckedKeys([]);
      setCheckAll(false);
    }
    
    form.setFieldsValue({
      roleCode: record.roleCode
    });
    setModalOpen(true);
  };

  const handleDelete = async (id) => {
    Modal.confirm({
      title: t('common.confirm'),
      content: t('permission.deleteConfirm'),
      onOk: async () => {
        try {
          // 删除该角色的所有权限
          await httpClient.post('/api/menupermissions/role-permissions', {
            roleCode: rolePermissions.find(rp => rp.id === id)?.roleCode,
            permissionCodes: []
          });
          message.success(t('permission.deleteSuccess'));
          fetchRolePermissions();
        } catch (error) {
          message.error(t('permission.deleteFailed'));
          console.error(error);
        }
      }
    });
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      
      // 验证是否选择了权限
      if (!checkedKeys || checkedKeys.length === 0) {
        message.error(t('permission.selectAtLeastOne'));
        return;
      }

      // 验证是否选择了角色
      if (!values.roleCode) {
        message.error(t('permission.roleRequired'));
        return;
      }

      // 提交权限配置
      await httpClient.post('/api/menupermissions/role-permissions', {
        roleCode: values.roleCode,
        permissionCodes: checkedKeys
      });

      message.success(t('permission.saveSuccess'));
      setModalOpen(false);
      setEditingRecord(null);
      setCheckedKeys([]);
      setCheckAll(false);
      form.resetFields();
      fetchRolePermissions();
    } catch (error) {
      if (error.errorFields) {
        return; // 表单验证错误
      }
      message.error(t('permission.saveFailed'));
      console.error(error);
    }
  };

  // 当角色选择变化时，检查是否是 SuperAdmin 或 Admin，如果是则全选
  const handleRoleChange = async (roleCode) => {
    if (!roleCode) {
      setCheckedKeys([]);
      setCheckAll(false);
      return;
    }

    const selectedRole = roles.find(r => r.code === roleCode);
    if (selectedRole && (selectedRole.name === 'SuperAdmin' || selectedRole.name === 'Admin')) {
      // SuperAdmin 和 Admin 默认全选
      setCheckedKeys(allPermissionKeys);
      setCheckAll(true);
    } else {
      // 其他角色，如果是编辑模式，加载已有权限
      if (editingRecord && editingRecord.roleCode === roleCode) {
        try {
          const permissionCodes = await httpClient.get(`/api/menupermissions/role-permissions/${roleCode}`);
          setCheckedKeys(permissionCodes || []);
          setCheckAll((permissionCodes || []).length === allPermissionKeys.length);
        } catch (error) {
          console.error('获取角色权限失败:', error);
          setCheckedKeys([]);
          setCheckAll(false);
        }
      } else {
        setCheckedKeys([]);
        setCheckAll(false);
      }
    }
  };

  const columns = [
    {
      title: t('permission.roleCode'),
      dataIndex: 'roleCode',
      key: 'roleCode',
      width: 150
    },
    {
      title: t('permission.roleName'),
      dataIndex: 'roleName',
      key: 'roleName',
      width: 150
    },
    {
      title: t('permission.permissionCount'),
      dataIndex: 'permissionCount',
      key: 'permissionCount',
      width: 100,
      render: (count) => <Tag color="blue">{count}</Tag>
    },
    {
      title: t('resource.operation'),
      key: 'action',
      width: 150,
      render: (_, record) => (
        <Space>
          <Button
            type="link"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
          >
            {t('resource.edit')}
          </Button>
          <Button
            type="link"
            danger
            icon={<DeleteOutlined />}
            onClick={() => handleDelete(record.id)}
          >
            {t('resource.delete')}
          </Button>
        </Space>
      )
    }
  ];

  return (
    <div className="resource-page">
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <h2 style={{ margin: 0 }}>{t('permission.title')}</h2>
        <Space>
          <Space.Compact style={{ width: 300 }}>
            <Input
              placeholder={t('permission.searchPlaceholder')}
              allowClear
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              onPressEnter={(e) => handleSearch(e.target.value)}
            />
            <Button 
              type="primary" 
              icon={<SearchOutlined />}
              onClick={() => handleSearch(searchKeyword)}
            >
              {t('permission.search')}
            </Button>
          </Space.Compact>
          <Button type="primary" icon={<PlusOutlined />} onClick={handleAdd}>
            {t('resource.add')}
          </Button>
        </Space>
      </div>
      <Table
        columns={columns}
        dataSource={filteredRolePermissions}
        rowKey="id"
        loading={loading}
        pagination={{
          showSizeChanger: true,
          showTotal: (total) => t('resource.total', { count: total })
        }}
      />

      <Modal
        title={editingRecord ? `${t('resource.edit')}${t('permission.title')}` : `${t('resource.add')}${t('permission.title')}`}
        open={modalOpen}
        onOk={handleSubmit}
        onCancel={() => {
          setModalOpen(false);
          setEditingRecord(null);
          setCheckedKeys([]);
          setCheckAll(false);
          form.resetFields();
        }}
        okText={t('common.save')}
        cancelText={t('common.cancel')}
        width={1000}
      >
        <Row gutter={16}>
          {/* 左侧：权限树选择器 */}
          <Col span={16}>
            <Card 
              title={t('permission.selectPermissions')} 
              size="small"
              style={{ height: '500px', display: 'flex', flexDirection: 'column' }}
              bodyStyle={{ flex: 1, overflow: 'auto', padding: '12px' }}
            >
              <div style={{ marginBottom: '12px' }}>
                <Checkbox
                  checked={checkAll}
                  onChange={handleCheckAll}
                  indeterminate={checkedKeys.length > 0 && checkedKeys.length < allPermissionKeys.length}
                >
                  {t('permission.selectAll')}
                </Checkbox>
                <span style={{ marginLeft: '8px', color: '#999', fontSize: '12px' }}>
                  {t('permission.selectedCount', { current: checkedKeys.length, total: allPermissionKeys.length })}
                </span>
              </div>
              <Tree
                checkable
                checkedKeys={checkedKeys}
                onCheck={handleTreeCheck}
                treeData={permissionTreeData}
                defaultExpandAll
                style={{ fontSize: '14px' }}
              />
            </Card>
          </Col>

          {/* 右侧：角色选择 */}
          <Col span={8}>
            <Form form={form} layout="vertical">
              <Form.Item
                name="roleCode"
                label={t('permission.role')}
                rules={[{ required: true, message: t('permission.roleRequired') }]}
              >
                <Select
                  placeholder={t('permission.selectRole')}
                  showSearch
                  filterOption={(input, option) =>
                    (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                  }
                  disabled={!!editingRecord}
                  onChange={handleRoleChange}
                >
                  {roles.map(role => (
                    <Option key={role.code} value={role.code} label={role.name}>
                      {role.name}
                    </Option>
                  ))}
                </Select>
              </Form.Item>
            </Form>
          </Col>
        </Row>
      </Modal>
    </div>
  );
}

export default PermissionConfig;

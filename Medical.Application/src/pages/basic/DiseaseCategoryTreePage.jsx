import { useEffect, useState, useMemo, useCallback } from 'react';
import { Table, Button, Modal, Form, Input, Select, Space, Typography, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import httpClient from '../../api/httpClient.js';
import { useLanguage } from '../../contexts/LanguageContext.jsx';

const { TextArea } = Input;
const { Title } = Typography;

export default function DiseaseCategoryTreePage() {
  const { t } = useLanguage();
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(false);
  const [pagination, setPagination] = useState({
    current: 1,
    pageSize: 5,
    total: 0
  });
  const [modalOpen, setModalOpen] = useState(false);
  const [editingRecord, setEditingRecord] = useState(null);
  const [departmentOptions, setDepartmentOptions] = useState([]);
  const [form] = Form.useForm();

  useEffect(() => {
    fetchData(1, 5);
    loadDepartmentOptions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // 加载科室选项
  const loadDepartmentOptions = async () => {
    try {
      const response = await httpClient.get('/api/departments', {
        params: { page: 1, pageSize: 1000 }
      });
      const items = Array.isArray(response) ? response : response?.items ?? [];
      setDepartmentOptions(items.map(dept => ({
        label: dept.name,
        value: dept.id
      })));
    } catch (err) {
      console.error('加载科室列表失败:', err);
    }
  };

  const fetchData = useCallback(async (page = 1, pageSize = 5) => {
    setLoading(true);
    try {
      const response = await httpClient.get('/api/diseasecategories/tree', {
        params: { page, pageSize }
      });
      
      // 转换数据为树形结构
      const treeData = (response.items || response).map(department => ({
        key: department.id,
        id: department.id,
        name: department.name,
        sortOrder: department.sortOrder,
        type: 'department',
        children: (department.children || []).map(disease => ({
          key: disease.id,
          id: disease.id,
          name: disease.name,
          symptoms: disease.symptoms,
          departmentId: disease.departmentId,
          createdAt: disease.createdAt,
          type: 'disease'
        }))
      }));
      setData(treeData);
      
      // 更新分页信息
      setPagination(prev => ({
        ...prev,
        current: response.page ?? page,
        pageSize: response.pageSize ?? pageSize,
        total: response.total ?? 0
      }));
    } catch (err) {
      console.error('获取疾病分类数据失败:', err);
      message.error(err.message ?? t('resource.disease-categories.loadFailed'));
      setData([]);
    } finally {
      setLoading(false);
    }
  }, [t]);

  const handleTableChange = (newPagination, filters, sorter) => {
    fetchData(newPagination.current, newPagination.pageSize);
  };

  // 打开新建模态框
  const handleAdd = () => {
    setEditingRecord(null);
    form.resetFields();
    setModalOpen(true);
  };

  // 打开编辑模态框
  const handleEdit = useCallback((record) => {
    setEditingRecord(record);
    form.setFieldsValue({
      departmentId: record.departmentId,
      name: record.name,
      symptoms: record.symptoms
    });
    setModalOpen(true);
  }, [form]);

  // 删除
  const handleDelete = useCallback((record) => {
    Modal.confirm({
      title: t('resource.deleteConfirm'),
      content: `${t('resource.deleteConfirm')} "${record.name}"?`,
      okText: t('common.confirm'),
      cancelText: t('common.cancel'),
      onOk: async () => {
        try {
          await httpClient.delete(`/api/diseasecategories/${record.id}`);
          message.success(t('resource.deleteSuccess'));
          // 使用函数形式获取最新的 pagination 值并刷新数据
          setPagination(prev => {
            fetchData(prev.current, prev.pageSize);
            return prev;
          });
        } catch (err) {
          console.error('删除失败:', err);
          message.error(err.message ?? t('resource.deleteFailed'));
        }
      }
    });
  }, [t, fetchData]);

  // 提交表单
  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      
      // 确保数据格式正确（后端使用 camelCase）
      const payload = {
        departmentId: values.departmentId,
        name: (values.name?.trim() || '').trim(),
        symptoms: values.symptoms?.trim() || null
      };
      
      // 验证必填字段
      if (!payload.departmentId) {
        message.error(t('resource.disease-categories.field.departmentId.rule.required'));
        return;
      }
      if (!payload.name || payload.name.length === 0) {
        message.error(t('resource.disease-categories.field.name.rule.required'));
        return;
      }
      
      console.log('提交数据:', payload);
      console.log('departmentId 类型:', typeof payload.departmentId, payload.departmentId);
      console.log('name 值:', payload.name, '长度:', payload.name.length);
      
      if (editingRecord) {
        // 编辑
        await httpClient.put(`/api/diseasecategories/${editingRecord.id}`, payload);
        message.success(t('resource.saveSuccess'));
      } else {
        // 新建
        await httpClient.post('/api/diseasecategories', payload);
        message.success(t('resource.saveSuccess'));
      }
      setModalOpen(false);
      form.resetFields();
      setEditingRecord(null);
      // 使用函数形式获取最新的 pagination 值并刷新数据
      setPagination(prev => {
        fetchData(prev.current, prev.pageSize);
        return prev;
      });
    } catch (err) {
      console.error('保存失败:', err);
      console.error('错误响应:', JSON.stringify(err.response?.data, null, 2));
      console.error('错误状态码:', err.response?.status);
      console.error('完整错误对象:', err);
      
      // 提取错误消息
      let errorMessage = t('resource.saveFailed');
      
      if (err.response?.data) {
        const errorData = err.response.data;
        console.log('错误数据类型:', typeof errorData);
        console.log('错误数据内容:', errorData);
        
        if (typeof errorData === 'string') {
          errorMessage = errorData;
        } else if (errorData.message) {
          errorMessage = errorData.message;
          // 如果有 errors 数组，也添加到消息中
          if (errorData.errors && Array.isArray(errorData.errors) && errorData.errors.length > 0) {
            const errorDetails = errorData.errors.map(e => 
              e.field ? `${e.field}: ${Array.isArray(e.errors) ? e.errors.join(', ') : e.errors}` : 
              (Array.isArray(e.errors) ? e.errors.join(', ') : String(e.errors))
            ).join('; ');
            errorMessage = `${errorMessage} (${errorDetails})`;
          }
        } else if (errorData.errors && Array.isArray(errorData.errors)) {
          // 处理验证错误数组
          const errorMessages = errorData.errors.map(e => 
            e.field ? `${e.field}: ${Array.isArray(e.errors) ? e.errors.join(', ') : e.errors}` : 
            (Array.isArray(e.errors) ? e.errors.join(', ') : String(e.errors))
          ).join('; ');
          errorMessage = errorMessages || errorMessage;
        } else if (errorData.errors && typeof errorData.errors === 'object') {
          // 处理 ModelState 错误对象（ASP.NET Core 格式）
          const errorMessages = Object.keys(errorData.errors)
            .map(key => {
              const value = errorData.errors[key];
              if (Array.isArray(value)) {
                return `${key}: ${value.join(', ')}`;
              } else if (typeof value === 'object' && value.Errors) {
                return `${key}: ${Array.isArray(value.Errors) ? value.Errors.map(e => e.ErrorMessage || e).join(', ') : value.Errors}`;
              } else {
                return `${key}: ${String(value)}`;
              }
            })
            .join('; ');
          errorMessage = errorMessages || errorMessage;
        } else if (typeof errorData === 'object') {
          // 尝试提取任何可能的错误信息
          const keys = Object.keys(errorData);
          if (keys.length > 0) {
            errorMessage = JSON.stringify(errorData);
          }
        }
      } else if (err.message) {
        errorMessage = err.message;
      }
      
      console.log('最终错误消息:', errorMessage);
      message.error(errorMessage);
    }
  };

  const columns = useMemo(() => [
    {
      title: t('resource.disease-categories.column.name'),
      dataIndex: 'name',
      key: 'name',
      width: 250,
      render: (text, record) => (
        <span style={{ fontWeight: record.type === 'department' ? 'bold' : 'normal' }}>
          {text}
        </span>
      )
    },
    {
      title: t('resource.disease-categories.column.symptoms'),
      dataIndex: 'symptoms',
      key: 'symptoms',
      ellipsis: true,
      render: (text, record) => {
        if (record.type === 'department') {
          return null; // 科室行不显示症状
        }
        return text || '-';
      }
    },
    {
      title: t('resource.disease-categories.column.createdAt'),
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 180,
      render: (value, record) => {
        if (record.type === 'department') {
          return null; // 科室行不显示创建时间
        }
        return value ? new Date(value).toLocaleString() : '-';
      }
    },
    {
      title: t('resource.operation'),
      key: 'action',
      width: 150,
      render: (_, record) => {
        if (record.type === 'department') {
          return null; // 科室行不显示操作按钮
        }
        return (
          <Space>
            <Button
              type="link"
              size="small"
              icon={<EditOutlined />}
              onClick={() => handleEdit(record)}
            >
              {t('common.edit')}
            </Button>
            <Button
              type="link"
              size="small"
              danger
              icon={<DeleteOutlined />}
              onClick={() => handleDelete(record)}
            >
              {t('common.delete')}
            </Button>
          </Space>
        );
      }
    }
  ], [t, handleEdit, handleDelete]);

  return (
    <div style={{ padding: '24px' }}>
      <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Title level={4} style={{ margin: 0 }}>{t('resource.disease-categories.title')}</Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={handleAdd}
        >
          {t('common.add')}
        </Button>
      </div>
      <Table
        columns={columns}
        dataSource={data}
        loading={loading}
        pagination={{
          current: pagination.current,
          pageSize: pagination.pageSize,
          total: pagination.total,
          showSizeChanger: true,
          showTotal: (total) => t('resource.total', { count: total }),
          pageSizeOptions: ['5', '10', '20', '50'],
          onChange: (current, pageSize) => {
            setPagination(prev => ({ ...prev, current, pageSize }));
            fetchData(current, pageSize);
          },
          onShowSizeChange: (current, pageSize) => {
            setPagination(prev => ({ ...prev, current, pageSize }));
            fetchData(current, pageSize);
          }
        }}
        expandable={{
          defaultExpandAllRows: false,
          indentSize: 20
        }}
        rowKey="key"
      />
      <Modal
        title={editingRecord ? `${t('common.edit')} ${t('resource.disease-categories.title')}` : `${t('common.add')} ${t('resource.disease-categories.title')}`}
        open={modalOpen}
        onOk={handleSubmit}
        onCancel={() => {
          setModalOpen(false);
          form.resetFields();
        }}
        okText={t('common.confirm')}
        cancelText={t('common.cancel')}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
        >
          <Form.Item
            name="departmentId"
            label={t('resource.disease-categories.field.departmentId.label')}
            rules={[{ required: true, message: t('resource.disease-categories.field.departmentId.rule.required') }]}
          >
            <Select
              placeholder={t('resource.disease-categories.field.departmentId.placeholder')}
              options={departmentOptions}
              showSearch
              filterOption={(input, option) =>
                (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
            />
          </Form.Item>
          <Form.Item
            name="name"
            label={t('resource.disease-categories.field.name.label')}
            rules={[{ required: true, message: t('resource.disease-categories.field.name.rule.required') }]}
          >
            <Input placeholder={t('resource.disease-categories.field.name.placeholder')} />
          </Form.Item>
          <Form.Item
            name="symptoms"
            label={t('resource.disease-categories.field.symptoms.label')}
          >
            <TextArea
              placeholder={t('resource.disease-categories.field.symptoms.placeholder')}
              rows={4}
            />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}


import { useEffect, useMemo, useState } from 'react';
import { Table, Card, message, Typography, Button, Modal, Space } from 'antd';
import httpClient from '../../api/httpClient';
import resourceService from '../../api/resourceService';
import { useLanguage } from '../../contexts/LanguageContext';

const { Title } = Typography;

export default function CartTreePage() {
  const { t } = useLanguage();
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(false);

  const fetchData = async () => {
    setLoading(true);
    try {
      const res = await httpClient.get('/api/carts/tree');
      setData(res.items || []);
    } catch (err) {
      message.error(err.message ?? t('resource.loadFailed'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const parentColumns = useMemo(() => [
    { title: t('cartTree.username'), dataIndex: 'username', align: 'center' },
    { title: t('cartTree.realName'), dataIndex: 'realName', align: 'center' },
    { title: t('cartTree.phoneNumber'), dataIndex: 'phoneNumber', align: 'center' },
    { title: t('cartTree.itemCount'), dataIndex: 'itemCount', align: 'center' },
    { 
      title: t('cartTree.totalAmount'), 
      dataIndex: 'totalAmount',
      align: 'center',
      render: (v) => (v ?? 0).toFixed(2)
    },
    { 
      title: t('cartTree.createdAt'), 
      dataIndex: 'createdAt',
      align: 'center',
      render: (v) => v ? new Date(v).toLocaleString() : '-'
    },
    {
      title: t('cartTree.action'),
      dataIndex: 'actions',
      align: 'center',
      render: (_, record) => (
        <Space>
          <Button danger type="link" onClick={() => handleClear(record)}>
            {t('cartTree.clear')}
          </Button>
        </Space>
      )
    }
  ], [t]);

  const childColumns = useMemo(() => [
    { title: t('cartTree.productName'), dataIndex: 'productName', width: 200, align: 'center' },
    { title: t('cartTree.specName'), dataIndex: 'specName', width: 150, align: 'center' },
    { title: t('cartTree.quantity'), dataIndex: 'quantity', width: 150, align: 'center' },
    { 
      title: t('cartTree.price'), 
      dataIndex: 'price',
      width: 150,
      align: 'center',
      render: (v) => (v ?? 0).toFixed(2)
    },
    { 
      title: t('cartTree.subtotal'), 
      dataIndex: 'subtotal',
      width: 150,
      align: 'center',
      render: (v) => (v ?? 0).toFixed(2)
    }
  ], [t]);

  const expandedRowRender = (record) => {
    const children = record.children || [];
    if (!children.length) return null;
    return (
      <Table
        rowKey="id"
        columns={childColumns}
        dataSource={children}
        pagination={false}
        size="small"
        // 子表不需要再次展开
        expandable={{ showExpandColumn: false }}
      />
    );
  };

  const handleClear = (record) => {
    Modal.confirm({
      title: t('cartTree.clearConfirm'),
      content: t('cartTree.clearContent'),
      onOk: async () => {
        try {
          // 按userId删除该用户的所有购物车及其商品项
          await httpClient.delete(`/api/carts/user/${record.userId}`);
          message.success(t('cartTree.clearSuccess'));
          fetchData();
        } catch (err) {
          message.error(err.message ?? t('cartTree.clearFailed'));
        }
      }
    });
  };

  return (
    <Card
      bordered={false}
      style={{ marginTop: 8, boxShadow: 'none' }}
      bodyStyle={{ padding: 16 }}
    >
      <Title level={4} style={{ marginTop: 0, marginBottom: 16 }}>
        {t('menu.cartTree')}
      </Title>
      <Table
        rowKey="id"
        loading={loading}
        columns={parentColumns}
        dataSource={data}
        childrenColumnName="__children" // 防止 antd 按 children 字段渲染树形空行
        expandable={{
          expandedRowRender,
          rowExpandable: (record) =>
            Array.isArray(record.children) && record.children.length > 0,
        }}
        pagination={false}
      />
    </Card>
  );
}


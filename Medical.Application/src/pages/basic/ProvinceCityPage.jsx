import { useEffect, useState, useMemo } from 'react';
import { Table, App } from 'antd';
import httpClient from '../../api/httpClient.js';
import { useLanguage } from '../../contexts/LanguageContext.jsx';

export default function ProvinceCityPage() {
  const { t } = useLanguage();
  const [data, setData] = useState([]);
  const [loading, setLoading] = useState(false);
  const { message: messageApi } = App.useApp();

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const response = await httpClient.get('/api/provinces/tree');
      // 转换数据为树形结构
      const treeData = response.map(province => ({
        key: province.id,
        id: province.id,
        name: province.name,
        code: province.code,
        shortName: province.shortName,
        sortOrder: province.sortOrder,
        type: 'province',
        children: (province.cities || province.children)?.map(city => ({
          key: city.id,
          id: city.id,
          name: city.name,
          code: city.code,
          shortName: city.shortName,
          sortOrder: city.sortOrder,
          provinceId: city.provinceId,
          type: 'city'
        })) || []
      }));
      setData(treeData);
    } catch (err) {
      console.error('获取省市数据失败:', err);
      messageApi.error(err.message ?? t('provinceCity.loadFailed'));
      setData([]);
    } finally {
      setLoading(false);
    }
  };

  const columns = useMemo(() => [
    {
      title: t('provinceCity.name'),
      dataIndex: 'name',
      key: 'name',
      width: 200,
      render: (text, record) => (
        <span style={{ fontWeight: record.type === 'province' ? 'bold' : 'normal' }}>
          {text}
        </span>
      )
    },
    {
      title: t('provinceCity.code'),
      dataIndex: 'code',
      key: 'code',
      width: 150
    },
    {
      title: t('provinceCity.shortName'),
      dataIndex: 'shortName',
      key: 'shortName',
      width: 100
    },
    {
      title: t('provinceCity.sortOrder'),
      dataIndex: 'sortOrder',
      key: 'sortOrder',
      width: 80
    }
  ], [t]);

  return (
    <div style={{ padding: '24px' }}>
      <Table
        columns={columns}
        dataSource={data}
        loading={loading}
        pagination={false}
        expandable={{
          defaultExpandAllRows: false,
          indentSize: 20
        }}
        rowKey="key"
      />
    </div>
  );
}


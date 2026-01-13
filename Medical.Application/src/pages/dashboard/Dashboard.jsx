import { Card, Col, Row, Statistic } from 'antd';
import { useEffect, useState } from 'react';
import ReactECharts from 'echarts-for-react';
import httpClient from '../../api/httpClient.js';
import resourceService from '../../api/resourceService.js';
import { useLanguage } from '../../contexts/LanguageContext.jsx';

export default function Dashboard() {
  const { t } = useLanguage();
  
  const summaryResources = [
    { key: 'patients', title: t('dashboard.patients'), basePath: '/api/dashboard/patient-count' },
    { key: 'doctors', title: t('dashboard.doctors'), basePath: '/api/doctors' },
    { key: 'consultations', title: t('dashboard.consultations'), basePath: '/api/consultations' }
  ];
  const [stats, setStats] = useState({});
  const [departmentDistribution, setDepartmentDistribution] = useState([]);
  const [doctorConsultationRanking, setDoctorConsultationRanking] = useState([]);
  const [doctorActivityRanking, setDoctorActivityRanking] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadStats = async () => {
      setLoading(true);
      try {
        // 加载统计数据
        const [patientCount, doctorCount, consultationMessageCount, deptDist, consultationRank, activityRank] = await Promise.all([
          httpClient.get('/api/dashboard/patient-count'),
          httpClient.get('/api/dashboard/doctor-count'),
          httpClient.get('/api/dashboard/consultation-message-count'),
          httpClient.get('/api/dashboard/department-patient-distribution'),
          httpClient.get('/api/dashboard/doctor-consultation-ranking'),
          httpClient.get('/api/dashboard/doctor-activity-ranking')
        ]);

        setStats({
          patients: patientCount,
          doctors: doctorCount,
          consultations: consultationMessageCount
        });

        setDepartmentDistribution(deptDist || []);
        setDoctorConsultationRanking(consultationRank || []);
        setDoctorActivityRanking(activityRank || []);
      } catch (error) {
        console.error('加载统计数据失败:', error);
      } finally {
        setLoading(false);
      }
    };

    loadStats();
  }, []);

  // 饼状图配置：科室的患者人数就诊占比
  const pieChartOption = {
    title: {
      text: t('dashboard.departmentDistribution'),
      left: 'center'
    },
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b}: {c} ({d}%)'
    },
    legend: {
      orient: 'vertical',
      left: 'left'
    },
    series: [
      {
        name: t('dashboard.patientCount'),
        type: 'pie',
        radius: '50%',
        data: departmentDistribution.map(item => ({
          value: item.value,
          name: item.name
        })),
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowOffsetX: 0,
            shadowColor: 'rgba(0, 0, 0, 0.5)'
          }
        }
      }
    ]
  };

  // 柱状图配置：医生的就诊次数排名
  const barChartOption = {
    title: {
      text: t('dashboard.doctorConsultationRanking'),
      left: 'center'
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      }
    },
    xAxis: {
      type: 'category',
      data: doctorConsultationRanking.map(item => item.name),
      axisLabel: {
        rotate: 45,
        interval: 0
      }
    },
    yAxis: {
      type: 'value',
      name: t('dashboard.consultationCount')
    },
    series: [
      {
        name: t('dashboard.consultationCount'),
        type: 'bar',
        data: doctorConsultationRanking.map(item => item.value),
        itemStyle: {
          color: '#1890ff'
        }
      }
    ]
  };

  // 二维图表配置：医生的活跃度排名（只显示前十名）
  const scatterChartOption = {
    title: {
      text: t('dashboard.doctorActivityRanking'),
      left: 'center'
    },
    tooltip: {
      trigger: 'item',
      formatter: (params) => {
        const data = params.data;
        return `${data.name}<br/>${t('dashboard.department')}: ${data.department}<br/>${t('dashboard.consultationCount')}: ${data.consultationCount}<br/>${t('dashboard.messageCount')}: ${data.messageCount}<br/>${t('dashboard.activity')}: ${data.activity}`;
      }
    },
    xAxis: {
      type: 'value',
      name: t('dashboard.consultationCount'),
      nameLocation: 'middle',
      nameGap: 30
    },
    yAxis: {
      type: 'value',
      name: t('dashboard.messageCount'),
      nameLocation: 'middle',
      nameGap: 50
    },
    series: [
      {
        name: t('dashboard.activity'),
        type: 'scatter',
        data: doctorActivityRanking.map(item => ({
          value: [item.consultationCount, item.messageCount, item.activity],
          name: item.name,
          department: item.department,
          consultationCount: item.consultationCount,
          messageCount: item.messageCount,
          activity: item.activity
        })),
        symbolSize: (data) => {
          // 根据活跃度（第三个值）调整点的大小
          const activity = data[2] || 10;
          return Math.sqrt(activity) * 3 + 10;
        },
        itemStyle: {
          color: '#52c41a'
        },
        label: {
          show: true,
          formatter: (params) => params.data.name,
          position: 'right',
          fontSize: 12
        }
      }
    ]
  };

  return (
    <div className="dashboard" style={{ padding: '24px' }}>
      {/* 统计卡片 */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        {summaryResources.map((card) => (
          <Col span={8} key={card.key}>
            <Card>
              <Statistic 
                title={card.title} 
                value={stats[card.key] ?? 0}
                loading={loading}
              />
            </Card>
          </Col>
        ))}
      </Row>

      {/* 图表区域 */}
      <Row gutter={[16, 16]}>
        {/* 饼状图：科室的患者人数就诊占比 */}
        <Col span={12}>
          <Card title={t('dashboard.departmentDistribution')} loading={loading}>
            {departmentDistribution.length > 0 ? (
              <ReactECharts option={pieChartOption} style={{ height: '400px' }} />
            ) : (
              <div style={{ textAlign: 'center', padding: '40px', color: '#999' }}>
                {t('dashboard.noData')}
              </div>
            )}
          </Card>
        </Col>

        {/* 柱状图：医生的就诊次数排名 */}
        <Col span={12}>
          <Card title={t('dashboard.doctorConsultationRanking')} loading={loading}>
            {doctorConsultationRanking.length > 0 ? (
              <ReactECharts option={barChartOption} style={{ height: '400px' }} />
            ) : (
              <div style={{ textAlign: 'center', padding: '40px', color: '#999' }}>
                {t('dashboard.noData')}
              </div>
            )}
          </Card>
        </Col>
      </Row>

      {/* 二维图表：医生的活跃度排名 */}
      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col span={24}>
          <Card title={t('dashboard.doctorActivityRanking')} loading={loading}>
            {doctorActivityRanking.length > 0 ? (
              <ReactECharts option={scatterChartOption} style={{ height: '500px' }} />
            ) : (
              <div style={{ textAlign: 'center', padding: '40px', color: '#999' }}>
                {t('dashboard.noData')}
              </div>
            )}
          </Card>
        </Col>
      </Row>
    </div>
  );
}


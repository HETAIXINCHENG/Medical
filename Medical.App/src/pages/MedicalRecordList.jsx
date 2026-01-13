import { useNavigate, useSearchParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useState, useEffect } from 'react';
import { medicalApi } from '../services/medicalApi.js';

export default function MedicalRecordList() {
  usePageStyles('medical-record-list.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const doctorId = searchParams.get('doctorId');
  const type = searchParams.get('type');
  const patientId = searchParams.get('patientId');
  // 如果是从"我的病历"跳转过来的（没有doctorId和type），则隐藏"填写新病历"按钮
  const isFromMyRecords = !doctorId && !type;
  const [records, setRecords] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deleteRecordId, setDeleteRecordId] = useState(null);

  useEffect(() => {
    const loadRecords = async () => {
      try {
        // 如果有 patientId 就传，没有的话后端会根据当前登录用户自动返回病历
        const params = patientId ? { patientId } : {};
        const data = await medicalApi.getMedicalRecords(params);
        const items = Array.isArray(data?.items) ? data.items : Array.isArray(data) ? data : [];

        const mapped = items.map((item) => ({
          id: item.id,
          date: item.createdAt ? String(item.createdAt).split('T')[0] : '',
          patientName: item.patient?.realName || '未知',
          disease: item.diseaseName || '',
          description: item.diseaseDescription || ''
        }));

        setRecords(mapped);
      } catch (err) {
        console.error('加载病历列表失败', err);
        setRecords([]);
      } finally {
        setLoading(false);
      }
    };

    loadRecords();
  }, [patientId]);

  const handleRecordSelect = (recordId) => {
    navigate(`/medical-record-detail?doctorId=${doctorId}&type=${type}&patientId=${patientId}&recordId=${recordId}`);
  };

  const handleAddRecord = () => {
    navigate(`/consultation-form?doctorId=${doctorId}&type=${type}&patientId=${patientId}`);
  };

  const handleDeleteClick = (e, recordId) => {
    e.stopPropagation();
    setDeleteRecordId(recordId);
    setShowDeleteModal(true);
  };

  const handleDeleteConfirm = async () => {
    if (!deleteRecordId) return;

    try {
      await medicalApi.deleteMedicalRecord(deleteRecordId);
      setRecords((prev) => prev.filter((r) => r.id !== deleteRecordId));
      setShowDeleteModal(false);
      setDeleteRecordId(null);
    } catch (err) {
      console.error('删除病历失败', err);
      alert('删除病历失败，请稍后重试');
      setShowDeleteModal(false);
      setDeleteRecordId(null);
    }
  };

  const handleDeleteCancel = () => {
    setShowDeleteModal(false);
    setDeleteRecordId(null);
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="consultation-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/return.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">病历列表</h1>
      </div>

      {/* 内容区域 */}
      <div className="content-area">
        <p className="prompt-text">请选择历史病历</p>

        {/* 新建病历按钮 - 只在问诊流程中显示，从"我的病历"跳转时不显示 */}
        {!isFromMyRecords && (
          <div className="add-record-box" onClick={handleAddRecord}>
            <span className="add-icon">+</span>
            <span className="add-text">填写新病历</span>
          </div>
        )}

        {/* 病历列表：如果没有病历，则不显示下面区域 */}
        {loading ? (
          <div className="loading">加载中...</div>
        ) : records.length > 0 ? (
          <div className="record-list">
            {records.map((record, index) => (
              <div
                key={record.id}
                className="record-card"
                onClick={() => handleRecordSelect(record.id)}
              >
                <div className="record-header">
                  <span className="record-title">
                    病历: {record.patientName} {record.date}
                  </span>
                  <button
                    className="delete-btn"
                    onClick={(e) => handleDeleteClick(e, record.id)}
                  >
                    <span className="delete-icon">🗑</span>
                    删除
                  </button>
                </div>
                <div className="record-content">
                  <div className="record-item">
                    <span className="record-label">疾病:</span>
                    <span className="record-value">{record.disease}</span>
                  </div>
                  <div className="record-item">
                    <span className="record-label">病情描述:</span>
                    <span className="record-value">{record.description}</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : null}
      </div>

      {/* 删除确认模态框 */}
      {showDeleteModal && (
        <div className="delete-modal-overlay" onClick={handleDeleteCancel}>
          <div className="delete-modal" onClick={(e) => e.stopPropagation()}>
            <div className="delete-modal-title">确认删除此病历?</div>
            <div className="delete-modal-buttons">
              <button className="delete-modal-cancel" onClick={handleDeleteCancel}>取消</button>
              <div className="delete-modal-divider"></div>
              <button className="delete-modal-confirm" onClick={handleDeleteConfirm}>删除</button>
            </div>
          </div>
        </div>
      )}
    </PageLayout>
  );
}


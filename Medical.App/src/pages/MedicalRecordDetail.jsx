import { useNavigate, useSearchParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { useState, useEffect } from 'react';
import { medicalApi } from '../services/medicalApi.js';

export default function MedicalRecordDetail() {
  usePageStyles('medical-record-detail.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const doctorId = searchParams.get('doctorId');
  const type = searchParams.get('type');
  const patientId = searchParams.get('patientId');
  const recordId = searchParams.get('recordId');
  // 如果是从"我的病历"跳转过来的（没有doctorId和type，或者值为"null"），则隐藏"填写"按钮
  const isFromMyRecords = (!doctorId || doctorId === 'null') && (!type || type === 'null');
  const [record, setRecord] = useState(null);
  const [loading, setLoading] = useState(true);
  const [editingField, setEditingField] = useState(null); // 'diseaseName' 或 'diseaseDescription'
  const [editValue, setEditValue] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    const loadRecord = async () => {
      if (!recordId) {
        setRecord(null);
        setLoading(false);
        return;
      }
      try {
        const data = await medicalApi.getMedicalRecordById(recordId);
        setRecord(data);
      } catch (err) {
        console.error('加载病历详情失败', err);
        setRecord(null);
      } finally {
        setLoading(false);
      }
    };
    loadRecord();
  }, [recordId]);


  // 处理点击"填写"按钮
  const handleFillClick = (field) => {
    setEditingField(field);
    // 初始化输入框值为空（追加模式）
    setEditValue('');
  };

  // 处理保存修改
  const handleSave = async () => {
    if (!recordId || !editingField) return;

    if (saving) return;
    setSaving(true);

    try {
      const updateData = {};
      if (editingField === 'diseaseName') {
        updateData.diseaseName = editValue.trim();
      } else if (editingField === 'diseaseDescription') {
        updateData.diseaseDescription = editValue.trim();
      }

      if (!updateData[editingField]) {
        alert('请输入内容');
        setSaving(false);
        return;
      }

      const response = await medicalApi.updateMedicalRecord(recordId, updateData);
      console.log('更新响应:', response);
      
      // 重新加载病历数据
      const updatedData = await medicalApi.getMedicalRecordById(recordId);
      console.log('重新加载的数据:', updatedData);
      setRecord(updatedData);
      
      // 关闭编辑模式
      setEditingField(null);
      setEditValue('');
      alert('保存成功');
    } catch (err) {
      console.error('保存病历失败', err);
      alert(err?.response?.data?.message || '保存失败，请稍后重试');
    } finally {
      setSaving(false);
    }
  };

  // 取消编辑
  const handleCancelEdit = () => {
    setEditingField(null);
    setEditValue('');
  };

  if (loading) {
    return (
      <PageLayout>
        <div className="loading">加载中...</div>
      </PageLayout>
    );
  }

  if (!record) {
    return (
      <PageLayout>
        <div className="empty">病历不存在</div>
      </PageLayout>
    );
  }

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="consultation-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/return.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">病历详情</h1>
        <a href="#" className="contact-service">联系客服</a>
      </div>

      {/* 警告提示 - 只在问诊流程中显示，从"我的病历"跳转时不显示 */}
      {!isFromMyRecords && (
        <div className="warning-banner">
          <span className="warning-icon">⚠</span>
          <span className="warning-text">
            提示: 病历不能修改, 只能追加补充; 如病情发生变化, 请返回填写新病历。
          </span>
        </div>
      )}

      {/* 内容区域 */}
      <div className="content-area">
        {/* 病历生成时间：绑定 MedicalRecord.CreatedAt */}
        <div className="info-section">
          <div className="section-bar"></div>
          <div className="section-content">
            <h3 className="section-title">病历生成时间</h3>
            <p className="section-value">
              {record.createdAt
                ? new Date(record.createdAt).toLocaleString()
                : '-'}
            </p>
          </div>
        </div>

        {/* 患者信息：只显示患者姓名 */}
        <div className="info-section">
          <div className="section-bar"></div>
          <div className="section-content">
            <h3 className="section-title">患者信息</h3>
            <p className="section-value">
              {record.patient?.realName || '未知'}
            </p>
          </div>
        </div>

        {/* 疾病名称：MedicalRecord.DiseaseName */}
        <div className="info-section">
          <div className="section-bar"></div>
          <div className="section-content">
            <h3 className="section-title">疾病名称</h3>
            <div className="section-value-row">
              <span>
                {record.diseaseName || '-'}
              </span>
              {!isFromMyRecords && (
                <button 
                  className="fill-link" 
                  onClick={() => handleFillClick('diseaseName')}
                  disabled={editingField === 'diseaseName'}
                >
                  填写
                </button>
              )}
            </div>
            {editingField === 'diseaseName' && (
              <div className="edit-input-group">
                <input
                  type="text"
                  className="edit-input"
                  placeholder="请输入要追加的疾病名称"
                  value={editValue}
                  onChange={(e) => setEditValue(e.target.value)}
                  autoFocus
                />
              </div>
            )}
          </div>
        </div>

        {/* 详细病情：绑定 DiseaseDescription 等 */}
        <div className="info-section">
          <div className="section-bar"></div>
          <div className="section-content">
            <h3 className="section-title">详细病情</h3>
            <div className="subsection">
              <h4 className="subsection-title">[病情描述]</h4>
              <div className="subsection-content-row">
                <p className="subsection-content">
                  {record.diseaseDescription || '暂无描述'}
                </p>
                {!isFromMyRecords && (
                  <button 
                    className="fill-link" 
                    onClick={() => handleFillClick('diseaseDescription')}
                    disabled={editingField === 'diseaseDescription'}
                  >
                    填写
                  </button>
                )}
              </div>
              {editingField === 'diseaseDescription' && (
                <div className="edit-input-group">
                  <textarea
                    className="edit-textarea"
                    placeholder="请输入要追加的病情描述"
                    value={editValue}
                    onChange={(e) => setEditValue(e.target.value)}
                    rows={4}
                    autoFocus
                  />
                </div>
              )}
            </div>
          </div>
        </div>

        {/* 既往史：只显示手术/放化疗及慢性病史，有内容时才展示 */}
        {record.majorTreatmentHistory && (
          <div className="info-section">
            <div className="section-bar"></div>
            <div className="section-content">
              <h3 className="section-title">既往史</h3>
              <div className="subsection">
                <h4 className="subsection-title">[手术/放化疗及慢性病史]</h4>
                <p className="subsection-content">
                  {record.majorTreatmentHistory}
                </p>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* 底部按钮 - 只在编辑模式时显示 */}
      {editingField && (
        <div className="action-buttons">
          <button className="btn-cancel" onClick={handleCancelEdit} disabled={saving}>
            取消
          </button>
          <button className="btn-save" onClick={handleSave} disabled={saving}>
            {saving ? '保存中...' : '保存'}
          </button>
        </div>
      )}
    </PageLayout>
  );
}


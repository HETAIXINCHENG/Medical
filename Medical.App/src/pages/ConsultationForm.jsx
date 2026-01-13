import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';

export default function ConsultationForm() {
  usePageStyles('consultation-form.css');
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const doctorId = searchParams.get('doctorId');
  const type = searchParams.get('type');
  const patientId = searchParams.get('patientId');
  const questionId = searchParams.get('questionId'); // 要填写的问题ID
  
  const [currentStep, setCurrentStep] = useState(1);
  const totalSteps = 11;
  
  // 表单数据
  const [formData, setFormData] = useState({
    height: '',
    weight: '',
    illnessDuration: '',
    diseaseName: '',
    hasVisitedHospital: '',
    hasMedication: '',
    currentMedications: '',
    isPregnant: '',
    hasMajorDisease: '',
    majorTreatmentHistory: '',
    hasDrugAllergy: '',
    allergyHistory: '',
    conditionDescription: ''
  });
  const [submitting, setSubmitting] = useState(false);

  const [editingQuestion, setEditingQuestion] = useState(questionId ? parseInt(questionId) : null);

  const questions = [
    { id: 1, title: '身高体重', key: 'heightWeight', answer: formData.height && formData.weight ? `${formData.height}cm; ${formData.weight}kg` : '' },
    { id: 2, title: '本次患病多久了?', key: 'illnessDuration', answer: formData.illnessDuration || '' },
    { id: 3, title: '疾病名称/症状', key: 'diseaseName', answer: formData.diseaseName || '' },
    { id: 4, title: '本次疾病去医院就诊过吗?', key: 'hasVisitedHospital', answer: formData.hasVisitedHospital || '' },
    { id: 5, title: '当前是否有正在使用的药物?', key: 'hasMedication', answer: formData.hasMedication || '' },
    { id: 6, title: '当前是否怀孕?', key: 'isPregnant', answer: formData.isPregnant || '' },
    { id: 7, title: '是否有手术、放化疗等重大疾病治疗经历及慢性病史?', key: 'hasMajorDisease', answer: formData.hasMajorDisease || '' },
    { id: 8, title: '是否有药物过敏历史?', key: 'hasDrugAllergy', answer: formData.hasDrugAllergy || '' },
    { id: 9, title: '病情描述', key: 'conditionDescription', answer: formData.conditionDescription || '' }
  ];

  const handleInputChange = (key, value) => {
    setFormData(prev => ({ ...prev, [key]: value }));
  };

  const handleQuestionClick = (questionId) => {
    setEditingQuestion(questionId);
  };

  const handleSaveAnswer = (questionId) => {
    setEditingQuestion(null);
    // 这里可以保存答案到后端
  };

  const handleConfirm = async () => {
    if (!patientId) {
      alert('未选择患者，无法保存病历');
      return;
    }

    // doctorId 必须是当前问诊医生的 ID
    if (!doctorId || doctorId === 'null' || doctorId === 'undefined') {
      alert('当前问诊医生信息缺失，请返回重新选择医生');
      return;
    }

    if (submitting) return;
    setSubmitting(true);

    try {
      const payload = {
        // patientId 由后端根据当前登录用户自动绑定，这里不再依赖
        doctorId,
        height: formData.height ? parseFloat(formData.height) : null,
        weight: formData.weight ? parseFloat(formData.weight) : null,
        diseaseDuration: formData.illnessDuration || null,
        diseaseName: formData.diseaseName || null,
        hasVisitedHospital:
          formData.hasVisitedHospital === '已就诊过'
            ? true
            : formData.hasVisitedHospital === '未就诊过'
            ? false
            : null,
        currentMedications: formData.currentMedications || null,
        isPregnant:
          formData.isPregnant === '怀孕'
            ? true
            : formData.isPregnant === '未怀孕'
            ? false
            : null,
        majorTreatmentHistory: formData.majorTreatmentHistory || null,
        allergyHistory: formData.allergyHistory || null,
        diseaseDescription: formData.conditionDescription || null
      };

      await medicalApi.createMedicalRecord(payload);
      navigate(`/medical-record-list?doctorId=${doctorId}&type=${type}&patientId=${patientId}`);
    } catch (err) {
      console.error('保存病历失败', err);
      alert(err?.response?.data?.message || '保存病历失败，请稍后重试');
    } finally {
      setSubmitting(false);
    }
  };

  const renderQuestionInput = (question) => {
    if (editingQuestion !== question.id) {
      return null;
    }

    switch (question.id) {
      case 1: // 身高体重
        return (
          <div className="question-input-group">
            <div className="input-row">
              <input
                type="number"
                placeholder="身高(cm)"
                value={formData.height}
                onChange={(e) => handleInputChange('height', e.target.value)}
                className="form-input"
              />
              <input
                type="number"
                placeholder="体重(kg)"
                value={formData.weight}
                onChange={(e) => handleInputChange('weight', e.target.value)}
                className="form-input"
              />
            </div>
            <button className="save-btn" onClick={() => handleSaveAnswer(question.id)}>保存</button>
          </div>
        );
      case 2: // 本次患病多久了
        return (
          <div className="question-input-group">
            <select
              value={formData.illnessDuration}
              onChange={(e) => handleInputChange('illnessDuration', e.target.value)}
              className="form-select"
            >
              <option value="">请选择</option>
              <option value="一周内">一周内</option>
              <option value="一个月内">一个月内</option>
              <option value="三个月内">三个月内</option>
              <option value="半年内">半年内</option>
              <option value="一年以上">一年以上</option>
            </select>
            <button className="save-btn" onClick={() => handleSaveAnswer(question.id)}>保存</button>
          </div>
        );
      case 3: // 疾病名称/症状
        return (
          <div className="question-input-group">
            <input
              type="text"
              placeholder="请输入疾病名称或症状"
              value={formData.diseaseName}
              onChange={(e) => handleInputChange('diseaseName', e.target.value)}
              className="form-input"
            />
            <button className="save-btn" onClick={() => handleSaveAnswer(question.id)}>保存</button>
          </div>
        );
      case 4: // 本次疾病去医院就诊过吗
        return (
          <div className="question-input-group">
            <div className="radio-group">
              <label className="radio-label">
                <input
                  type="radio"
                  name="hasVisitedHospital"
                  value="就诊过"
                  checked={formData.hasVisitedHospital === '就诊过'}
                  onChange={(e) => handleInputChange('hasVisitedHospital', e.target.value)}
                />
                <span>就诊过</span>
              </label>
              <label className="radio-label">
                <input
                  type="radio"
                  name="hasVisitedHospital"
                  value="未就诊过"
                  checked={formData.hasVisitedHospital === '未就诊过'}
                  onChange={(e) => handleInputChange('hasVisitedHospital', e.target.value)}
                />
                <span>未就诊过</span>
              </label>
            </div>
            <button className="save-btn" onClick={() => handleSaveAnswer(question.id)}>保存</button>
          </div>
        );
      case 5: // 当前是否有正在使用的药物
        return (
          <div className="question-input-group">
            <div className="radio-group">
              <label className="radio-label">
                <input
                  type="radio"
                  name="hasMedication"
                  value="有"
                  checked={formData.hasMedication === '有'}
                  onChange={(e) => handleInputChange('hasMedication', e.target.value)}
                />
                <span>有</span>
              </label>
              <label className="radio-label">
                <input
                  type="radio"
                  name="hasMedication"
                  value="没有"
                  checked={formData.hasMedication === '没有'}
                  onChange={(e) => handleInputChange('hasMedication', e.target.value)}
                />
                <span>没有</span>
              </label>
            </div>
            {formData.hasMedication === '有' && (
              <textarea
                placeholder="请输入当前正在使用的药物名称、剂量等信息"
                value={formData.currentMedications}
                onChange={(e) => handleInputChange('currentMedications', e.target.value)}
                className="form-textarea"
                rows={3}
                maxLength={1000}
              />
            )}
            <button className="save-btn" onClick={() => handleSaveAnswer(question.id)}>保存</button>
          </div>
        );
      case 6: // 当前是否怀孕
        return (
          <div className="question-input-group">
            <div className="radio-group">
              <label className="radio-label">
                <input
                  type="radio"
                  name="isPregnant"
                  value="怀孕"
                  checked={formData.isPregnant === '怀孕'}
                  onChange={(e) => handleInputChange('isPregnant', e.target.value)}
                />
                <span>怀孕</span>
              </label>
              <label className="radio-label">
                <input
                  type="radio"
                  name="isPregnant"
                  value="未怀孕"
                  checked={formData.isPregnant === '未怀孕'}
                  onChange={(e) => handleInputChange('isPregnant', e.target.value)}
                />
                <span>未怀孕</span>
              </label>
            </div>
            <button className="save-btn" onClick={() => handleSaveAnswer(question.id)}>保存</button>
          </div>
        );
      case 7: // 是否有手术、放化疗等重大疾病治疗经历及慢性病史
        return (
          <div className="question-input-group">
            <div className="radio-group">
              <label className="radio-label">
                <input
                  type="radio"
                  name="hasMajorDisease"
                  value="有"
                  checked={formData.hasMajorDisease === '有'}
                  onChange={(e) => handleInputChange('hasMajorDisease', e.target.value)}
                />
                <span>有</span>
              </label>
              <label className="radio-label">
                <input
                  type="radio"
                  name="hasMajorDisease"
                  value="没有"
                  checked={formData.hasMajorDisease === '没有'}
                  onChange={(e) => handleInputChange('hasMajorDisease', e.target.value)}
                />
                <span>没有</span>
              </label>
            </div>
            {formData.hasMajorDisease === '有' && (
              <textarea
                placeholder="请输入手术、放化疗等重大疾病治疗经历及慢性病史"
                value={formData.majorTreatmentHistory}
                onChange={(e) => handleInputChange('majorTreatmentHistory', e.target.value)}
                className="form-textarea"
                rows={3}
                maxLength={2000}
              />
            )}
            <button className="save-btn" onClick={() => handleSaveAnswer(question.id)}>保存</button>
          </div>
        );
      case 8: // 是否有药物过敏历史
        return (
          <div className="question-input-group">
            <div className="radio-group">
              <label className="radio-label">
                <input
                  type="radio"
                  name="hasDrugAllergy"
                  value="有"
                  checked={formData.hasDrugAllergy === '有'}
                  onChange={(e) => handleInputChange('hasDrugAllergy', e.target.value)}
                />
                <span>有</span>
              </label>
              <label className="radio-label">
                <input
                  type="radio"
                  name="hasDrugAllergy"
                  value="没有"
                  checked={formData.hasDrugAllergy === '没有'}
                  onChange={(e) => handleInputChange('hasDrugAllergy', e.target.value)}
                />
                <span>没有</span>
              </label>
            </div>
            {formData.hasDrugAllergy === '有' && (
              <textarea
                placeholder="请输入具体的药物过敏史"
                value={formData.allergyHistory}
                onChange={(e) => handleInputChange('allergyHistory', e.target.value)}
                className="form-textarea"
                rows={3}
                maxLength={1000}
              />
            )}
            <button className="save-btn" onClick={() => handleSaveAnswer(question.id)}>保存</button>
          </div>
        );
      case 9: // 病情描述
        return (
          <div className="question-input-group">
            <textarea
              placeholder="可在此处填写近期病情变化,如病情加重或缓解情况。(请勿填写姓名、联系方式等个人隐私信息)"
              value={formData.conditionDescription}
              onChange={(e) => handleInputChange('conditionDescription', e.target.value)}
              className="form-textarea"
              rows={6}
              maxLength={2000}
            />
            <div className="textarea-footer">
              <span className="char-count">{formData.conditionDescription.length}/2000字</span>
              <span className="min-chars">最少20字</span>
            </div>
            <button className="save-btn" onClick={() => handleSaveAnswer(question.id)}>保存</button>
          </div>
        );
      default:
        return null;
    }
  };

  return (
    <PageLayout>
      {/* 顶部导航 */}
      <div className="consultation-header">
        <button onClick={() => navigate(-1)} className="back-btn">
          <img src="/Img/return.png" alt="返回" className="back-icon" />
        </button>
        <h1 className="header-title">诊前信息收集</h1>
      </div>

      {/* 提示信息 */}
      <div className="instruction-banner">
        为了更好的帮助你,请认真填写资料
      </div>

      {/* 表单内容 */}
      <div className="form-content">
        {questions.map((question) => (
          <div key={question.id} className="question-card">
            <div className="question-header">
              <span className="question-number">{question.id}.</span>
              <span className="question-title">{question.title}</span>
              {question.answer && (
                <span className="edit-link" onClick={() => handleQuestionClick(question.id)}>
                  填写
                </span>
              )}
            </div>
            {question.answer ? (
              <div className="question-answer">{question.answer}</div>
            ) : (
              <div className="question-placeholder">未填写</div>
            )}
            {editingQuestion === question.id && renderQuestionInput(question)}
            {!question.answer && editingQuestion !== question.id && (
              <button className="fill-btn" onClick={() => handleQuestionClick(question.id)}>
                填写
              </button>
            )}
          </div>
        ))}
      </div>

      {/* 底部按钮 */}
      <div className="action-buttons">
        <button className="confirm-btn" onClick={handleConfirm} disabled={submitting}>
          下一步
        </button>
      </div>
    </PageLayout>
  );
}


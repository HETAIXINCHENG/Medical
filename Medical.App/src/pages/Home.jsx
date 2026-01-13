import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function Home() {
  usePageStyles('home.css');
  const { t } = useLanguage();
  const navigate = useNavigate();

  const [recommendDoctors, setRecommendDoctors] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [diseaseCategories, setDiseaseCategories] = useState([]);
  const [departmentCount, setDepartmentCount] = useState(0);
  const [diseaseCount, setDiseaseCount] = useState(0);
  const [hospitalCount, setHospitalCount] = useState(0);
  const [userProvince, setUserProvince] = useState(null); // 用户所在省份
  const [products, setProducts] = useState([]); // 商品列表
  const [articles, setArticles] = useState([]); // 健康知识文章列表
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // 格式化时间显示
  const formatTime = (dateString) => {
    if (!dateString) return '';
    try {
      const date = new Date(dateString);
      if (isNaN(date.getTime())) {
        return '';
      }
      const now = new Date();
      const diff = now - date;
      const days = Math.floor(diff / (1000 * 60 * 60 * 24));
      
      if (days === 0) {
        return '今天';
      } else if (days === 1) {
        return '昨天';
      } else if (days < 7) {
        return `${days}天前`;
      } else {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
      }
    } catch (e) {
      return '';
    }
  };

  useEffect(() => {
    const loadHomeData = async () => {
      setLoading(true);
      setError('');
      try {
        const [doctorData, deptData, deptCountData] = await Promise.all([
          medicalApi.getDoctors({ isRecommended: true, pageSize: 4 }),
          medicalApi.getDepartments({ pageSize: 6 }),
          medicalApi.getDepartments({ pageSize: 1 }) // 只获取总数
        ]);
        
        // 后端返回的是分页格式 { items, total, page, pageSize }，需要提取 items
        setRecommendDoctors(doctorData?.items ?? doctorData ?? []);
        setDepartments(deptData?.items ?? deptData ?? []);
        
        // 设置科室总数
        if (deptCountData?.total !== undefined) {
          setDepartmentCount(deptCountData.total);
        } else if (deptData?.total !== undefined) {
          setDepartmentCount(deptData.total);
        }
        
        // 单独获取疾病总数，避免Promise.all中一个失败影响其他
        try {
          const diseaseCountData = await medicalApi.getDiseaseCategories({ pageSize: 1 });
          console.log('疾病总数API返回数据:', diseaseCountData);
          // API返回格式: { items, total, page, pageSize }
          const total = diseaseCountData?.total ?? diseaseCountData?.data?.total;
          if (total !== undefined && total !== null) {
            setDiseaseCount(total);
            console.log('成功设置疾病总数为:', total);
          } else {
            console.warn('未能从返回数据中提取疾病总数，完整数据:', diseaseCountData);
          }
        } catch (err) {
          console.error('获取疾病总数失败:', err);
          // 如果API调用失败，保持diseaseCount为0，不显示默认值
        }

        // 单独获取三甲医院总数
        try {
          const hospitalCountData = await medicalApi.getTertiaryHospitals({ pageSize: 1 });
          console.log('三甲医院总数API返回数据:', hospitalCountData);
          // API返回格式: { items, total, page, pageSize }
          const total = hospitalCountData?.total ?? hospitalCountData?.data?.total;
          if (total !== undefined && total !== null) {
            setHospitalCount(total);
            console.log('成功设置三甲医院总数为:', total);
          } else {
            console.warn('未能从返回数据中提取三甲医院总数，完整数据:', hospitalCountData);
          }
        } catch (err) {
          console.error('获取三甲医院总数失败:', err);
          // 如果API调用失败，保持hospitalCount为0，不显示默认值
        }

        // 获取商品列表（只获取启用的商品，最多3个）
        try {
          const productData = await medicalApi.getProducts({ 
            pageSize: 3,
            // 可以添加 isEnabled: true 的筛选，如果后端支持的话
          });
          const productItems = productData?.items ?? productData ?? [];
          // 只显示启用的商品
          const enabledProducts = productItems.filter(p => p.isEnabled !== false);
          setProducts(enabledProducts.slice(0, 3)); // 最多显示3个
          console.log('成功加载商品数据:', enabledProducts.slice(0, 3));
        } catch (err) {
          console.error('获取商品列表失败:', err);
          // 如果API调用失败，保持products为空数组
          setProducts([]);
        }

        // 获取健康知识文章列表（健康科普分类，最多3篇）
        const defaultArticles = [
          {
            id: '1',
            title: '心脏健康的饮食建议',
            summary: '了解保护心脏健康的饮食原则,预防心血管疾病。',
            coverImageUrl: 'https://picsum.photos/200/200?random=1',
            authorName: '张医生',
            authorTitle: '主任医师',
            authorAvatar: '',
            authorDepartment: '心内科',
            authorHospital: '北京协和医院',
            createdAt: new Date(),
            readCount: 85,
            favoriteCount: 1
          },
          {
            id: '2',
            title: '天天洗还这么脏! 水杯一个部位可能正悄悄长霉菌, 越喝身体越差',
            summary: '相信被它创过的朋友都猜到了, 开门见山, 这个歹毒的「霉菌刺客」就是——吸管竹...',
            coverImageUrl: 'https://picsum.photos/200/200?random=2',
            authorName: '李医生',
            authorTitle: '主治医师',
            authorAvatar: '',
            authorDepartment: '感染科',
            authorHospital: '北京协和医院',
            createdAt: new Date(),
            readCount: 33,
            favoriteCount: 10
          },
          {
            id: '3',
            title: '臭衣服有救了! 这两个除味妙招, 比除味喷雾还管用',
            summary: '生活烦恼千千万, 衣服有异味高低得算一个! 像是压箱底的衣服被翻出来后, 夹杂着衣柜...',
            coverImageUrl: 'https://picsum.photos/200/200?random=3',
            authorName: '王医生',
            authorTitle: '副主任医师',
            authorAvatar: '',
            authorDepartment: '皮肤科',
            authorHospital: '北京协和医院',
            createdAt: new Date(),
            readCount: 17,
            favoriteCount: 7
          }
        ];

        try {
          // 使用与Discover页面"精选"标签相同的数据获取逻辑，但只取前3条
          const articleData = await medicalApi.getHealthKnowledge({ pageSize: 3 });
          const articleItems = articleData?.items ?? articleData ?? [];
          console.log('API返回的健康知识数据:', articleItems);
          
          if (Array.isArray(articleItems) && articleItems.length > 0) {
            // 处理API返回的数据，确保有所有必要字段
            // 后端已按ReadCount降序排序，直接取前3条
            const processedArticles = articleItems.slice(0, 3).map(item => ({
              id: item.id || item.Id,
              title: item.title || item.Title || '',
              summary: item.summary || item.Summary || '',
              coverImageUrl: item.coverImageUrl || item.CoverImageUrl || '',
              authorName: item.authorName || item.AuthorName || '',
              authorTitle: item.authorTitle || item.AuthorTitle || '',
              authorAvatar: item.authorAvatar || item.AuthorAvatar || '',
              authorDepartment: item.authorDepartment || item.AuthorDepartment || '',
              authorHospital: item.authorHospital || item.AuthorHospital || '',
              createdAt: item.createdAt || item.CreatedAt || new Date(),
              readCount: item.readCount || item.ReadCount || 0,
              favoriteCount: item.favoriteCount || item.FavoriteCount || 0
            }));
            setArticles(processedArticles);
            console.log('成功加载健康知识数据（前3条，与Discover页面一致）:', processedArticles);
          } else {
            // 如果API返回空数组，使用示例数据（只取前3条）
            console.log('API返回空数组，使用示例数据');
            setArticles(defaultArticles.slice(0, 3));
          }
        } catch (err) {
          console.error('获取健康知识失败:', err);
          // 如果API调用失败，使用示例数据（只取前3条）
          setArticles(defaultArticles.slice(0, 3));
        }

        // 通过后端API获取用户IP对应的省份信息
        try {
          const locationData = await medicalApi.getProvinceByIp();
          const provinceName = locationData?.province || '';
          
          if (provinceName) {
            setUserProvince(provinceName);
            console.log('成功设置用户省份:', provinceName);
          }
        } catch (err) {
          // 静默处理所有错误，避免控制台报错影响用户体验
          // IP定位失败不影响页面功能，只是不显示省份信息
          // 不输出任何错误日志，包括网络错误、超时等
        }

        // 疾病分类：当前接口权限不足会 403，为避免前端报错，默认不请求，使用静态文案
        setDiseaseCategories([]);
      } catch (err) {
        setError(err.message ?? t('loading-home'));
      } finally {
        setLoading(false);
      }
    };
    loadHomeData();
  }, [t]);

  return (
    <PageLayout>
      {/* 顶部搜索栏：用于快速检索医院或药品 */}
      <div className="search-section">
        <div className="search-bar">
          <img src="/Img/search.png" alt="搜索" className="search-icon" />
          <input type="text" placeholder={t('search-doctor-by-disease-hospital-name')} />
          <div className="search-divider"></div>
          <button className="search-btn">{t('search')}</button>
        </div>
      </div>

      {/* 按科室找 */}
      <div className="search-section-card">
        <div className="search-left-panel blue">
          <div className="search-title">按科室找</div>
          <div className="search-desc">覆盖{departmentCount > 0 ? departmentCount : departments.length > 0 ? departments.length : 0}个科室</div>
          <button  className="search-btn-panel"  onClick={() => navigate('/department?type=normal')} >
            去找专家
          </button>
        </div>
        <div className="search-right-panel">
          <div className="search-grid">
            {departments.slice(0, 5).map((dept) => {
              const heartFallback =
                dept.name &&
                (dept.name.includes('心血管') || dept.name.includes('心脑血管'))
                  ? '/Img/heart-50.png'
                  : null;
              const lungsFallback =
                dept.name && (dept.name.includes('呼吸') || dept.name.includes('肺'))
                  ? '/Img/lungs-50.png'
                  : null;
              const stomachFallback =
                dept.name && (dept.name.includes('消化') || dept.name.includes('肠胃'))
                  ? '/Img/stomach-50.png'
                  : null;
              const brainFallback =
                dept.name && dept.name.includes('神经')
                  ? '/Img/brain-50.png'
                  : null;
              const endocrineFallback =
                dept.name && dept.name.includes('内分泌')
                  ? '/Img/man-50.png'
                  : null;
              const iconUrl =
                dept.iconUrl ||
                heartFallback ||
                lungsFallback ||
                stomachFallback ||
                brainFallback ||
                endocrineFallback;
              const isFallbackIcon =
                (heartFallback && iconUrl === heartFallback) ||
                (lungsFallback && iconUrl === lungsFallback) ||
                (stomachFallback && iconUrl === stomachFallback) ||
                (brainFallback && iconUrl === brainFallback) ||
                (endocrineFallback && iconUrl === endocrineFallback);
              return (
                <Link
                  key={dept.id}
                  to={`/department?type=normal&deptId=${dept.id}`}
                  className="search-item"
                >
                  {iconUrl && (
                    <div className="search-item-icon">
                      <img
                        src={iconUrl}
                        alt={dept.name}
                        style={isFallbackIcon ? { width: 18, height: 18 } : undefined}
                      />
                    </div>
                  )}
                  <div className="search-item-text">{dept.name}</div>
                </Link>
              );
            })}
            <Link 
              to="/department?type=normal" 
              className="search-item more"
            >
              <div className="search-item-icon">
                <img src="/Img/menu-50.png" alt="更多" />
              </div>
              <div className="search-item-text">更多</div>
            </Link>
          </div>
        </div>
      </div>

      {/* 按疾病找 */}
      <div className="search-section-card">
        <div className="search-left-panel orange">
          <div className="search-title">按疾病找</div>
          <div className="search-desc">覆盖{diseaseCount > 0 ? diseaseCount : '...'}个疾病</div>
          <button 
            className="search-btn-panel" 
            onClick={() => navigate('/department?type=expert')}
          >
            去找专家
          </button>
        </div>
        <div className="search-right-panel">
          <div className="search-grid">
            {(diseaseCategories?.slice(0, 5)?.length
              ? diseaseCategories.slice(0, 5)
              : ['颈椎病', '高血压', '糖尿病', '咽喉炎', '痛风','鼻炎','焦虑症','湿疹']
            ).map((item, idx) => {
              const name = typeof item === 'string' ? item : item.name || item.title || '';
              return (
                <Link key={idx} to="/department?type=expert" className="search-item disease">
                  <div className="search-item-text">{name || '疾病'}</div>
                </Link>
              );
            })}
            <Link to="/department?type=expert" className="search-item disease more">
              <div className="search-item-text">更多</div>
            </Link>
          </div>
        </div>
      </div>

      {/* 按医院找 */}
      <div className="search-section-card">
        <div className="search-left-panel teal">
          <div className="search-title-teal">按医院找</div>
          <div className="search-desc-teal">{hospitalCount > 0 ? hospitalCount : '...'} 三甲医院</div>
          <button 
            className="search-btn-panel" 
            onClick={() => navigate('/doctors')}
          >
            去找专家
          </button>
        </div>
        <div className="search-right-panel">
          <div className="search-grid">
            <div 
              className="search-item hospital"
              onClick={() => {
                const province = userProvince || '广东';
                navigate(`/hospital-search?province=${province}`);
              }}
              style={{ cursor: 'pointer' }}
            >
              <div className="search-item-text">{userProvince || '全国'}</div>
              <div className="search-item-subtext">百强医院榜</div>
            </div>
            <div 
              className="search-item hospital"
              onClick={() => {
                console.log('点击百强医院榜，准备跳转到 /hospital-ranking');
                navigate('/hospital-ranking');
              }}
              style={{ cursor: 'pointer' }}
            >
              <div className="search-item-text">全国</div>
              <div className="search-item-subtext">百强医院榜</div>
            </div>
            <Link to="/tcm-specialty-ranking" className="search-item hospital">
              <div className="search-item-text">中医专科</div>
              <div className="search-item-subtext">影响力榜</div>
            </Link>
          </div>
        </div>
      </div>

      {/* 数据加载或错误提示 */}
      {loading && <div className="loading">{t('loading-home')}</div>}
      {error && <div className="error-tip">{error}</div>}

      {/* 名医工作室 */}
      <div className="section">
        <div className="section-header">
          <div className="section-title-group">
            <h2 className="section-title">精选名医</h2>
            <div className="section-subtitle">专病门诊</div>
          </div>
          <Link to="/doctors" className="view-all">
            查看更多 &gt;
          </Link>
        </div>
        <div className="doctor-cards">
          {recommendDoctors.slice(0, 3).map((doctor) => (
            <Link to={`/doctor/${doctor.id}`} className="doctor-card" key={doctor.id}>
              <div className="doctor-avatar">
                <img 
                  src={buildImageUrl(doctor.avatarUrl, '/Img/02-发现/1.png')} 
                  alt={doctor.name}
                  onError={(e) => {
                    // 如果图片加载失败，使用默认图片
                    e.target.src = '/Img/02-发现/1.png';
                  }}
                />
              </div>
              <div className="doctor-info">
                <div className="doctor-name">{doctor.name}</div>
                <div className="doctor-tag">{doctor.title}</div>
                {doctor.department && (
                  <div className="doctor-department">{doctor.department}</div>
                )}
                {doctor.specialty && (
                  <div className="doctor-specialty">{doctor.specialty}</div>
                )}
              </div>
            </Link>
          ))}
          {recommendDoctors.length === 0 && <div className="empty">{t('no-recommended-doctors')}</div>}
        </div>
      </div>

      {/* 可能感兴趣 */}
      <div className="section">
        <div className="section-header">
          <div className="section-title-group">
            <h2 className="section-title">可能感兴趣</h2>
          </div>
        </div>
        <div className="article-list">
          {articles.length > 0 ? (
            articles.map((article) => (
              <div className="article-post" key={article.id}>
                <Link 
                  to={`/health-knowledge/${article.id}`}
                  className="article-link"
                >
                  <div className="post-header">
                    <div className="doctor-avatar">
                      <img
                        src={buildImageUrl(article.authorAvatar, '/Img/Director.png')}
                        alt={article.authorName ?? '医生'}
                        className="avatar-img"
                        onError={(e) => {
                          e.target.src = '/Img/Director.png';
                        }}
                      />
                    </div>
                    <div className="doctor-info">
                      <div className="doctor-name-title">
                        <span className="doctor-name">{article.authorName ?? '医生'}</span>
                        {article.authorTitle && (
                          <span className="doctor-title">{article.authorTitle}</span>
                        )}
                      </div>
                      <div className="doctor-hospital">
                        {article.authorDepartment && `${article.authorDepartment} · `}
                        {article.authorHospital ?? '医院'}
                      </div>
                      <div className="post-meta">
                        {formatTime(article.createdAt)} {t('from-doctor')}{article.authorName ?? t('doctor')}{t('doctor-science')}
                      </div>
                    </div>
                  </div>
                  <div className="post-content">
                    <div className="post-main">
                      <h3 className="post-title">{article.title}</h3>
                      <p className="post-excerpt">
                        {article.summary ?? article.content?.slice(0, 100) ?? '暂无摘要'}
                      </p>
                    </div>
                    <div className="post-thumbnail">
                      <img 
                        src={buildImageUrl(article.coverImageUrl, '/Img/Director.png')} 
                        alt={article.title} 
                        className="thumbnail-img"
                        onError={(e) => {
                          // 如果图片加载失败，使用默认图片
                          e.target.src = '/Img/Director.png';
                        }}
                      />
                    </div>
                  </div>
                </Link>
                <div className="post-actions">
                  <span className="action-item">
                    <span className="action-icon">👁️</span>
                    <span className="action-text">{t('read')} {article.readCount ?? 0}</span>
                  </span>
                  <span className="action-item">
                    <span className="action-icon">⭐</span>
                    <span className="action-text">{t('favorite')} {article.favoriteCount ?? 0}</span>
                  </span>
                </div>
              </div>
            ))
          ) : (
            <div className="empty">暂无推荐文章</div>
          )}
        </div>
      </div>

    </PageLayout>
  );
}


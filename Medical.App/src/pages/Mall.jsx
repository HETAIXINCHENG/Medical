import { useEffect, useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import PageLayout from '../components/PageLayout.jsx';
import usePageStyles from '../hooks/usePageStyles.js';
import { medicalApi } from '../services/medicalApi.js';
import { buildImageUrl } from '../utils/imageUtils.js';
import { useLanguage } from '../contexts/LanguageContext.jsx';

export default function Mall() {
  usePageStyles('mall.css');
  const { t } = useLanguage();
  const navigate = useNavigate();

  const [searchKeyword, setSearchKeyword] = useState('');
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [activeTab, setActiveTab] = useState('hot'); // 'hot' 或 'new'
  const [trendingProducts, setTrendingProducts] = useState([]); // 趋势商品（健脾祛湿）
  const [giftProducts, setGiftProducts] = useState([]); // 礼品商品（送礼送到心坎里）
  const pageSize = 10;
  const loaderRef = useRef(null);

  // 分类配置
  const categories = [
    { id: 'women', name: '女性健康', icon: '/Img/mall/women-health.png' },
    { id: 'men', name: '男性保健', icon: '/Img/mall/men-health.png' },
    { id: 'classic', name: '经典滋补', icon: '/Img/mall/classic-tonic.png' },
    { id: 'moisturizing', name: '清润滋养', icon: '/Img/mall/moisturizing.png' },
    { id: 'spleen', name: '脾胃养护', icon: '/Img/mall/spleen-care.png' }
  ];

  // 加载商品列表
  const loadProducts = async (pageNum = 1, append = false) => {
    if (!append) {
      setLoading(true);
      setProducts([]);
    } else {
      setLoadingMore(true);
    }

    try {
      const params = {
        page: pageNum,
        pageSize,
        ...(searchKeyword && { keyword: searchKeyword }),
        ...(selectedCategory && { categoryId: selectedCategory })
      };

      const data = await medicalApi.getProducts(params);
      const items = data?.items ?? data ?? [];
      const total = data?.total ?? items.length;

      if (append) {
        setProducts(prev => [...prev, ...items]);
      } else {
        setProducts(items);
      }

      setHasMore(items.length === pageSize && (pageNum * pageSize < total));
      setPage(pageNum);
    } catch (err) {
      console.error('加载商品失败:', err);
    } finally {
      setLoading(false);
      setLoadingMore(false);
    }
  };

  // 加载趋势商品（健脾祛湿）
  const loadTrendingProducts = async () => {
    try {
      // 这里可以根据实际需求筛选特定分类的商品
      const data = await medicalApi.getProducts({ page: 1, pageSize: 3 });
      const items = data?.items ?? data ?? [];
      setTrendingProducts(items.slice(0, 3));
    } catch (err) {
      console.error('加载趋势商品失败:', err);
    }
  };

  // 加载礼品商品（送礼送到心坎里）
  const loadGiftProducts = async () => {
    try {
      // 这里可以根据实际需求筛选特定分类的商品
      const data = await medicalApi.getProducts({ page: 1, pageSize: 3 });
      const items = data?.items ?? data ?? [];
      setGiftProducts(items.slice(0, 3));
    } catch (err) {
      console.error('加载礼品商品失败:', err);
    }
  };

  // 初始加载
  useEffect(() => {
    loadProducts(1, false);
    loadTrendingProducts();
    loadGiftProducts();
  }, [searchKeyword, selectedCategory]);

  // 下拉加载更多
  useEffect(() => {
    if (!hasMore || loadingMore || loading) return;

    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting) {
          loadProducts(page + 1, true);
        }
      },
      { threshold: 0.1 }
    );

    if (loaderRef.current) {
      observer.observe(loaderRef.current);
    }

    return () => observer.disconnect();
  }, [hasMore, loadingMore, loading, page]);

  // 处理搜索
  const handleSearch = () => {
    setPage(1);
    loadProducts(1, false);
  };

  // 处理分类选择
  const handleCategorySelect = (categoryId) => {
    setSelectedCategory(categoryId === selectedCategory ? null : categoryId);
    setPage(1);
  };

  // 处理商品图片路径
  const getProductImageUrl = (product) => {
    let coverUrl = product.coverUrl || product.CoverUrl || '';
    
    // 如果 coverUrl 是 JSON 数组字符串，需要解析
    if (typeof coverUrl === 'string' && coverUrl.trim().startsWith('[') && coverUrl.trim().endsWith(']')) {
      try {
        const imageUrls = JSON.parse(coverUrl);
        if (Array.isArray(imageUrls) && imageUrls.length > 0) {
          coverUrl = imageUrls[0];
        } else {
          coverUrl = '';
        }
      } catch (e) {
        console.error('解析商品图片URL失败:', e);
        coverUrl = '';
      }
    }
    
    return buildImageUrl(coverUrl, '/Img/default-product.png');
  };

  return (
    <PageLayout>
      <div className="mall-page">
        {/* 搜索栏 */}
        <div className="mall-search-bar">
          <div className="search-bar">
            <img src="/Img/search.png" alt="搜索" className="search-icon" />
            <input
              type="text"
              placeholder="儿童湿疹、过敏护理"
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
            />
            <div className="search-divider"></div>
            <button className="search-btn" onClick={handleSearch}>
              搜索
            </button>
          </div>
        </div>

        {/* 精选区域 */}
        <div className="mall-featured">
          <div className="featured-header">
            <h2 className="featured-title">精选专区</h2>
            <div className="featured-tabs">
              <button
                className={`tab-btn ${activeTab === 'hot' ? 'active' : ''}`}
                onClick={() => setActiveTab('hot')}
              >
                热销榜单
              </button>
            </div>
            <div className="featured-subtitle">养生好物,限时特惠</div>
          </div>

          {/* 产品列表 */}
          <div className="mall-products-horizontal">
            {loading && products.length === 0 ? (
              <div className="loading">加载中...</div>
            ) : products.length > 0 ? (
              <>
                {products.map((product, index) => {
                  const productName = product.name || product.Name || '';
                  const productDescription = product.description || product.Description || '';
                  const price = product.price || product.Price || 0;
                  const marketPrice = product.marketPrice || product.MarketPrice;
                  const productImageUrl = getProductImageUrl(product);

                  // 从描述中提取功能特性（用于banner显示）
                  let bannerText = '';
                  if (productDescription) {
                    const parts = productDescription.split('|');
                    if (parts.length > 1) {
                      bannerText = parts[1].trim();
                    } else {
                      bannerText = productDescription.substring(0, 10);
                    }
                  }

                  return (
                    <div
                      key={product.id || product.Id || index}
                      className="mall-product-card-horizontal"
                      role="button"
                      onClick={(e) => {
                        e.preventDefault();
                        e.stopPropagation();
                      }}
                    >
                      <div className="product-image-wrapper">
                        <img
                          src={productImageUrl}
                          alt={productName}
                          className="product-image"
                          onError={(e) => {
                            e.target.src = '/Img/default-product.png';
                          }}
                        />
                        {bannerText && (
                          <div className="product-banner">{bannerText}</div>
                        )}
                      </div>
                      <div className="product-info">
                        <div className="product-name">{productName}</div>
                        <div className="product-sales">
                          <span className="hot-sale">热卖</span>
                        </div>
                        <div className="product-price-row">
                          <span className="product-price">¥{price.toFixed(2)}</span>
                          {marketPrice && marketPrice > price && (
                            <span className="market-price">¥{marketPrice.toFixed(2)}</span>
                          )}
                        </div>
                      </div>
                    </div>
                  );
                })}

              </>
            ) : (
              <div className="empty">暂无商品</div>
            )}
          </div>
        </div>

        {/* 健脾祛湿区块 */}
        <div className="mall-section">
          <div className="section-header">
            <div className="section-title-group">
              <h3 className="section-title">健脾祛湿</h3>
              <span className="section-badge">新趋势</span>
            </div>
            <div className="section-subtitle">无湿一身轻,体态更轻盈</div>
          </div>
          <div className="mall-products-horizontal">
            {trendingProducts.map((product, index) => {
              const productName = product.name || product.Name || '';
              const price = product.price || product.Price || 0;
              const marketPrice = product.marketPrice || product.MarketPrice;
              const productImageUrl = getProductImageUrl(product);

              return (
                <div
                  key={product.id || product.Id || index}
                  className="mall-product-card-horizontal"
                  role="button"
                  onClick={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                  }}
                >
                  <div className="product-image-wrapper">
                    <img
                      src={productImageUrl}
                      alt={productName}
                      className="product-image"
                      onError={(e) => {
                        e.target.src = '/Img/default-product.png';
                      }}
                    />
                  </div>
                  <div className="product-info">
                    <div className="product-name">{productName}</div>
                    <div className="product-tags">
                      <span className="product-tag">经典膏方</span>
                    </div>
                    <div className="product-price-row">
                      <span className="product-price">¥{price.toFixed(2)}</span>
                      {marketPrice && marketPrice > price && (
                        <span className="market-price">¥{marketPrice.toFixed(2)}</span>
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* 送礼送到心坎里区块 */}
        <div className="mall-section">
          <div className="section-header">
            <div className="section-title-group">
              <h3 className="section-title">送礼送到心坎里</h3>
              <span className="section-badge">爱家人</span>
            </div>
            <div className="section-subtitle">顺时养生,滋补甄选</div>
          </div>
          <div className="mall-products-horizontal">
            {giftProducts.map((product, index) => {
              const productName = product.name || product.Name || '';
              const price = product.price || product.Price || 0;
              const marketPrice = product.marketPrice || product.MarketPrice;
              const productImageUrl = getProductImageUrl(product);

              return (
                <div
                  key={product.id || product.Id || index}
                  className="mall-product-card-horizontal"
                  role="button"
                  onClick={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                  }}
                >
                  <div className="product-image-wrapper">
                    <img
                      src={productImageUrl}
                      alt={productName}
                      className="product-image"
                      onError={(e) => {
                        e.target.src = '/Img/default-product.png';
                      }}
                    />
                  </div>
                  <div className="product-info">
                    <div className="product-name">{productName}</div>
                    <div className="product-tags">
                      <span className="product-tag">健康滋补</span>
                      <span className="product-tag">开盖即食</span>
                    </div>
                    <div className="product-price-row">
                      <span className="product-price">¥{price.toFixed(2)}</span>
                      {marketPrice && marketPrice > price && (
                        <span className="market-price">¥{marketPrice.toFixed(2)}</span>
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </PageLayout>
  );
}


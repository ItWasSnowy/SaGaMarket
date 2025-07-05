import React, { useState, useEffect, useRef } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import './Catalog.css';

function Catalog() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 12,
    totalCount: 0
  });
  const [inputPage, setInputPage] = useState('');
  const abortControllerRef = useRef(null);
  const navigate = useNavigate();

  const fetchProducts = async (page = 1) => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    const controller = new AbortController();
    abortControllerRef.current = controller;

    try {
      setLoading(true);
      setError(null);
      
      const response = await axios.get('https://localhost:7182/api/Product', {
        params: { 
          page, 
          pageSize: pagination.pageSize 
        },
        signal: controller.signal
      });

      const totalCount = parseInt(response.headers['x-total-count'], 10) || 0;
      const productsWithDefaults = response.data.map(p => ({
        ...p,
        productName: p.productName || 'Без названия',
        minPrice: p.minPrice || 0,
        maxPrice: p.maxPrice || p.minPrice || 0,
        imageUrl: p.imageUrl || null,
        averageRating: p.averageRating || 0,
        reviewsCount: p.reviewsCount || 0
      }));

      setProducts(productsWithDefaults.filter(p => p.productId));
      setPagination(prev => ({
        ...prev,
        page,
        totalCount
      }));
      setInputPage('');

    } catch (error) {
      if (axios.isCancel(error)) return;
      
      setError(error.response?.data?.message || 
              'Ошибка при загрузке каталога');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts(pagination.page);
    return () => abortControllerRef.current?.abort();
  }, []);

  const handlePrevPage = () => {
    if (pagination.page > 1) {
      fetchProducts(pagination.page - 1);
    }
  };

  const handleNextPage = () => {
    const totalPages = Math.ceil(pagination.totalCount / pagination.pageSize);
    if (pagination.page < totalPages) {
      fetchProducts(pagination.page + 1);
    }
  };

  const handlePageJump = () => {
    const pageNum = parseInt(inputPage);
    const totalPages = Math.ceil(pagination.totalCount / pagination.pageSize);
    
    if (!isNaN(pageNum)) { 
      if (pageNum >= 1 && pageNum <= totalPages) {
        fetchProducts(pageNum);
      } else {
        setError(`Введите номер страницы от 1 до ${totalPages}`);
      }
    }
  };

  const handleProductClick = (productId) => {
    if (!productId) {
      console.error('Attempted to navigate with undefined productId');
      return;
    }
    navigate(`/product/${productId}`);
  };

  const renderRatingStars = (rating) => {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

    return (
      <>
        {[...Array(fullStars)].map((_, i) => (
          <span key={`full-${i}`}>★</span>
        ))}
        {hasHalfStar && <span>½</span>}
        {[...Array(emptyStars)].map((_, i) => (
          <span key={`empty-${i}`}>☆</span>
        ))}
      </>
    );
  };

  const totalPages = Math.ceil(pagination.totalCount / pagination.pageSize);

  if (error) {
    return (
      <div className="error-container">
        <h3>Ошибка</h3>
        <p>{error}</p>
        <button 
          onClick={() => fetchProducts(pagination.page)}
          className="retry-button"
        >
          Повторить попытку
        </button>
      </div>
    );
  }

  if (loading && products.length === 0) {
    return <div className="loading">Загрузка товаров...</div>;
  }

  return (
    <div className="catalog">
      <h1>Каталог товаров</h1>
      
      <div className="products-grid">
        {products.map(product => (
          <div 
            key={product.productId}
            className="product-card"
            onClick={() => handleProductClick(product.productId)}
          >
            <div className="product-image-container">
              {product.imageUrl ? (
                <img 
                  src={product.imageUrl} 
                  alt={product.productName}
                  onError={(e) => {
                    e.target.src = '/placeholder-product.png';
                    e.target.onerror = null;
                  }}
                />
              ) : (
                <div className="image-placeholder">
                  <span>Нет изображения</span>
                </div>
              )}
            </div>
            
            <div className="product-info">
              <h3 className="product-title">{product.productName}</h3>
              <p className="product-price">
                {product.minPrice === product.maxPrice
                  ? `${product.minPrice.toLocaleString()} ₽`
                  : `от ${product.minPrice.toLocaleString()} ₽`}
              </p>
              
              <div className="product-rating">
                <div className="stars">
                  {renderRatingStars(product.averageRating)}
                </div>
                <span className="rating-value">
                  {product.averageRating.toFixed(1)}
                </span>
                {product.reviewsCount > 0 && (
                  <span className="reviews-count">
                    ({product.reviewsCount})
                  </span>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>

      {pagination.totalCount > 0 && (
        <div className="pagination-container">
          <div className="pagination">
            <button 
              onClick={handlePrevPage}
              disabled={pagination.page === 1 || loading}
              className="pagination-button"
            >
              Назад
            </button>
            
            <span className="page-info">
              Страница {pagination.page} из {totalPages}
            </span>
            
            <button 
              onClick={handleNextPage}
              disabled={pagination.page >= totalPages || loading}
              className="pagination-button"
            >
              Вперед
            </button>
          </div>
          
          <div className="page-jump">
            <input
              type="number"
              min="1"
              max={totalPages}
              value={inputPage}
              onChange={(e) => setInputPage(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handlePageJump()}
              placeholder="№ страницы"
              className="page-input"
            />
            <button 
              onClick={handlePageJump}
              disabled={loading}
              className="jump-button"
            >
              Перейти
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default Catalog;
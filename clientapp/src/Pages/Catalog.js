import React, { useState, useEffect, useRef } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import './Catalog.css';

function Catalog() {
  const [state, setState] = useState({
    products: [],
    loading: true,
    error: null,
    pagination: {
      page: 1,
      pageSize: 10,
      totalCount: 0
    }
  });
  const [inputPage, setInputPage] = useState('');
  const abortControllerRef = useRef(null);
  const navigate = useNavigate();

  const fetchProductsWithRatings = async (page = 1) => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    abortControllerRef.current = new AbortController();

    try {
      setState(prev => ({ ...prev, loading: true, error: null }));
      
      // 1. Запрос списка продуктов с пагинацией
      const productsResponse = await axios.get('https://localhost:7182/api/Product', {
        params: { 
          page, 
          pageSize: state.pagination.pageSize 
        },
        signal: abortControllerRef.current.signal
      });

      const totalCount = parseInt(productsResponse.headers['x-total-count'], 10) || 0;
      
      // 2. Подготовка ID для запроса рейтингов
      const productIds = productsResponse.data
        .map(p => p.productId)
        .filter(id => id) // Фильтрация null/undefined
        .join(',');

      let productsWithRatings = productsResponse.data.map(p => ({
        ...p,
        averageRating: 0,
        reviewsCount: 0
      }));

      // 3. Запрос рейтингов только если есть ID товаров
      if (productIds) {
        try {
          const ratingsResponse = await axios.get('https://localhost:7182/api/Product/products', {
            params: { productIds },
            signal: abortControllerRef.current.signal
          });

          // Объединение данных
          productsWithRatings = productsResponse.data.map(product => {
            const ratingInfo = ratingsResponse.data.find(r => r.productId === product.productId);
            return {
              ...product,
              averageRating: ratingInfo?.averageRating || 0,
              reviewsCount: ratingInfo?.reviewsCount || 0
            };
          });
        } catch (ratingsError) {
          console.warn('Ошибка загрузки рейтингов:', ratingsError);
          // Продолжаем без рейтингов, если их не удалось загрузить
        }
      }

      setState({
        products: productsWithRatings,
        loading: false,
        error: null,
        pagination: {
          ...state.pagination,
          page,
          totalCount
        }
      });
      setInputPage('');

    } catch (error) {
      if (axios.isCancel(error)) return;
      
      console.error('Ошибка загрузки:', error);
      setState(prev => ({
        ...prev,
        loading: false,
        error: error.response?.data?.message || 
              error.response?.data?.Message || 
              error.message || 
              'Ошибка при загрузке данных'
      }));
    }
  };

  useEffect(() => {
    fetchProductsWithRatings(state.pagination.page);
    
    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, []);

  const handleNextPage = () => {
    const totalPages = Math.ceil(state.pagination.totalCount / state.pagination.pageSize);
    if (state.pagination.page < totalPages) {
      fetchProductsWithRatings(state.pagination.page + 1);
    }
  };

  const handlePrevPage = () => {
    if (state.pagination.page > 1) {
      fetchProductsWithRatings(state.pagination.page - 1);
    }
  };

  const handlePageJump = () => {
    const pageNum = parseInt(inputPage);
    const totalPages = Math.ceil(state.pagination.totalCount / state.pagination.pageSize);
    
    if (!isNaN(pageNum)) { 
      if (pageNum >= 1 && pageNum <= totalPages) {
        fetchProductsWithRatings(pageNum);
      } else {
        setState(prev => ({
          ...prev,
          error: `Введите номер страницы от 1 до ${totalPages}`
        }));
      }
    }
  };

  const renderRating = (rating, reviewsCount) => {
    const normalizedRating = rating || 0;
    const fullStars = Math.floor(normalizedRating);
    const hasHalfStar = normalizedRating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

    return (
      <div className="product-rating">
        <div className="stars-container">
          {[...Array(fullStars)].map((_, i) => (
            <span key={`full-${i}`} className="star filled">★</span>
          ))}
          {hasHalfStar && (
            <span className="star half">★</span>
          )}
          {[...Array(emptyStars)].map((_, i) => (
            <span key={`empty-${i}`} className="star">★</span>
          ))}
        </div>
        <span className="rating-value">
          {normalizedRating.toFixed(1)}
        </span>
        {reviewsCount > 0 && (
          <span className="reviews-count">
            ({reviewsCount} {reviewsCount === 1 ? 'отзыв' : reviewsCount < 5 ? 'отзыва' : 'отзывов'})
          </span>
        )}
      </div>
    );
  };

  if (state.error) {
    return (
      <div className="error-container">
        <h3>Ошибка</h3>
        <p>{state.error}</p>
        <button 
          onClick={() => fetchProductsWithRatings(state.pagination.page)}
          className="retry-button"
        >
          Повторить попытку
        </button>
      </div>
    );
  }

  if (state.loading && state.products.length === 0) {
    return <div className="loading">Загрузка товаров...</div>;
  }

  const totalPages = Math.ceil(state.pagination.totalCount / state.pagination.pageSize);

  return (
    <div className="catalog">
      <h1>Каталог товаров</h1>
      
      <div className="products-grid">
        {state.products.map(product => (
          <div 
            key={product.productId} 
            className="product-card"
            onClick={() => navigate(`/product/${product.productId}`)}
          >
            {product.imageUrl ? (
              <img 
                src={product.imageUrl} 
                alt={product.productName} 
                className="product-image"
                onError={(e) => {
                  e.target.src = 'https://via.placeholder.com/300?text=No+Image';
                  e.target.onerror = null;
                }}
              />
            ) : (
              <div className="product-image-placeholder">
                <span>Нет изображения</span>
              </div>
            )}
            <h3>{product.productName}</h3>
            <p className="price">
              {product.minPrice === product.maxPrice 
                ? `${product.minPrice.toLocaleString()} ₽` 
                : `от ${product.minPrice.toLocaleString()} до ${product.maxPrice.toLocaleString()} ₽`}
            </p>
            {renderRating(product.averageRating, product.reviewsCount)}
          </div>
        ))}
      </div>

      {state.pagination.totalCount > 0 && (
        <div className="pagination-container">
          <div className="pagination">
            <button 
              onClick={handlePrevPage}
              disabled={state.pagination.page === 1 || state.loading}
              className="pagination-button"
            >
              Назад
            </button>
            
            <span className="page-info">
              Страница {state.pagination.page} из {totalPages}
            </span>
            
            <button 
              onClick={handleNextPage}
              disabled={state.pagination.page >= totalPages || state.loading}
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
              disabled={state.loading}
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
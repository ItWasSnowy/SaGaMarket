import React, { useState, useEffect, useRef } from 'react';
import axios from 'axios';
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

  const abortControllerRef = useRef(null);

  // Функция для отображения рейтинга звездами
  const renderRating = (rating) => {
    const normalizedRating = rating || 0;
    const fullStars = Math.floor(normalizedRating);
    const hasHalfStar = normalizedRating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

    return (
      <div className="stars">
        {[...Array(fullStars)].map((_, i) => (
          <span key={`full-${i}`} className="star full">★</span>
        ))}
        {hasHalfStar && <span className="star half">★</span>}
        {[...Array(emptyStars)].map((_, i) => (
          <span key={`empty-${i}`} className="star empty">★</span>
        ))}
        <span className="rating-value">
          {normalizedRating.toFixed(1)}
        </span>
      </div>
    );
  };

  const fetchProducts = async (page = 1) => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    abortControllerRef.current = new AbortController();

    try {
      setState(prev => ({ ...prev, loading: true, error: null }));
      
      const response = await axios.get('https://localhost:7182/api/Product', {
        params: { 
          page, 
          pageSize: state.pagination.pageSize 
        },
        signal: abortControllerRef.current.signal
      });

      const totalCount = parseInt(response.headers['x-total-count']) || 0;

      setState({
        products: response.data,
        loading: false,
        error: null,
        pagination: {
          ...state.pagination,
          page,
          totalCount
        }
      });

    } catch (error) {
      if (axios.isCancel(error)) return;
      
      setState(prev => ({
        ...prev,
        loading: false,
        error: error.response?.data?.message || error.message || 'Ошибка при загрузке данных'
      }));
    }
  };

  useEffect(() => {
    fetchProducts();
    return () => abortControllerRef.current?.abort();
  }, []);

  const handleNextPage = () => {
    const totalPages = Math.ceil(state.pagination.totalCount / state.pagination.pageSize);
    if (state.pagination.page < totalPages) {
      fetchProducts(state.pagination.page + 1);
    }
  };

  const handlePrevPage = () => {
    if (state.pagination.page > 1) {
      fetchProducts(state.pagination.page - 1);
    }
  };

  if (state.error) {
    return (
      <div className="error-container">
        <h3>Ошибка</h3>
        <p>{state.error}</p>
        <button 
          onClick={() => fetchProducts(state.pagination.page)}
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

  return (
    <div className="catalog">
      <h1>Каталог товаров</h1>
      
      <div className="products-grid">
        {state.products.map(product => (
          <div key={product.productId} className="product-card">
            <h3>{product.productName}</h3>
            <p className="price">
              {product.minPrice === product.maxPrice 
                ? `${product.minPrice} ₽` 
                : `от ${product.minPrice} до ${product.maxPrice} ₽`}
            </p>
            <div className="product-rating">
              {renderRating(product.averageRating)}
              <span className="reviews-count">
                {product.reviewIds?.length || 0} отзывов
              </span>
            </div>
          </div>
        ))}
      </div>

      {state.pagination.totalCount > 0 && (
        <div className="pagination">
          <button 
            onClick={handlePrevPage}
            disabled={state.pagination.page === 1 || state.loading}
          >
            Назад
          </button>
          
          <span>
            Страница {state.pagination.page} из{' '}
            {Math.ceil(state.pagination.totalCount / state.pagination.pageSize)}
          </span>
          
          <button 
            onClick={handleNextPage}
            disabled={
              state.pagination.page >= 
              Math.ceil(state.pagination.totalCount / state.pagination.pageSize) || 
              state.loading
            }
          >
            Вперед
          </button>
        </div>
      )}
    </div>
  );
}

export default Catalog;
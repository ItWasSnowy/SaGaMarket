import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './Catalog.css';

// Отключение проверки SSL только для разработки (добавьте в начале файла)
if (process.env.NODE_ENV === 'development') {
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
}

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

  const fetchProducts = async (page = 1, pageSize = 10) => {
    try {
      setState(prev => ({ ...prev, loading: true, error: null }));
      
      const response = await axios.get('https://localhost:7182/api/Product', {
        params: { page, pageSize }
      });
      
      setState({
        products: response.data,
        loading: false,
        error: null,
        pagination: {
          page,
          pageSize,
          totalCount: parseInt(response.headers['x-total-count']) || 0
        }
      });
      
    } catch (error) {
      let errorMessage = 'Ошибка при загрузке данных';
      
      if (error.response) {
        // Сервер ответил с кодом ошибки
        errorMessage = `Ошибка сервера: ${error.response.status}`;
      } else if (error.request) {
        // Запрос был сделан, но ответ не получен
        errorMessage = 'Сервер не отвечает. Проверьте:';
      } else {
        // Другие ошибки
        errorMessage = error.message;
      }
      
      setState(prev => ({
        ...prev,
        loading: false,
        error: errorMessage
      }));
    }
  };

  useEffect(() => {
    fetchProducts();
  }, []);

  const handleNextPage = () => {
    const nextPage = state.pagination.page + 1;
    if (nextPage <= Math.ceil(state.pagination.totalCount / state.pagination.pageSize)) {
      fetchProducts(nextPage);
    }
  };

  const handlePrevPage = () => {
    const prevPage = state.pagination.page - 1;
    if (prevPage >= 1) {
      fetchProducts(prevPage);
    }
  };

  if (state.error) {
    return (
      <div className="error-container">
        <h3>{state.error}</h3>
        {state.error.includes('Проверьте') && (
          <ul>
            <li>• Сервер запущен на https://localhost:7182</li>
            <li>• CORS настроен на сервере</li>
            <li>• Сертификат разработки добавлен в доверенные</li>
          </ul>
        )}
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
            <p className="category">{product.productCategory}</p>
            <p className="rating">
              Рейтинг: {isNaN(product.averageRating) 
                ? 'нет оценок' 
                : product.averageRating.toFixed(1)}
            </p>
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
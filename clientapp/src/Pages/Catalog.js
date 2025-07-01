import React, { useState, useEffect, useRef } from 'react';
import axios from 'axios';
import './Catalog.css';

// Отключение проверки SSL только для разработки
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

  // Используем ref для хранения AbortController
  const abortControllerRef = useRef(null);

  const fetchProducts = async (page = 1, pageSize = 10) => {
    // Отменяем предыдущий запрос, если он существует
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    // Создаем новый AbortController
    abortControllerRef.current = new AbortController();

    try {
      setState(prev => ({ ...prev, loading: true, error: null }));
      
      const response = await axios.get('https://localhost:7182/api/Product', {
        params: { page, pageSize },
        signal: abortControllerRef.current.signal
      });

      // Получаем общее количество товаров
      const totalCount = response.headers['x-total-count'] 
        ? parseInt(response.headers['x-total-count'])
        : (response.data.length >= pageSize 
            ? (page * pageSize) + 1 
            : ((page - 1) * pageSize) + response.data.length);

      setState({
        products: response.data,
        loading: false,
        error: null,
        pagination: {
          page,
          pageSize,
          totalCount: Math.max(totalCount, response.data.length)
        }
      });

    } catch (error) {
      if (axios.isCancel(error)) {
        console.log('Запрос был отменен');
        return;
      }
      
      setState(prev => ({
        ...prev,
        loading: false,
        error: error.response?.data?.message || 
              error.message || 
              'Ошибка при загрузке данных'
      }));
    }
  };

  useEffect(() => {
    fetchProducts();

    // Функция очистки - отменяем запрос при размонтировании
    return () => {
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, []);

  const handleNextPage = () => {
    if (state.pagination.page < Math.ceil(state.pagination.totalCount / state.pagination.pageSize)) {
      fetchProducts(state.pagination.page + 1);
    }
  };

  const handlePrevPage = () => {
    if (state.pagination.page > 1) {
      fetchProducts(state.pagination.page - 1);
    }
  };

  // Рассчитываем общее количество страниц
  const totalPages = Math.ceil(state.pagination.totalCount / state.pagination.pageSize);

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
      
      {state.products.length === 0 ? (
        <div className="empty">Товары не найдены</div>
      ) : (
        <>
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

          <div className="pagination">
            <button 
              onClick={handlePrevPage}
              disabled={state.pagination.page === 1 || state.loading}
            >
              Назад
            </button>
            
            <span>
              Страница {state.pagination.page} из {totalPages}
            </span>
            
            <button 
              onClick={handleNextPage}
              disabled={state.pagination.page >= totalPages || state.loading}
            >
              Вперед
            </button>
          </div>
        </>
      )}
    </div>
  );
}

export default Catalog;
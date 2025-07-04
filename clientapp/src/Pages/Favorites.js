import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Favorites.css';

function Favorites({ setFavoritesCount }) {
  const [userId, setUserId] = useState('');
  const [favoriteProducts, setFavoriteProducts] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const savedUserId = localStorage.getItem('userId');
    if (savedUserId) {
      setUserId(savedUserId);
    }
  }, []);

  useEffect(() => {
    if (userId) {
      fetchUserFavorites(userId);
    }
  }, [userId]);

  const fetchVariantsForProduct = async (productId) => {
    try {
      const response = await fetch(
        `https://localhost:7182/api/products/${productId}/variants`
      );
      if (!response.ok) throw new Error(`Не удалось загрузить варианты для товара ${productId}`);
      return await response.json();
    } catch (err) {
      console.error(`Ошибка при загрузке вариантов для товара ${productId}:`, err);
      return [];
    }
  };

  const fetchUserFavorites = async (userId) => {
    try {
      setIsLoading(true);
      setError(null);
      
      // 1. Получаем список productId избранного
      const favoritesResponse = await fetch(
        `https://localhost:7182/api/favorites/items?userId=${userId}`
      );
      
      if (!favoritesResponse.ok) throw new Error('Не удалось загрузить избранное');
      
      const productIds = await favoritesResponse.json();
      
      if (!Array.isArray(productIds)) {
        throw new Error('Некорректный формат данных избранного');
      }

      setFavoritesCount(productIds.length);

      if (productIds.length === 0) {
        setFavoriteProducts([]);
        return;
      }

      // 2. Получаем основную информацию о товарах
      const productsResponse = await fetch(
        `https://localhost:7182/api/favorites/info?productIds=${productIds.join('&productIds=')}`
      );
      
      if (!productsResponse.ok) throw new Error('Не удалось загрузить информацию о товарах');
      
      const productsData = await productsResponse.json();

      // 3. Для каждого продукта получаем варианты и объединяем данные
      const productsWithVariants = await Promise.all(
        productsData.map(async product => {
          const variants = await fetchVariantsForProduct(product.productId);
          
          // Формируем строку с диапазоном цен
          let priceRange = 'Цена не указана';
          if (variants.length > 0) {
            const prices = variants.map(v => v.price);
            const minPrice = Math.min(...prices);
            const maxPrice = Math.max(...prices);
            
            priceRange = minPrice === maxPrice 
              ? `${minPrice} ₽` 
              : `${minPrice} - ${maxPrice} ₽`;
          }

          // Формируем список вариантов для отображения
          const variantNames = variants.map(v => v.name).join(', ');

          return {
            ...product,
            priceRange,
            variantNames,
            variants
          };
        })
      );

      setFavoriteProducts(productsWithVariants);
    } catch (err) {
      console.error('Ошибка при загрузке избранного:', err);
      setError(err.message);
      setFavoritesCount(0);
    } finally {
      setIsLoading(false);
    }
  };

  const removeFromFavorites = async (productId) => {
    try {
      const response = await fetch(
        `https://localhost:7182/api/favorites/remove?userId=${userId}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ productId })
        }
      );

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Не удалось удалить из избранного');
      }
      
      // Обновляем список избранного после удаления
      await fetchUserFavorites(userId);
      
    } catch (err) {
      console.error('Ошибка при удалении из избранного:', err);
      setError(err.message);
    }
  };

  return (
    <div className="favorites-page">
      <h1>Мои избранные товары</h1>
      
      <div className="user-id-section">
        <form onSubmit={(e) => {
          e.preventDefault();
          if (!userId) {
            setError('Пожалуйста, введите ID пользователя');
            return;
          }
          localStorage.setItem('userId', userId);
          fetchUserFavorites(userId);
        }}>
          <label>
            ID пользователя:
            <input
              type="text"
              value={userId}
              onChange={(e) => setUserId(e.target.value)}
              placeholder="Введите ваш ID"
            />
          </label>
          <button type="submit">Загрузить избранное</button>
        </form>
      </div>

      {error && <div className="error-message">{error}</div>}

      {isLoading ? (
        <div className="loading">Загрузка избранного...</div>
      ) : favoriteProducts.length > 0 ? (
        <div className="favorites-grid">
          {favoriteProducts.map((product) => (
            <div key={product.productId} className="favorite-item">
              <div 
                className="product-image"
                onClick={() => navigate(`/product/${product.productId}`)}
              >
                <img 
                  src={product.imageUrl || '/images/default-product.png'} 
                  alt={product.name} 
                />
              </div>
              
              <div className="product-info">
                <h3 onClick={() => navigate(`/product/${product.productId}`)}>
                  {product.name}
                </h3>
                
                <div className="product-details">
                  <p className="variants">Варианты: {product.variantNames || 'Нет вариантов'}</p>
                  <p className="price-range">Цена: {product.priceRange}</p>
                  <p className="category">Категория: {product.category}</p>
                  <p className="seller">Продавец: {product.sellerName}</p>
                </div>
                
                {/* <div className="rating">
                  {!isNaN(product.averageRating) ? (
                    <>
                      {'★'.repeat(Math.round(product.averageRating))}
                      {'☆'.repeat(5 - Math.round(product.averageRating))}
                      <span>({product.averageRating.toFixed(1)})</span>
                    </>
                  ) : (
                    <span>Нет оценок</span>
                  )}
                </div> */}
                
                <div className="product-actions">
                  <button 
                    className="remove-btn"
                    onClick={() => removeFromFavorites(product.productId)}
                  >
                    Удалить из избранного
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        !isLoading && <div className="empty-favorites">Ваш список избранного пуст</div>
      )}
    </div>
  );
}

export default Favorites;
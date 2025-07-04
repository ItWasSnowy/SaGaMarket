import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Favorites.css';

function Favorites({ setFavoritesCount }) {
  const [userId, setUserId] = useState('');
  const [favoriteProducts, setFavoriteProducts] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  // Загрузка userId из localStorage при монтировании
  useEffect(() => {
    const savedUserId = localStorage.getItem('userId');
    if (savedUserId) {
      setUserId(savedUserId);
    }
  }, []);

  // Загрузка избранного при изменении userId
  useEffect(() => {
    if (userId) {
      fetchUserFavorites(userId);
    }
  }, [userId]);

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
      console.log('Product IDs in favorites:', productIds);
      
      if (!Array.isArray(productIds)) {
        throw new Error('Некорректный формат данных избранного');
      }

      // Обновляем счетчик
      setFavoritesCount(productIds.length);

      if (productIds.length === 0) {
        setFavoriteProducts([]);
        return;
      }

      // 2. Получаем полную информацию о товарах
      const productsResponse = await fetch(
        `https://localhost:7182/api/favorites/info?productIds=${productIds.join('&productIds=')}`
      );
      
      if (!productsResponse.ok) throw new Error('Не удалось загрузить информацию о товарах');
      
      const productsData = await productsResponse.json();
      console.log('Favorites products data:', productsData);
      
      setFavoriteProducts(productsData || []);
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
        `https://localhost:7182/api/favorites/items?userId=${userId}`,
        {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ productId })
        }
      );

      if (!response.ok) throw new Error('Не удалось удалить из избранного');
      
      // Обновляем список после удаления
      await fetchUserFavorites(userId);
    } catch (err) {
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
                  alt={product.productName} 
                />
              </div>
              
              <div className="product-info">
                <h3 onClick={() => navigate(`/product/${product.productId}`)}>
                  {product.productName}
                </h3>
                
                <div className="rating">
                  {!isNaN(product.productRating) ? (
                    <>
                      {'★'.repeat(Math.round(product.productRating))}
                      {'☆'.repeat(5 - Math.round(product.productRating))}
                      <span>({product.productRating.toFixed(1)})</span>
                    </>
                  ) : (
                    <span>Нет оценок</span>
                  )}
                </div>
                
                <p className="price">Цена: ${product.price?.toFixed(2) || 'N/A'}</p>
                <p className="seller">Продавец: {product.sellerName}</p>
                
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
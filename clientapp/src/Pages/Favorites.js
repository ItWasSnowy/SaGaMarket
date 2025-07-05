import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Favorites.css';

function Favorites({ setFavoritesCount }) {
  const [favoriteProducts, setFavoriteProducts] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  const getCurrentUser = () => {
    const userData = localStorage.getItem('userData');
    if (!userData) {
      navigate('/login');
      return null;
    }
    return JSON.parse(userData);
  };

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

  // Функция для обработки ошибок загрузки изображений
  const handleImageError = (e) => {
    // Просто скрываем изображение при ошибке
    e.target.style.display = 'none';
  };

  const fetchUserFavorites = async () => {
    const user = getCurrentUser();
    if (!user?.userId) return;

    try {
      setIsLoading(true);
      setError(null);
      
      const favoritesResponse = await fetch(
        `https://localhost:7182/api/favorites/items?userId=${user.userId}`,
        {
          credentials: 'include'
        }
      );
      
      if (favoritesResponse.status === 401) {
        localStorage.removeItem('userData');
        navigate('/login');
        return;
      }

      if (!favoritesResponse.ok) throw new Error('Не удалось загрузить избранное');
      
      const productIds = await favoritesResponse.json();
      
      if (!Array.isArray(productIds)) {
        throw new Error('Некорректный формат данных избранного');
      }

      if (setFavoritesCount) {
        setFavoritesCount(productIds.length);
      }

      if (productIds.length === 0) {
        setFavoriteProducts([]);
        return;
      }

      const queryParams = productIds.map(id => `productIds=${encodeURIComponent(id)}`).join('&');
      const productsResponse = await fetch(
        `https://localhost:7182/api/favorites/info?${queryParams}`,
        {
          credentials: 'include'
        }
      );
      
      if (!productsResponse.ok) throw new Error('Не удалось загрузить информацию о товарах');
      
      const productsData = await productsResponse.json();

      const productsWithVariants = await Promise.all(
        productsData.map(async product => {
          const variants = await fetchVariantsForProduct(product.productId);
          
          let priceRange = 'Цена не указана';
          if (variants.length > 0) {
            const prices = variants.map(v => v.price);
            const minPrice = Math.min(...prices);
            const maxPrice = Math.max(...prices);
            
            priceRange = minPrice === maxPrice 
              ? `${minPrice} ₽` 
              : `${minPrice} - ${maxPrice} ₽`;
          }

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
      if (setFavoritesCount) {
        setFavoritesCount(0);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const removeFromFavorites = async (productId) => {
    const user = getCurrentUser();
    if (!user?.userId) return;

    try {
      const response = await fetch(
        `https://localhost:7182/api/favorites/remove?userId=${user.userId}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ productId }),
          credentials: 'include'
        }
      );

      if (response.status === 401) {
        localStorage.removeItem('userData');
        navigate('/login');
        return;
      }

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Не удалось удалить из избранного');
      }
      
      await fetchUserFavorites();
      
    } catch (err) {
      console.error('Ошибка при удалении из избранного:', err);
      setError(err.message);
    }
  };

  useEffect(() => {
    fetchUserFavorites();
  }, []);

  return (
    <div className="favorites-page">
      <h1>Мои избранные товары</h1>

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
                {product.imageUrl && (
                  <img 
                    src={product.imageUrl} 
                    alt={product.name}
                    onError={handleImageError}
                  />
                )}
              </div>
              
              <div className="product-info">
                <h3 onClick={() => navigate(`/product/${product.productId}`)}>
                  {product.name}
                </h3>
                
                <div className="product-details">
                  <p className="price-range">Цена: {product.priceRange}</p>
                  <p className="category">Категория: {product.category}</p>
                  <p className="seller">Продавец: {product.sellerName}</p>
                </div>
                
                <div className="product-actions">
                  <button 
                    className="remove-btn"
                    onClick={() => removeFromFavorites(product.productId)}
                    disabled={isLoading}
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
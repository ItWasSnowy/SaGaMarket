import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../authContext';
import { FaHeart, FaSpinner } from 'react-icons/fa';
import './Favorites.css';

function Favorites({ setFavoritesCount }) {
  const [favoriteProducts, setFavoriteProducts] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [favoritesLoading, setFavoritesLoading] = useState({});
  const navigate = useNavigate();
  const { user } = useAuth();

  const fetchVariantsForProduct = async (productId) => {
    try {
      const response = await fetch(
        `https://localhost:7182/api/Variant/products/${productId}/variants`,
        { credentials: 'include' }
      );
      if (!response.ok) throw new Error(`Не удалось загрузить варианты для товара ${productId}`);
      const variants = await response.json();
      return variants.length > 0 ? variants : [];
    } catch (err) {
      console.error(`Ошибка при загрузке вариантов для товара ${productId}:`, err);
      return [];
    }
  };

  const fetchUserFavorites = async () => {
    if (!user) return;

    try {
      setIsLoading(true);
      setError(null);
      
      const favoritesResponse = await fetch(
        `https://localhost:7182/api/favorites/items?userId=${user.userId}`,
        { credentials: 'include' }
      );
      
      if (favoritesResponse.status === 401) {
        navigate('/login');
        return;
      }

      if (!favoritesResponse.ok) throw new Error('Не удалось загрузить избранное');
      
      const productIds = await favoritesResponse.json();
      
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
        { credentials: 'include' }
      );
      
      if (!productsResponse.ok) throw new Error('Не удалось загрузить информацию о товарах');
      
      const productsData = await productsResponse.json();

      const productsWithVariants = await Promise.all(
        productsData.map(async product => {
          const variants = await fetchVariantsForProduct(product.productId);
          return {
            ...product,
            variants: variants,
            imageUrl: variants.length > 0 && variants[0].variantId 
              ? `https://localhost:7182/api/Media/image/${variants[0].variantId}.png`
              : null
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

  const removeFromFavorites = async (productId, e) => {
    e.stopPropagation();
    if (!user) return;

    try {
      setFavoritesLoading(prev => ({ ...prev, [productId]: true }));
      
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
        navigate('/login');
        return;
      }

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Не удалось удалить из избранного');
      }
      
      setFavoriteProducts(prev => prev.filter(p => p.productId !== productId));
      if (setFavoritesCount) {
        setFavoritesCount(prev => prev - 1);
      }
    } catch (err) {
      console.error('Ошибка при удалении из избранного:', err);
      setError(err.message);
    } finally {
      setFavoritesLoading(prev => ({ ...prev, [productId]: false }));
    }
  };

  useEffect(() => {
    fetchUserFavorites();
  }, [user]);

  const handleProductClick = (productId) => {
    navigate(`/product/${productId}`);
  };

  return (
    <div className="favorites-page">
      <h1>Мои избранные товары</h1>

      {error && <div className="error-message">{error}</div>}

      {isLoading ? (
        <div className="loading">Загрузка избранного...</div>
      ) : favoriteProducts.length > 0 ? (
        <div className="favorites-grid">
          {favoriteProducts.map((product) => (
            <div 
              key={product.productId} 
              className="favorite-item"
              onClick={() => handleProductClick(product.productId)}
            >
              <div className="product-image-container">
                {product.imageUrl ? (
                  <img 
                    src={product.imageUrl} 
                    alt={product.name}
                    className="product-image"
                    onError={(e) => {
                      e.target.src = '/placeholder-image.png'; // Запасное изображение
                    }}
                  />
                ) : (
                  <div className="image-placeholder">Нет изображения</div>
                )}
                <button
                  className="remove-favorite-btn"
                  onClick={(e) => removeFromFavorites(product.productId, e)}
                  disabled={favoritesLoading[product.productId]}
                >
                  {favoritesLoading[product.productId] ? (
                    <FaSpinner className="spinner-icon" />
                  ) : (
                    <FaHeart color="#ff4d4d" />
                  )}
                </button>
              </div>
              
              <div className="product-info">
                <h3>{product.name}</h3>
                
                <div className="product-details">
                  <p className="category">Категория: {product.category}</p>
                  {product.variant?.price && (
                    <p className="price">{product.variant.price.toLocaleString()} ₽</p>
                  )}
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

import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import axios from 'axios';
import './MyProductsPage.css';

axios.defaults.withCredentials = true;

const MyProductsPage = () => {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { sellerId } = useParams();

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        const response = await axios.get(
          `https://localhost:7182/api/Product/api/Products?sellerId=${sellerId}`
        );
        
        if (response.data && Array.isArray(response.data)) {
          setProducts(response.data);
        } else {
          throw new Error('Неверный формат данных');
        }
      } catch (err) {
        console.error('Ошибка загрузки товаров:', err);
        setError(err.response?.data?.message || 'Не удалось загрузить товары');
      } finally {
        setLoading(false);
      }
    };

    if (sellerId) {
      fetchProducts();
    } else {
      setError('ID продавца не указан');
      setLoading(false);
    }
  }, [sellerId]);

  const handleCreateProduct = () => {
    navigate('/create-product');
  };

  const handleProductDetails = (productId) => {
    navigate(`/product/${productId}`); // Переход на страницу товара
  };

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Загрузка товаров...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="error-container">
        <p className="error-message">{error}</p>
        <button onClick={() => navigate(-1)} className="btn back-btn">
          Назад
        </button>
      </div>
    );
  }

  return (
    <div className="my-products-container">
      <div className="header-section">
        <h1>Мои товары</h1>
        <div className="action-buttons">
          <button onClick={() => navigate('/profile')} className="btn back-btn">
            В профиль
          </button>
          <button onClick={handleCreateProduct} className="btn create-btn">
            + Создать товар
          </button>
        </div>
      </div>

      {products.length === 0 ? (
        <div className="empty-state">
          <p>У вас пока нет товаров</p>
          <button onClick={handleCreateProduct} className="btn create-btn">
            Создать первый товар
          </button>
        </div>
      ) : (
        <div className="products-grid">
          {products.map((product) => (
            <div key={product.productId} className="product-card">
              {product.imageUrl && (
                <img
                  src={product.imageUrl}
                  alt={product.name}
                  className="product-image"
                  onClick={() => handleProductDetails(product.productId)} // Клик по изображению
                />
              )}
              <div className="product-info">
                <h3 onClick={() => handleProductDetails(product.productId)} style={{cursor: 'pointer'}}>
                  {product.name}
                </h3>
                <div className="product-actions">
                  <button
                    onClick={() => handleProductDetails(product.productId)}
                    className="btn details-btn"
                  >
                    Подробнее
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default MyProductsPage;
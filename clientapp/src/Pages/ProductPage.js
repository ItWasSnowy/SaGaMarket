import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useParams, useNavigate } from 'react-router-dom';
import './ProductPage.css';

function ProductPage() {
  const { productId } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState(null);
  const [variants, setVariants] = useState([]);
  const [selectedVariant, setSelectedVariant] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);
        
        // Получаем основную информацию о товаре
        const productResponse = await axios.get(`https://localhost:7182/api/Product/${productId}`);
        setProduct(productResponse.data);
        
        // Получаем варианты товара
        const variantsResponse = await axios.get(`https://localhost:7182/api/products/${productId}/variants`);
        setVariants(variantsResponse.data);
        
        // Выбираем первый вариант по умолчанию
        if (variantsResponse.data.length > 0) {
          setSelectedVariant(variantsResponse.data[0]);
        }
      } catch (err) {
        setError(err.response?.data?.message || err.message || 'Ошибка при загрузке данных');
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [productId]);

  if (loading) return <div className="loading">Загрузка товара...</div>;
  if (error) return <div className="error">{error}</div>;
  if (!product) return <div className="empty">Товар не найден</div>;

  return (
    <div className="product-page">
      <button className="back-button" onClick={() => navigate(-1)}>← Назад к каталогу</button>
      
      <div className="product-main">
        <div className="product-gallery">
          {selectedVariant?.imageUrl ? (
            <img 
              src={selectedVariant.imageUrl} 
              alt={selectedVariant.name} 
              className="main-image"
            />
          ) : (
            <div className="image-placeholder">Нет изображения</div>
          )}
          
          <div className="variants-grid">
            {variants.map(variant => (
              <button
                key={variant.variantId}
                className={`variant-button ${selectedVariant?.variantId === variant.variantId ? 'active' : ''}`}
                onClick={() => setSelectedVariant(variant)}
              >
                {variant.imageUrl ? (
                  <img src={variant.imageUrl} alt={variant.name} />
                ) : (
                  <span>{variant.name}</span>
                )}
              </button>
            ))}
          </div>
        </div>
        
        <div className="product-info">
          <h1>{product.productName}</h1>
          <div className="price">
            {selectedVariant ? (
              <span>{selectedVariant.price} ₽</span>
            ) : (
              <span>{product.minPrice} - {product.maxPrice} ₽</span>
            )}
          </div>
          
          <div className="description">
            <h3>Описание</h3>
            <p>{product.description || 'Описание отсутствует'}</p>
          </div>
        </div>
      </div>
      
      <div className="product-sections">
        <div className="section">
          <h2>Обзоры</h2>
          <div className="placeholder">
            <p>Здесь будут отображаться обзоры товара</p>
          </div>
        </div>
        
        <div className="section">
          <h2>Комментарии</h2>
          <div className="placeholder">
            <p>Здесь будут отображаться комментарии к обзорам</p>
          </div>
        </div>
      </div>
    </div>
  );
}

export default ProductPage;
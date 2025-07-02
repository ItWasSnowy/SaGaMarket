import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './ProductVariantsModal.css';

const ProductVariantsModal = ({ product, onClose }) => {
  const [variants, setVariants] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchVariants = async () => {
      try {
        setLoading(true);
        setError(null);
        
        // Используем новый эндпоинт
        const response = await axios.get(
          `https://localhost:7182/api/products/${product.productId}/variants`
        );
        
        setVariants(response.data);
      } catch (err) {
        setError(err.response?.data?.message || err.message || 'Ошибка при загрузке вариантов');
      } finally {
        setLoading(false);
      }
    };

    if (product) {
      fetchVariants();
    }
  }, [product]);

  if (!product) return null;

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <button className="close-button" onClick={onClose}>×</button>
        <h2>{product.productName} - Варианты</h2>
        
        {loading ? (
          <div className="loading">Загрузка вариантов...</div>
        ) : error ? (
          <div className="error">{error}</div>
        ) : variants.length === 0 ? (
          <div className="empty">Нет доступных вариантов</div>
        ) : (
          <div className="variants-list">
            {variants.map(variant => (
              <div key={variant.variantId} className="variant-item">
                <h3>{variant.name}</h3>
                <p>{variant.description}</p>
                <p className="price">{variant.price} ₽</p>
                <p className="count">Доступно: {variant.count} шт.</p>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default ProductVariantsModal;
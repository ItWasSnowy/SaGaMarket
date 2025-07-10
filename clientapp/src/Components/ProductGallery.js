import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './ProductGallery.css';

const ProductGallery = ({ variantId, productName }) => {
  const [imageUrl, setImageUrl] = useState('/images/placeholder-product-large.png');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchImage = async () => {
      if (!variantId) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError(null);
        
        // Формируем URL для запроса изображения
        const apiUrl = `https://localhost:7182/api/Media/image/${variantId}.png`;
        
        // Сначала проверяем, существует ли изображение через HEAD-запрос
        try {
          await axios.head(apiUrl);
          // Если HEAD-запрос успешен, устанавливаем URL изображения
          setImageUrl(apiUrl);
        } catch (headErr) {
          // Если изображение не найдено, используем placeholder
          setImageUrl('/images/placeholder-product-large.png');
        }
      } catch (err) {
        console.error('Error fetching image:', err);
        setError('Не удалось загрузить изображение');
        setImageUrl('/images/placeholder-product-large.png');
      } finally {
        setLoading(false);
      }
    };

    fetchImage();
  }, [variantId]);

  if (loading) {
    return (
      <div className="gallery-loading">
        <div className="loading-spinner"></div>
      </div>
    );
  }

  return (
    <div className="product-gallery-container">
      <div className="product-image-wrapper">
        <img
          src={imageUrl}
          alt={`${productName}`}
          className="product-main-image"
          onError={(e) => {
            // Fallback если даже placeholder не загрузится
            e.target.src = '/images/placeholder-product-large.png';
            e.target.className = 'product-image-placeholder';
          }}
        />
      </div>
    </div>
  );
};

export default ProductGallery;
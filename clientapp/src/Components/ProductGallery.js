import React, { useState, useEffect } from 'react';
import './ProductGallery.css';

const ProductGallery = ({ variantId, productName }) => {
  const [imageUrl, setImageUrl] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!variantId) {
      setImageUrl(null);
      setLoading(false);
      return;
    }

    const checkImageExists = async () => {
      try {
        setLoading(true);
        const response = await fetch(
          `https://localhost:7182/api/Media/image/${variantId}.png`,
          { method: 'HEAD' }
        );
        
        if (response.ok) {
          setImageUrl(`https://localhost:7182/api/Media/image/${variantId}.png`);
        } else {
          setImageUrl(null);
        }
      } catch (error) {
        console.error('Error checking image:', error);
        setImageUrl(null);
      } finally {
        setLoading(false);
      }
    };

    checkImageExists();
  }, [variantId]);

  if (loading) {
    return <div className="image-loading">Загрузка изображения...</div>;
  }

  return (
    <div className="product-gallery-container">
      <div className="product-image-wrapper">
        {imageUrl ? (
          <img
            src={imageUrl}
            alt={productName}
            className="product-main-image"
            onError={() => setImageUrl(null)}
          />
        ) : (
          <div className="image-placeholder">
            <span>Изображение отсутствует</span>
          </div>
        )}
      </div>
    </div>
  );
};

export default ProductGallery;
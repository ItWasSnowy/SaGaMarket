import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useParams, useNavigate } from 'react-router-dom';
import './ProductPage.css';

function ProductPage() {
  const { productId } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState(null);
  const [variants, setVariants] = useState([]);
  const [reviews, setReviews] = useState([]);
  const [selectedVariant, setSelectedVariant] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [newReview, setNewReview] = useState({
    rating: 0,
    comment: ''
  });

  const renderRating = (rating) => {
    const normalizedRating = rating || 0;
    const fullStars = Math.floor(normalizedRating);
    const hasHalfStar = normalizedRating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);

    return (
      <div className="stars-container">
        {[...Array(fullStars)].map((_, i) => (
          <span key={`full-${i}`} className="star filled">★</span>
        ))}
        {hasHalfStar && (
          <span className="star half">★</span>
        )}
        {[...Array(emptyStars)].map((_, i) => (
          <span key={`empty-${i}`} className="star">★</span>
        ))}
        <span className="rating-value">{normalizedRating.toFixed(1)}</span>
      </div>
    );
  };

  const handleStarClick = (rating) => {
    setNewReview(prev => ({ ...prev, rating }));
  };

  const handleReviewSubmit = async (e) => {
    e.preventDefault();
    try {
      await axios.post(`https://localhost:7182/api/products/${productId}/reviews`, {
        userRating: newReview.rating,
        comment: newReview.comment
      });
      // Обновляем отзывы после отправки
      const response = await axios.get(`https://localhost:7182/api/products/${productId}/reviews`);
      setReviews(response.data);
      setNewReview({ rating: 0, comment: '' });
    } catch (err) {
      setError(err.response?.data?.message || 'Ошибка при отправке отзыва');
    }
  };

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);
        
        const [productRes, variantsRes, reviewsRes] = await Promise.all([
          axios.get(`https://localhost:7182/api/Product/${productId}`),
          axios.get(`https://localhost:7182/api/products/${productId}/variants`),
          axios.get(`https://localhost:7182/api/products/${productId}/reviews`)
        ]);

        setProduct(productRes.data);
        setVariants(variantsRes.data);
        setReviews(reviewsRes.data);
        
        if (variantsRes.data.length > 0) {
          setSelectedVariant(variantsRes.data[0]);
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

  const averageRating = reviews.length > 0 
    ? reviews.reduce((sum, review) => sum + review.userRating, 0) / reviews.length
    : 0;

  return (
    <div className="product-page">
      <button className="back-button" onClick={() => navigate(-1)}>← Назад к каталогу</button>
      
      <div className="product-header">
        <h1>{product.productName}</h1>
        <div className="product-rating-summary">
          {renderRating(averageRating)}
          <span className="reviews-count">
            {reviews.length} {reviews.length === 1 ? 'отзыв' : reviews.length < 5 ? 'отзыва' : 'отзывов'}
          </span>
        </div>
      </div>
      
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
        <div className="reviews-section">
          <h2>Отзывы</h2>
          
          <div className="add-review">
            <h3>Оставить отзыв</h3>
            <form onSubmit={handleReviewSubmit}>
              <div className="rating-input">
                <span>Оценка:</span>
                {[1, 2, 3, 4, 5].map(star => (
                  <span 
                    key={star}
                    className={`star ${star <= newReview.rating ? 'filled' : ''}`}
                    onClick={() => handleStarClick(star)}
                  >
                    ★
                  </span>
                ))}
              </div>
              <textarea
                value={newReview.comment}
                onChange={(e) => setNewReview(prev => ({ ...prev, comment: e.target.value }))}
                placeholder="Ваш отзыв..."
                rows="4"
              />
              <button type="submit">Отправить</button>
            </form>
          </div>
          
          {reviews.length === 0 ? (
            <div className="no-reviews">Пока нет отзывов. Будьте первым!</div>
          ) : (
            <div className="reviews-list">
              {reviews.map(review => (
                <div key={review.reviewId} className="review-card">
                  <div className="review-header">
                    <span className="review-author">{review.author?.username || 'Аноним'}</span>
                    <span className="review-date">
                      {new Date(review.createdAt).toLocaleDateString()}
                    </span>
                    <div className="review-rating">
                      {renderRating(review.userRating)}
                    </div>
                  </div>
                  {review.comment && (
                    <div className="review-content">
                      {review.comment}
                    </div>
                  )}
                  {review.comments?.length > 0 && (
                    <div className="comments-count">
                      {review.comments.length} {review.comments.length === 1 ? 'комментарий' : review.comments.length < 5 ? 'комментария' : 'комментариев'}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default ProductPage;
import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useParams, useNavigate } from 'react-router-dom';
import './ProductPage.css';

function ProductPage({ setCartItemsCount }) {
  const { productId } = useParams();
  const navigate = useNavigate();
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedVariant, setSelectedVariant] = useState(null);
  const [quantity, setQuantity] = useState(1);
  const [reviews, setReviews] = useState([]);
  const [comments, setComments] = useState({});
  const [loadingReviews, setLoadingReviews] = useState(true);
  const [reviewError, setReviewError] = useState(null);
  const [averageRating, setAverageRating] = useState(0);
  const [authorNames, setAuthorNames] = useState({});
  const [loadingComments, setLoadingComments] = useState({});

  // Загрузка данных товара
  useEffect(() => {
    const fetchProduct = async () => {
      try {
        setLoading(true);
        const response = await axios.get(`https://localhost:7182/api/Product/${productId}`);
        setProduct(response.data);
        setSelectedVariant(response.data.variants?.[0] || null);
      } catch (err) {
        setError(err.response?.data?.message || 'Не удалось загрузить товар');
      } finally {
        setLoading(false);
      }
    };

    if (productId) fetchProduct();
  }, [productId]);

  // Загрузка отзывов и авторов
  useEffect(() => {
    const fetchReviews = async () => {
      try {
        setLoadingReviews(true);
        const response = await axios.get(
          `https://localhost:7182/api/Review/products/${productId}/reviews`
        );
        setReviews(response.data || []);

        const ids = [...new Set(response.data.map(r => r.authorId))];
        const names = {};
        await Promise.all(ids.map(async id => {
          try {
            const user = await axios.get(`https://localhost:7182/api/User/${id}/name`);
            names[id] = user.data;
          } catch {
            names[id] = 'Аноним';
          }
        }));
        setAuthorNames(names);
      } catch (err) {
        setReviewError('Не удалось загрузить отзывы');
      } finally {
        setLoadingReviews(false);
      }
    };

    if (productId) fetchReviews();
  }, [productId]);

  // Расчет средней оценки
  useEffect(() => {
    if (reviews.length > 0) {
      const validReviews = reviews.filter(r => r.userRating != null);
      const sum = validReviews.reduce((acc, r) => acc + r.userRating, 0);
      setAverageRating(sum / validReviews.length || 0);
    }
  }, [reviews]);

  // Загрузка комментариев
  const fetchComments = async (reviewId) => {
    try {
      setLoadingComments(prev => ({ ...prev, [reviewId]: true }));
      
      const response = await axios.get(
        `https://localhost:7182/api/Comment/by-review/${reviewId}`,
        { withCredentials: true }
      );

      // Форматирование комментариев
      const commentsData = Array.isArray(response.data) ? response.data : [response.data];
      const formattedComments = commentsData.map(comment => ({
        id: comment.id || comment.commentId,
        author: comment.authorName || comment.author?.name || 'Аноним',
        text: comment.text || comment.content || comment.message || '',
        date: comment.createdAt || comment.date || new Date().toISOString()
      }));

      setComments(prev => ({ ...prev, [reviewId]: formattedComments }));
    } catch (err) {
      console.error('Ошибка загрузки комментариев:', err);
      setComments(prev => ({ ...prev, [reviewId]: [] }));
    } finally {
      setLoadingComments(prev => ({ ...prev, [reviewId]: false }));
    }
  };

  const toggleComments = (reviewId) => {
    if (!comments[reviewId] && !loadingComments[reviewId]) {
      fetchComments(reviewId);
    } else if (comments[reviewId]) {
      setComments(prev => ({ ...prev, [reviewId]: undefined }));
    }
  };

  const handleAddToCart = async () => {
    if (!selectedVariant) return;

    try {
      const response = await axios.post('https://localhost:7182/api/Cart/add', {
        productId: product.productId,
        variantId: selectedVariant.variantId,
        quantity: quantity
      }, {
        headers: {
          'Content-Type': 'application/json'
        },
        withCredentials: true
      });

      if (response.status === 200) {
        setCartItemsCount(prev => prev + quantity);
        alert('Товар добавлен в корзину');
      }
    } catch (error) {
      console.error('Ошибка при добавлении в корзину:', error);
      alert('Не удалось добавить товар в корзину');
    }
  };

  const renderRatingStars = (rating) => {
    const stars = [];
    const fullStars = Math.floor(rating);
    const hasHalf = rating % 1 >= 0.5;
    
    for (let i = 0; i < 5; i++) {
      if (i < fullStars) {
        stars.push(<span key={i} className="star filled">★</span>);
      } else if (i === fullStars && hasHalf) {
        stars.push(<span key={i} className="star half">★</span>);
      } else {
        stars.push(<span key={i} className="star">★</span>);
      }
    }
    
    return (
      <div className="rating-stars">
        {stars}
        <span className="rating-value">{rating.toFixed(1)}</span>
      </div>
    );
  };

  const renderComments = (reviewId) => {
    if (!comments[reviewId]) return null;

    return (
      <div className="comments-list">
        {comments[reviewId].length > 0 ? (
          comments[reviewId].map(comment => (
            <div key={comment.id} className="comment">
              <div className="comment-header">
                <span>{comment.author}</span>
                <span>{new Date(comment.date).toLocaleDateString()}</span>
              </div>
              <p className="comment-text">{comment.text}</p>
            </div>
          ))
        ) : (
          <div className="no-comments">Нет комментариев</div>
        )}
      </div>
    );
  };

  if (loading) return <div className="loading">Загрузка...</div>;
  if (error) return <div className="error">{error}</div>;
  if (!product) return <div className="not-found">Товар не найден</div>;

  return (
    <div className="product-page">
      <button className="back-button" onClick={() => navigate(-1)}>← Назад</button>

      <div className="product-content">
        <div className="product-gallery">
          {selectedVariant?.imageUrl ? (
            <img
              src={selectedVariant.imageUrl}
              alt={product.name}
              className="product-main-image"
              onError={(e) => {
                e.target.src = '/placeholder-product-large.png';
                e.target.className = 'product-image-placeholder';
              }}
            />
          ) : (
            <div className="product-image-placeholder">
              <span>Нет изображения</span>
            </div>
          )}
        </div>

        <div className="product-details">
          <h1 className="product-title">{product.name}</h1>
          
          <div className="price-section">
            <span className="product-price">
              {selectedVariant 
                ? `${selectedVariant.price.toLocaleString()} ₽` 
                : 'Цена не указана'}
            </span>
            <span className={`stock-status ${selectedVariant?.count > 0 ? 'in-stock' : 'out-of-stock'}`}>
              {selectedVariant?.count > 0 
                ? `В наличии: ${selectedVariant.count} шт.` 
                : 'Нет в наличии'}
            </span>
          </div>

          {reviews.length > 0 && (
            <div className="product-rating-summary">
              {renderRatingStars(averageRating)}
              <span className="reviews-count">({reviews.length} отзывов)</span>
            </div>
          )}

          {product.variants?.length > 1 && (
            <div className="variants-section">
              <h3>Варианты:</h3>
              <div className="variants-grid">
                {product.variants.map(variant => (
                  <button
                    key={variant.variantId}
                    className={`variant-button ${
                      selectedVariant?.variantId === variant.variantId ? 'active' : ''
                    }`}
                    onClick={() => {
                      setSelectedVariant(variant);
                      setQuantity(1);
                    }}
                  >
                    {variant.name}
                  </button>
                ))}
              </div>
            </div>
          )}

          <div className="quantity-selector">
            <label>Количество:</label>
            <input
              type="number"
              min="1"
              max={selectedVariant?.count || 1}
              value={quantity}
              onChange={(e) => {
                const value = Math.max(1, Math.min(selectedVariant?.count || 1, Number(e.target.value) || 1));
                setQuantity(value);
              }}
            />
          </div>

          <button
            className="add-to-cart-button"
            onClick={handleAddToCart}
            disabled={!selectedVariant || selectedVariant.count < 1}
          >
            {selectedVariant?.count > 0 ? 'Добавить в корзину' : 'Нет в наличии'}
          </button>

          <div className="product-description">
            <h3>Описание</h3>
            <p>{selectedVariant?.description || product.description || 'Описание отсутствует'}</p>
          </div>
        </div>
      </div>

      <div className="reviews-section">
        <h2>Отзывы {reviews.length > 0 && `(${reviews.length})`}</h2>
        
        {reviews.length > 0 && (
          <div className="average-rating">
            Средняя оценка: {renderRatingStars(averageRating)}
          </div>
        )}

        {loadingReviews ? (
          <div>Загрузка отзывов...</div>
        ) : reviewError ? (
          <div className="error">{reviewError}</div>
        ) : reviews.length === 0 ? (
          <div>Пока нет отзывов</div>
        ) : (
          <div className="reviews-list">
            {reviews.map(review => (
              <div key={review.reviewId} className="review">
                <div className="review-header">
                  <span>{authorNames[review.authorId] || 'Аноним'}</span>
                  <span>{new Date(review.createdAt).toLocaleDateString()}</span>
                  {review.userRating != null && (
                    <div className="review-rating">
                      Оценка: {renderRatingStars(review.userRating)}
                    </div>
                  )}
                </div>
                <div className="review-content">
                  {review.comment && <p>{review.comment}</p>}
                </div>

                <div className="comments-section">
                  <button
                    onClick={() => toggleComments(review.reviewId)}
                    disabled={loadingComments[review.reviewId]}
                  >
                    {loadingComments[review.reviewId] 
                      ? 'Загрузка...' 
                      : comments[review.reviewId] 
                        ? 'Скрыть комментарии' 
                        : 'Показать комментарии'}
                  </button>
                  {renderComments(review.reviewId)}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default ProductPage;
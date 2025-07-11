import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useParams, useNavigate } from 'react-router-dom';
import ProductGallery from '../Components/ProductGallery'; 
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
  const [reviewsCount, setReviewsCount] = useState(0);
  const [authorNames, setAuthorNames] = useState({});
  const [loadingComments, setLoadingComments] = useState({});
  const [showReviewForm, setShowReviewForm] = useState(false);
  const [reviewRating, setReviewRating] = useState(5);
  const [reviewText, setReviewText] = useState('');
  const [submittingReview, setSubmittingReview] = useState(false);
  const [reviewSubmitError, setReviewSubmitError] = useState(null);
  const [showCommentForms, setShowCommentForms] = useState({});
  const [commentTexts, setCommentTexts] = useState({});
  const [submittingComments, setSubmittingComments] = useState({});
  const [commentErrors, setCommentErrors] = useState({});

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

  useEffect(() => {
    const fetchReviews = async () => {
      try {
        setLoadingReviews(true);
        const response = await axios.get(
          `https://localhost:7182/api/Review/products/${productId}/reviews`
        );
        
        const reviewsData = Array.isArray(response.data) ? response.data : [response.data];
        setReviews(reviewsData);
        setReviewsCount(reviewsData.length);

        const authorIds = [...new Set(reviewsData.map(r => r.authorId))];
        const names = {};
        
        await Promise.all(authorIds.map(async id => {
          try {
            const userResponse = await axios.get(`https://localhost:7182/api/User/${id}/name`);
            names[id] = userResponse.data;
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

  useEffect(() => {
    if (reviews.length > 0) {
      const validReviews = reviews.filter(r => r.userRating != null);
      const sum = validReviews.reduce((acc, r) => acc + r.userRating, 0);
      setAverageRating(sum / validReviews.length || 0);
    } else {
      setAverageRating(0);
    }
  }, [reviews]);

  const fetchComments = async (reviewId) => {
    try {
      setLoadingComments(prev => ({ ...prev, [reviewId]: true }));
      
      const response = await axios.get(
        `https://localhost:7182/api/Comment/by-review/${reviewId}`,
        { withCredentials: true }
      );

      let commentsData = [];
      if (Array.isArray(response.data)) {
        commentsData = response.data;
      } else if (response.data && typeof response.data === 'object') {
        commentsData = [response.data];
      }

      const formattedComments = commentsData.map(comment => ({
        id: comment.id || comment.commentId || Math.random().toString(36).substr(2, 9),
        author: comment.authorName || comment.author?.name || 'Аноним',
        text: comment.text || comment.content || comment.message || comment.commentText || '',
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

  const toggleCommentForm = (reviewId) => {
    setShowCommentForms(prev => ({
      ...prev,
      [reviewId]: !prev[reviewId]
    }));
    setCommentTexts(prev => ({
      ...prev,
      [reviewId]: ''
    }));
    setCommentErrors(prev => ({
      ...prev,
      [reviewId]: null
    }));
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

  const handleSubmitReview = async () => {
    if (!reviewText.trim()) {
      setReviewSubmitError('Пожалуйста, напишите отзыв');
      return;
    }

    try {
      setSubmittingReview(true);
      setReviewSubmitError(null);

      const response = await axios.post(
        'https://localhost:7182/api/Review',
        {
          productId: productId,
          userRating: reviewRating,
          textReview: reviewText
        },
        {
          withCredentials: true,
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );

      if (response.status === 201) {
        setShowReviewForm(false);
        setReviewText('');
        setReviewRating(5);
        
        const reviewsResponse = await axios.get(
          `https://localhost:7182/api/Review/products/${productId}/reviews`
        );
        
        const updatedReviews = Array.isArray(reviewsResponse.data) ? reviewsResponse.data : [reviewsResponse.data];
        setReviews(updatedReviews);
        setReviewsCount(updatedReviews.length);

        if (response.data?.authorId && !authorNames[response.data.authorId]) {
          try {
            const userResponse = await axios.get(`https://localhost:7182/api/User/${response.data.authorId}/name`);
            setAuthorNames(prev => ({
              ...prev,
              [response.data.authorId]: userResponse.data
            }));
          } catch {
            setAuthorNames(prev => ({
              ...prev,
              [response.data.authorId]: 'Аноним'
            }));
          }
        }
      }
    } catch (error) {
      console.error('Ошибка при отправке отзыва:', error);
      setReviewSubmitError(error.response?.data?.message || 'Не удалось отправить отзыв');
    } finally {
      setSubmittingReview(false);
    }
  };

  const handleSubmitComment = async (reviewId) => {
    const commentText = commentTexts[reviewId] || '';
    
    if (!commentText.trim()) {
      setCommentErrors(prev => ({
        ...prev,
        [reviewId]: 'Пожалуйста, напишите комментарий'
      }));
      return;
    }

    try {
      setSubmittingComments(prev => ({
        ...prev,
        [reviewId]: true
      }));
      setCommentErrors(prev => ({
        ...prev,
        [reviewId]: null
      }));

      const response = await axios.post(
        'https://localhost:7182/api/Comment',
        {
          reviewId: reviewId,
          commentText: commentText
        },
        {
          withCredentials: true,
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );

      if (response.status === 201) {
        setShowCommentForms(prev => ({
          ...prev,
          [reviewId]: false
        }));
        setCommentTexts(prev => ({
          ...prev,
          [reviewId]: ''
        }));
        await fetchComments(reviewId);
      }
    } catch (error) {
      console.error('Ошибка при отправке комментария:', error);
      setCommentErrors(prev => ({
        ...prev,
        [reviewId]: error.response?.data?.message || 'Не удалось отправить комментарий'
      }));
    } finally {
      setSubmittingComments(prev => ({
        ...prev,
        [reviewId]: false
      }));
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
        stars.push(<span key={i} className="star empty">★</span>);
      }
    }
    
    return (
      <div className="rating-stars">
        {stars}
        <span className="rating-value">{rating.toFixed(1)}</span>
      </div>
    );
  };

  const renderReviewForm = () => (
    <div className="review-form">
      <h3>Оставить отзыв</h3>
      <div className="form-group">
        <label>Ваша оценка:</label>
        <div className="rating-selector">
          {[1, 2, 3, 4, 5].map((star) => (
            <span
              key={star}
              className={`star-select ${reviewRating >= star ? 'selected' : ''}`}
              onClick={() => setReviewRating(star)}
            >
              ★
            </span>
          ))}
        </div>
      </div>
      <div className="form-group">
        <label>Ваш отзыв:</label>
        <textarea
          value={reviewText}
          onChange={(e) => setReviewText(e.target.value)}
          placeholder="Расскажите о вашем опыте использования товара"
          rows="5"
        />
      </div>
      {reviewSubmitError && <div className="error">{reviewSubmitError}</div>}
      <div className="form-actions">
        <button 
          onClick={handleSubmitReview}
          disabled={submittingReview}
          className="submit-review-btn"
        >
          {submittingReview ? 'Отправка...' : 'Отправить отзыв'}
        </button>
        <button 
          onClick={() => setShowReviewForm(false)}
          className="cancel-review-btn"
        >
          Отмена
        </button>
      </div>
    </div>
  );

  const renderCommentForm = (reviewId) => (
    <div className="comment-form">
      <div className="form-group">
        <label>Ваш комментарий:</label>
        <textarea
          value={commentTexts[reviewId] || ''}
          onChange={(e) => setCommentTexts(prev => ({
            ...prev,
            [reviewId]: e.target.value
          }))}
          placeholder="Напишите ваш комментарий"
          rows="3"
        />
      </div>
      {commentErrors[reviewId] && <div className="error">{commentErrors[reviewId]}</div>}
      <div className="form-actions">
        <button 
          onClick={() => handleSubmitComment(reviewId)}
          disabled={submittingComments[reviewId]}
          className="submit-comment-btn"
        >
          {submittingComments[reviewId] ? 'Отправка...' : 'Отправить комментарий'}
        </button>
        <button 
          onClick={() => toggleCommentForm(reviewId)}
          className="cancel-comment-btn"
        >
          Отмена
        </button>
      </div>
    </div>
  );

  const renderComments = (reviewId) => {
    if (!comments[reviewId]) return null;

    return (
      <div className="comments-section">
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
        
        {!showCommentForms[reviewId] ? (
          <button
            className="add-comment-btn"
            onClick={() => toggleCommentForm(reviewId)}
          >
            Добавить комментарий
          </button>
        ) : (
          renderCommentForm(reviewId)
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
          <ProductGallery 
            variantId={selectedVariant?.variantId} 
            productName={product.name} 
          />
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

          {reviewsCount > 0 && (
            <div className="product-rating-summary">
              {renderRatingStars(averageRating)}
              <span className="reviews-count">({reviewsCount} отзывов)</span>
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
        <div className="reviews-header">
          <h2>Отзывы {reviewsCount > 0 && `(${reviewsCount})`}</h2>
          <button 
            onClick={() => setShowReviewForm(!showReviewForm)}
            className="leave-review-btn"
          >
            {showReviewForm ? 'Скрыть форму' : 'Оставить отзыв'}
          </button>
        </div>

        {showReviewForm && renderReviewForm()}

        {reviewsCount > 0 && (
          <div className="average-rating">
            Средняя оценка: {renderRatingStars(averageRating)}
          </div>
        )}

        {loadingReviews ? (
          <div>Загрузка отзывов...</div>
        ) : reviewError ? (
          <div className="error">{reviewError}</div>
        ) : reviewsCount === 0 ? (
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
                  {review.textReview && <p>{review.textReview}</p>}
                </div>

                <div className="comments-toggle">
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
                </div>
                
                {comments[review.reviewId] && renderComments(review.reviewId)}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default ProductPage;
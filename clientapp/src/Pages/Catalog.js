import React, { useState, useEffect, useRef } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import { FaHeart, FaRegHeart, FaSpinner } from 'react-icons/fa';
import ProductGallery from '../Components/ProductGallery';
import { useAuth } from '../authContext';
import './Catalog.css';

function Catalog() {
  const { user } = useAuth();
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 12,
    totalCount: 0
  });
  const [inputPage, setInputPage] = useState('');
  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState('');
  const [minRating, setMinRating] = useState(0);
  const [searchTerm, setSearchTerm] = useState('');
  const [favoritesLoading, setFavoritesLoading] = useState({});
  const abortControllerRef = useRef(null);
  const variantsCache = useRef({});
  const reviewsCache = useRef({});
  const navigate = useNavigate();

  const fetchCategories = async () => {
    try {
      const response = await axios.get('https://localhost:7182/api/Product/categories');
      const filteredCategories = response.data.filter(
        category => category !== null && category !== undefined && category !== ''
      );
      setCategories(filteredCategories);
    } catch (error) {
      console.error('Error fetching categories:', error);
      setError('Не удалось загрузить категории');
    }
  };

  const fetchVariantsForProduct = async (productId) => {
    if (variantsCache.current[productId]) {
      return variantsCache.current[productId];
    }

    try {
      const response = await axios.get(
        `https://localhost:7182/api/Variant/products/${productId}/variants`,
        { timeout: 5000 }
      );
      variantsCache.current[productId] = response.data;
      return response.data;
    } catch (error) {
      console.error(`Error fetching variants for product ${productId}:`, error);
      return [];
    }
  };

  const fetchReviewsForProduct = async (productId) => {
    if (reviewsCache.current[productId]) {
      return reviewsCache.current[productId];
    }

    try {
      const response = await axios.get(
        `https://localhost:7182/api/Review/products/${productId}/reviews`,
        { timeout: 5000 }
      );
      reviewsCache.current[productId] = response.data;
      return response.data;
    } catch (error) {
      console.error(`Error fetching reviews for product ${productId}:`, error);
      return [];
    }
  };

  const calculateProductRating = (reviews) => {
    if (!reviews || reviews.length === 0) return { averageRating: 0, reviewsCount: 0 };

    const totalRating = reviews.reduce((sum, review) => sum + review.userRating, 0);
    const averageRating = totalRating / reviews.length;
    return {
      averageRating: parseFloat(averageRating.toFixed(1)),
      reviewsCount: reviews.length
    };
  };

  const toggleFavorite = async (productId, e) => {
    e.stopPropagation();
    
    if (!user) {
      alert('Для добавления в избранное необходимо авторизоваться');
      navigate('/login');
      return;
    }

    setFavoritesLoading(prev => ({ ...prev, [productId]: true }));
    
    try {
      const product = products.find(p => p.productId === productId);
      const isCurrentlyFavorite = product.isFavorite;
      
      const config = {
        headers: {
          'Authorization': `Bearer ${user.token}`,
          'Content-Type': 'application/json'
        }
      };

      if (isCurrentlyFavorite) {
        // Удаление из избранного
        await axios.delete(`https://localhost:7182/api/favorites?productId=${productId}`, config);
      } else {
        // Добавление в избранное
        await axios.post(
          'https://localhost:7182/api/favorites/add',
          { productId },
          config
        );
      }

      setProducts(products.map(p => 
        p.productId === productId ? { ...p, isFavorite: !isCurrentlyFavorite } : p
      ));
    } catch (error) {
      console.error('Ошибка при обновлении избранного:', error);
      alert('Не удалось обновить избранное. Пожалуйста, попробуйте позже.');
    } finally {
      setFavoritesLoading(prev => ({ ...prev, [productId]: false }));
    }
  };

  const fetchProducts = async (page = 1) => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    const controller = new AbortController();
    abortControllerRef.current = controller;

    try {
      setLoading(true);
      setError(null);
      
      const productsResponse = await axios.get('https://localhost:7182/api/Product/filtered', {
        params: { 
          page, 
          pageSize: pagination.pageSize,
          category: selectedCategory || undefined,
          minRating: minRating > 0 ? minRating : undefined,
          searchTerm: searchTerm || undefined
        },
        signal: controller.signal
      });

      const totalCount = parseInt(productsResponse.headers['x-total-count'], 10) || 0;
      
      const productsWithVariantsAndReviews = await Promise.all(
        productsResponse.data.map(async product => {
          const [variants, reviews] = await Promise.all([
            fetchVariantsForProduct(product.productId),
            fetchReviewsForProduct(product.productId)
          ]);
          
          const { averageRating, reviewsCount } = calculateProductRating(reviews);
          
          return {
            ...product,
            variants,
            averageRating,
            reviewsCount,
            isFavorite: product.isFavorite || false
          };
        })
      );

      const processedProducts = productsWithVariantsAndReviews.map(product => {
        const prices = product.variants.length > 0 
          ? product.variants.map(v => v.price)
          : [product.price || 0];
        
        return {
          ...product,
          productName: product.productName || 'Без названия',
          minPrice: Math.min(...prices),
          maxPrice: Math.max(...prices),
          variants: product.variants
        };
      }).filter(p => p.productId);

      setProducts(processedProducts);
      setPagination(prev => ({
        ...prev,
        page,
        totalCount
      }));
      setInputPage('');

    } catch (error) {
      if (axios.isCancel(error)) return;
      setError(error.response?.data?.message || 'Ошибка при загрузке каталога');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCategories();
    fetchProducts(pagination.page);
    return () => abortControllerRef.current?.abort();
  }, []);

  useEffect(() => {
    fetchProducts(1);
  }, [selectedCategory, minRating, searchTerm]);

  const handlePrevPage = () => {
    if (pagination.page > 1) {
      fetchProducts(pagination.page - 1);
    }
  };

  const handleNextPage = () => {
    const totalPages = Math.ceil(pagination.totalCount / pagination.pageSize);
    if (pagination.page < totalPages) {
      fetchProducts(pagination.page + 1);
    }
  };

  const handlePageJump = () => {
    const pageNum = parseInt(inputPage);
    const totalPages = Math.ceil(pagination.totalCount / pagination.pageSize);
    
    if (!isNaN(pageNum)) { 
      if (pageNum >= 1 && pageNum <= totalPages) {
        fetchProducts(pageNum);
      } else {
        setError(`Введите номер страницы от 1 до ${totalPages}`);
      }
    }
  };

  const handleProductClick = (productId) => {
    navigate(`/product/${productId}`);
  };

  const renderRatingStars = (rating) => {
    if (rating === 0 || rating === null || rating === undefined) {
      return <span className="no-rating">Нет оценок</span>;
    }
    
    const stars = [];
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;

    for (let i = 1; i <= 5; i++) {
      if (i <= fullStars) {
        stars.push(<span key={i} className="star filled">★</span>);
      } else if (i === fullStars + 1 && hasHalfStar) {
        stars.push(<span key={i} className="star half">★</span>);
      } else {
        stars.push(<span key={i} className="star empty">☆</span>);
      }
    }

    return (
      <div className="rating-stars">
        {stars}
        <span className="rating-value">{rating.toFixed(1)}</span>
      </div>
    );
  };

  const renderProductCard = (product) => {
    const firstVariant = product.variants.length > 0 ? product.variants[0] : null;
    
    return (
      <div 
        key={product.productId}
        className="product-card"
        onClick={() => handleProductClick(product.productId)}
      >
        <div className="product-image-container">
          <ProductGallery 
            variantId={firstVariant?.variantId} 
            productName={product.productName} 
          />
          
          <button
            className={`favorite-btn ${product.isFavorite ? 'active' : ''}`}
            onClick={(e) => toggleFavorite(product.productId, e)}
            disabled={favoritesLoading[product.productId]}
            aria-label={product.isFavorite ? 'Удалить из избранного' : 'Добавить в избранное'}
          >
            {favoritesLoading[product.productId] ? (
              <FaSpinner className="spinner-icon" />
            ) : product.isFavorite ? (
              <FaHeart color="#ff4d4d" />
            ) : (
              <FaRegHeart />
            )}
          </button>
        </div>
        
        <div className="product-info">
          <h3 className="product-title">
            {product.productName}
          </h3>
          <p className="product-price">
            {product.minPrice === product.maxPrice
              ? `${product.minPrice.toLocaleString()} ₽`
              : `${product.minPrice.toLocaleString()} - ${product.maxPrice.toLocaleString()} ₽`}
          </p>
          
          <div className="product-rating">
            {product.averageRating > 0 ? (
              <>
                {renderRatingStars(product.averageRating)}
                {product.reviewsCount > 0 && (
                  <span className="reviews-count">({product.reviewsCount})</span>
                )}
              </>
            ) : (
              <span className="no-rating">Нет оценок</span>
            )}
          </div>

          {product.variants.length > 0 && (
            <div className="product-variants">
              Варианты: {product.variants.slice(0, 3).map(v => v.name).join(', ')}
              {product.variants.length > 3 && '...'}
            </div>
          )}
        </div>
      </div>
    );
  };

  const totalPages = Math.ceil(pagination.totalCount / pagination.pageSize);

  if (error) {
    return (
      <div className="error-container">
        <h3>Ошибка</h3>
        <p>{error}</p>
        <button 
          onClick={() => fetchProducts(pagination.page)}
          className="retry-button"
        >
          Повторить попытку
        </button>
      </div>
    );
  }

  if (loading && products.length === 0) {
    return (
      <div className="loading-container">
        <FaSpinner className="spinner" />
        <p>Загрузка товаров...</p>
      </div>
    );
  }

  return (
    <div className="catalog">
      <h1>Каталог товаров</h1>
      
      <div className="filters">
        <div className="search-box">
          <input
            type="text"
            placeholder="Поиск по названию..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        
        <div className="filter-group">
          <label>Категория:</label>
          <select
            value={selectedCategory}
            onChange={(e) => setSelectedCategory(e.target.value)}
          >
            <option value="">Все категории</option>
            {categories.map(category => (
              <option key={category} value={category}>
                {category}
              </option>
            ))}
          </select>
        </div>
        
        <div className="filter-group">
          <label>Минимальный рейтинг:</label>
          <div className="rating-filter">
            {[0, 1, 2, 3, 4, 5].map(rating => (
              <button
                key={rating}
                className={`rating-star ${minRating === rating ? 'active' : ''}`}
                onClick={() => setMinRating(rating)}
              >
                {rating === 0 ? 'Любой' : `${rating}+`}
              </button>
            ))}
          </div>
        </div>
      </div>
      
      <div className="products-grid">
        {products.map(renderProductCard)}
      </div>

      {pagination.totalCount > 0 && (
        <div className="pagination-container">
          <div className="pagination">
            <button 
              onClick={handlePrevPage}
              disabled={pagination.page === 1 || loading}
              className="pagination-button"
            >
              Назад
            </button>
            
            <span className="page-info">
              Страница {pagination.page} из {totalPages}
            </span>
            
            <button 
              onClick={handleNextPage}
              disabled={pagination.page >= totalPages || loading}
              className="pagination-button"
            >
              Вперед
            </button>
          </div>
          
          <div className="page-jump">
            <input
              type="number"
              min="1"
              max={totalPages}
              value={inputPage}
              onChange={(e) => setInputPage(e.target.value)}
              onKeyPress={(e) => e.key === 'Enter' && handlePageJump()}
              placeholder="№ страницы"
              className="page-input"
            />
            <button 
              onClick={handlePageJump}
              disabled={loading}
              className="jump-button"
            >
              Перейти
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default Catalog;
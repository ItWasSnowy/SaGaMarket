import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import './ProductList.css';

const ProductList = () => {
  const [products, setProducts] = useState([]);
  const [variants, setVariants] = useState({});
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const navigate = useNavigate();

  const loadProducts = useCallback(async () => {
    if (loading || !hasMore) return;

    setLoading(true);
    try {
      const response = await axios.get(`/api/product?page=${page}&pageSize=10`);
      const newProducts = response.data;

      setProducts(prev => [...prev, ...newProducts]);
      setPage(prev => prev + 1);
      setHasMore(newProducts.length > 0);
    } catch (error) {
      console.error('Error loading products:', error);
    } finally {
      setLoading(false);
    }
  }, [page, loading, hasMore]);

  const loadVariants = async (productId) => {
    try {
      const response = await axios.get(`/api/variant?productId=${productId}`);
      setVariants(prev => ({ ...prev, [productId]: response.data }));
    } catch (error) {
      console.error('Error loading variants:', error);
    }
  };

  useEffect(() => {
    const handleScroll = () => {
      if (
        window.innerHeight + document.documentElement.scrollTop >=
        document.documentElement.offsetHeight - 200
      ) {
        loadProducts();
      }
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, [loadProducts]);

  useEffect(() => {
    loadProducts();
  }, []);

  return (
    <div className="product-list-container">
      <div className="products-grid">
        {products.map(product => (
          <div key={product.productId} className="product-card">
            <div 
              className="product-main"
              onClick={() => loadVariants(product.productId)}
            >
              <img 
                src={product.imageUrl || 'https://via.placeholder.com/200'} 
                alt={product.name} 
                className="product-image" 
              />
              <h3 className="product-title">{product.name}</h3>
              <p className="product-price">
                {product.minPrice !== product.maxPrice 
                  ? `${product.minPrice} ₽ - ${product.maxPrice} ₽` 
                  : `${product.minPrice} ₽`}
              </p>
            </div>

            {variants[product.productId] && (
              <div className="variants-container">
                {variants[product.productId].map(variant => (
                  <div 
                    key={variant.variantId}
                    className="variant-card"
                    onClick={() => navigate(`/variant/${variant.variantId}`)}
                  >
                    <h4>{variant.name}</h4>
                    <p>{variant.price} ₽</p>
                    <button 
                      className="add-to-cart"
                      onClick={(e) => {
                        e.stopPropagation();
                        // Логика добавления в корзину
                      }}
                    >
                      В корзину
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>
      {loading && <div className="loading-indicator">Загрузка...</div>}
    </div>
  );
};

export default ProductList;
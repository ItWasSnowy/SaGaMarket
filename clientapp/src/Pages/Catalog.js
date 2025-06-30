import React, { useState, useEffect } from 'react';
import axios from 'axios';
import './Catalog.css';

// Настройка axios (лучше вынести в отдельный файл)
axios.defaults.baseURL = 'http://localhost:7182'; // 

function Catalog() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchProducts = async () => {
      try {
        setLoading(true);
        const response = await axios.get('http://localhost:7182/api/product', {
    params: { page: 1, pageSize: 10 },
    timeout: 1000 // 10 секунд
});
        
        if (!response.data) throw new Error('Нет данных в ответе');
        setProducts(response.data);
      } catch (err) {
        setError(err.message);
        console.error('Ошибка запроса:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, []);

  if (error) return <div className="error">Ошибка: {error}</div>;
  if (loading) return <div>Загрузка...</div>;

  return (
    <div className="catalog">
      {products.map(product => (
        <div key={product.productId} className="product-card">
          <h3>{product.productName}</h3>
          <p>Цена: {product.price} ₽</p>
        </div>
      ))}
    </div>
  );
}

export default Catalog;
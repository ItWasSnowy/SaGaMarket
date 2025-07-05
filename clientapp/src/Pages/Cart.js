import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Cart.css';

function Cart({ setCartItemsCount }) {
  const [cartState, setCartState] = useState({
    items: [],
    isLoading: true,
    error: null,
    isEmpty: true
  });
  const navigate = useNavigate();

  const getCurrentUser = () => {
    const userData = localStorage.getItem('userData');
    if (!userData) {
      navigate('/login');
      return null;
    }
    return JSON.parse(userData);
  };

  const fetchCartItems = async () => {
    const user = getCurrentUser();
    if (!user?.userId) return;

    try {
      setCartState(prev => ({ ...prev, isLoading: true, error: null }));

      const itemsResponse = await fetch(`https://localhost:7182/api/cart/items?userId=${user.userId}`, {
        credentials: 'include'
      });

      if (itemsResponse.status === 401) {
        localStorage.removeItem('userData');
        navigate('/login');
        return;
      }

      if (!itemsResponse.ok) {
        throw new Error(`Ошибка сервера: ${itemsResponse.status}`);
      }

      const variantIds = await itemsResponse.json();

      if (Array.isArray(variantIds) && variantIds.length > 0) {
        const baseUrl = 'https://localhost:7182/api/cart/info?';
        const queryParams = variantIds.map(id => `variantIds=${encodeURIComponent(id)}`).join('&');
        const detailsUrl = baseUrl + queryParams;

        const detailsResponse = await fetch(detailsUrl, {
          credentials: 'include'
        });

        if (!detailsResponse.ok) {
          throw new Error(`Ошибка загрузки деталей: ${detailsResponse.status}`);
        }

        const productsData = await detailsResponse.json();

        setCartState({
          items: productsData,
          isLoading: false,
          error: null,
          isEmpty: productsData.length === 0
        });
        setCartItemsCount(productsData.length);
      } else {
        setCartState({
          items: [],
          isLoading: false,
          error: null,
          isEmpty: true
        });
        setCartItemsCount(0);
      }
    } catch (error) {
      console.error('Cart loading error:', error);
      setCartState({
        items: [],
        isLoading: false,
        error: error.message,
        isEmpty: true
      });
      setCartItemsCount(0);
    }
  };

  const removeItem = async (variantId) => {
    const user = getCurrentUser();
    if (!user?.userId) return;

    try {
      setCartState(prev => ({ ...prev, isLoading: true }));

      const response = await fetch('https://localhost:7182/api/cart/remove', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          userId: user.userId,
          variantId: variantId
        }),
        credentials: 'include'
      });

      if (!response.ok) {
        throw new Error(`Ошибка удаления: ${response.status}`);
      }

      await fetchCartItems();
    } catch (error) {
      console.error('Remove item error:', error);
      setCartState(prev => ({
        ...prev,
        error: error.message,
        isLoading: false
      }));
    }
  };

  useEffect(() => {
    fetchCartItems();
  }, []);

  const formatPrice = (price) => {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 0
    }).format(price);
  };

  const calculateTotal = () => {
    return cartState.items.reduce((total, item) => total + (item.price || 0), 0);
  };

  const CartItem = ({ item, onRemove }) => {
    const [imageError, setImageError] = useState(false);

    return (
      <div className="cart-item">
        <div className="cart-item-image">
          {item.imageUrl && !imageError ? (
            <img
              src={item.imageUrl}
              alt={item.productName}
              onError={() => setImageError(true)}
            />
          ) : (
            <div className="cart-image-placeholder">
              <span>Нет изображения</span>
            </div>
          )}
        </div>
        
        <div className="cart-item-details">
          <h3 className="cart-item-title">{item.productName}</h3>
          {item.variantName && <p className="cart-item-variant">Вариант: {item.variantName}</p>}
          <p className="cart-item-price">{formatPrice(item.price)}</p>
          <p className="cart-item-stock">Доступно: {item.availableCount} шт.</p>
        </div>
        
        <button
          className="cart-remove-btn"
          onClick={() => onRemove(item.variantId)}
          disabled={cartState.isLoading}
        >
          Удалить
        </button>
      </div>
    );
  };

  if (cartState.isLoading) {
    return (
      <div className="cart-loading">
        <div className="loading-spinner"></div>
        <p>Загрузка корзины...</p>
      </div>
    );
  }

  if (cartState.error) {
    return (
      <div className="cart-error">
        <h3>Произошла ошибка</h3>
        <p>{cartState.error}</p>
        <button 
          className="retry-btn"
          onClick={fetchCartItems}
        >
          Повторить попытку
        </button>
      </div>
    );
  }

  if (cartState.isEmpty) {
    return (
      <div className="cart-empty">
        <div className="empty-icon">🛒</div>
        <h3>Ваша корзина пуста</h3>
        <button 
          className="catalog-btn"
          onClick={() => navigate('/catalog')}
        >
          Перейти в каталог
        </button>
      </div>
    );
  }

  return (
    <div className="cart-page">
      <div className="cart-header">
        <h1>Корзина</h1>
        <span className="items-count">{cartState.items.length} товаров</span>
      </div>
      
      <div className="cart-content">
        <div className="cart-items-list">
          {cartState.items.map(item => (
            <CartItem
              key={item.variantId}
              item={item}
              onRemove={removeItem}
            />
          ))}
        </div>

        <div className="cart-summary">
          <h2>Итого</h2>
          <div className="summary-row">
            <span>Товары ({cartState.items.length})</span>
            <span>{formatPrice(calculateTotal())}</span>
          </div>
          <div className="summary-row">
            <span>Доставка</span>
            <span>Бесплатно</span>
          </div>
          <div className="summary-total">
            <span>Общая сумма</span>
            <span>{formatPrice(calculateTotal())}</span>
          </div>
          <button
            className="checkout-btn"
            onClick={() => navigate('/checkout')}
          >
            Оформить заказ
          </button>
        </div>
      </div>
    </div>
  );
}

export default Cart;
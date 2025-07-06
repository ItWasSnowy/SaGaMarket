import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Cart.css';

const API_BASE = 'https://localhost:7182/api';

function Cart({ setCartItemsCount }) {
  const [state, setState] = useState({
    items: [],
    isLoading: true,
    error: null,
    isProcessing: false
  });

  const navigate = useNavigate();

  const fetchCartItems = async () => {
    try {
      setState(prev => ({ ...prev, isLoading: true, error: null }));

      // 1. Получаем ID товаров в корзине
      const itemsRes = await fetch(`${API_BASE}/cart/items`, {
        credentials: 'include'
      });

      if (itemsRes.status === 401) {
        localStorage.removeItem('userData');
        navigate('/login');
        return;
      }

      if (!itemsRes.ok) {
        throw new Error(`Ошибка загрузки корзины: ${itemsRes.status}`);
      }

      const variantIds = await itemsRes.json();
      
      if (!variantIds?.length) {
        setState({ items: [], isLoading: false, error: null });
        setCartItemsCount(0);
        return;
      }

      // 2. Получаем полную информацию о товарах
      const queryParams = variantIds.map(id => `variantIds=${id}`).join('&');
      const infoRes = await fetch(`${API_BASE}/cart/info?${queryParams}`, {
        credentials: 'include'
      });

      if (!infoRes.ok) {
        throw new Error(`Ошибка загрузки информации: ${infoRes.status}`);
      }

      const itemsData = await infoRes.json();
      setState({ items: itemsData, isLoading: false, error: null });
      setCartItemsCount(itemsData.length);

    } catch (error) {
      console.error('Ошибка:', error);
      setState({
        items: [],
        isLoading: false,
        error: error.message
      });
      setCartItemsCount(0);
    }
  };

  const handleCheckout = async () => {
    try {
      setState(prev => ({ ...prev, isProcessing: true }));
      
      const orderData = {
        items: state.items.map(item => ({
          productId: item.productId,
          variantId: item.variantId,
          quantity: item.quantity || 1,
          price: item.price
        })),
        shippingAddress: "Адрес доставки",
        billingAddress: "Адрес оплаты",
        paymentMethod: "Карта",
        notes: "Комментарий к заказу"
      };

      const res = await fetch(`${API_BASE}/Order`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(orderData),
        credentials: 'include'
      });

      if (!res.ok) {
        const errorData = await res.json();
        throw new Error(errorData.message || 'Ошибка оформления заказа');
      }

      const order = await res.json();
      
      // Очистка корзины
      await fetch(`${API_BASE}/cart/clear`, {
        method: 'POST',
        credentials: 'include'
      });

      // Перенаправление с передачей ID нового заказа
      navigate('/my-orders', { 
        state: { 
          newOrderId: order.orderId,
          message: 'Заказ успешно создан!'
        }
      });

    } catch (error) {
      console.error('Ошибка оформления:', error);
      setState(prev => ({ ...prev, error: error.message }));
    } finally {
      setState(prev => ({ ...prev, isProcessing: false }));
    }
  };

  const removeItem = async (variantId) => {
    try {
      setState(prev => ({ ...prev, isLoading: true }));
      
      const res = await fetch(`${API_BASE}/cart/remove`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ variantId }),
        credentials: 'include'
      });

      if (!res.ok) {
        throw new Error('Не удалось удалить товар');
      }

      await fetchCartItems();
    } catch (error) {
      console.error('Ошибка удаления:', error);
      setState(prev => ({ ...prev, error: error.message }));
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
    return state.items.reduce((total, item) => 
      total + (item.price * (item.quantity || 1)), 0);
  };

  if (state.isLoading) {
    return (
      <div className="cart-loading">
        <div className="spinner"></div>
        <p>Загрузка корзины...</p>
      </div>
    );
  }

  if (state.error) {
    return (
      <div className="cart-error">
        <h3>Произошла ошибка</h3>
        <p>{state.error}</p>
        <button 
          className="retry-btn"
          onClick={fetchCartItems}
        >
          Повторить попытку
        </button>
      </div>
    );
  }

  if (state.items.length === 0) {
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
        <span className="items-count">{state.items.length} товаров</span>
      </div>
      
      <div className="cart-content">
        <div className="cart-items-list">
          {state.items.map(item => (
            <div key={`${item.productId}-${item.variantId}`} className="cart-item">
              <div className="cart-item-image">
                <img
                  src={'Нет изображения'}
                  alt={item.productName}
                  onError={(e) => {
                  }}
                />
              </div>
              
              <div className="cart-item-details">
                <h3 className="cart-item-title">{item.productName}</h3>
                {item.variantName && (
                  <p className="cart-item-variant">Вариант: {item.variantName}</p>
                )}
                <p className="cart-item-price">{formatPrice(item.price)}</p>
                <div className="cart-item-quantity">
                  <span>Количество: {item.quantity || 1}</span>
                </div>
              </div>
              
              <button
                className="cart-remove-btn"
                onClick={() => removeItem(item.variantId)}
                disabled={state.isProcessing}
              >
                Удалить
              </button>
            </div>
          ))}
        </div>

        <div className="cart-summary">
          <h2>Итого</h2>
          <div className="summary-row">
            <span>Товары ({state.items.length})</span>
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
            onClick={handleCheckout}
            disabled={state.isProcessing}
          >
            {state.isProcessing ? (
              <>
                <span className="spinner"></span>
                Оформление заказа...
              </>
            ) : (
              'Оформить заказ'
            )}
          </button>
        </div>
      </div>
    </div>
  );
}

export default Cart;
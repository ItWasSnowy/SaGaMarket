import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import './OrdersPage.css';

const API_BASE = 'https://localhost:7182/api';

function OrdersPage() {
  const [state, setState] = useState({
    orders: [],
    isLoading: true,
    error: null
  });

  const location = useLocation();
  const navigate = useNavigate();

  const fetchOrders = async () => {
    try {
      setState(prev => ({ ...prev, isLoading: true, error: null }));
      
      const res = await fetch(`${API_BASE}/Order/my-orders`, {
        credentials: 'include'
      });

      if (res.status === 401) {
        localStorage.removeItem('userData');
        navigate('/login');
        return;
      }

      if (!res.ok) {
        throw new Error(`Ошибка загрузки заказов: ${res.status}`);
      }

      const orders = await res.json();
      
      // Добавляем статус по умолчанию, если он не пришел с сервера
      const normalizedOrders = orders.map(order => ({
        ...order,
        status: order.status || 'В обработке'
      }));
      
      setState({ orders: normalizedOrders, isLoading: false, error: null });

      if (location.state?.newOrderId) {
        setTimeout(() => {
          const element = document.getElementById(`order-${location.state.newOrderId}`);
          if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            element.classList.add('highlight');
          }
        }, 300);
      }

    } catch (error) {
      console.error('Ошибка:', error);
      setState({
        orders: [],
        isLoading: false,
        error: error.message
      });
    }
  };

  useEffect(() => {
    fetchOrders();
  }, [location.state]);

  const formatDate = (dateString) => {
    const options = { 
      day: 'numeric', 
      month: 'long', 
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    };
    return new Date(dateString).toLocaleDateString('ru-RU', options);
  };

  const formatPrice = (price) => {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
      minimumFractionDigits: 0
    }).format(price);
  };

  if (state.isLoading) {
    return (
      <div className="orders-loading">
        <div className="spinner"></div>
        <p>Загрузка ваших заказов...</p>
      </div>
    );
  }

  if (state.error) {
    return (
      <div className="orders-error">
        <h3>Произошла ошибка</h3>
        <p>{state.error}</p>
        <button 
          className="retry-btn"
          onClick={fetchOrders}
        >
          Повторить попытку
        </button>
      </div>
    );
  }

  if (state.orders.length === 0) {
    return (
      <div className="no-orders">
        <div className="empty-icon">📦</div>
        <h3>У вас пока нет заказов</h3>
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
    <div className="orders-page">
      <div className="orders-header">
        <h1>Мои заказы</h1>
        {location.state?.message && (
          <div className="success-message">
            {location.state.message}
          </div>
        )}
      </div>

      <div className="orders-list">
        {state.orders.map(order => {
          // Безопасное получение статуса
          const status = order.status || 'В обработке';
          const statusClass = status.toLowerCase().replace(/\s+/g, '-');
          
          return (
            <div 
              key={order.orderId}
              id={`order-${order.orderId}`}
              className={`order-card ${statusClass}`}
            >
              <div className="order-header">
                <h3>Заказ #{order.orderNumber || order.orderId.split('-')[0]}</h3>
                <span className="order-date">
                  {order.createdAt ? formatDate(order.createdAt) : 'Дата не указана'}
                </span>
              </div>

              <div className="order-status">
                Статус: <span className={`status-badge ${statusClass}`}>
                  {status}
                </span>
              </div>

              <div className="order-items-summary">
                <h4>Товары:</h4>
                <ul>
                  {order.items?.slice(0, 3).map(item => (
                    <li key={item.orderItemId || `${order.orderId}-${item.productId}`}>
                      {item.productName || 'Товар'} × {item.quantity || 1}
                    </li>
                  ))}
                  {order.items?.length > 3 && (
                    <li>и ещё {order.items.length - 3} товаров...</li>
                  )}
                </ul>
              </div>

              <div className="order-footer">
                <div className="order-total">
                  Сумма: {order.totalPrice ? formatPrice(order.totalPrice) : '0 ₽'}
                </div>
                <button
                  className="details-btn"
                  onClick={() => navigate(`/orders/${order.orderId}`)}
                >
                  Подробнее
                </button>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default OrdersPage;
import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Link, useNavigate } from 'react-router-dom';
import './App.css';
import Home from './Pages/Home';
import Catalog from './Pages/Catalog';
import ProductPage from './Pages/ProductPage';
import Cart from './Pages/Cart';
import Favorites from './Pages/Favorites'; // Импортируем компонент избранного

function App() {
  const [cartItemsCount, setCartItemsCount] = useState(0);
  const [favoritesCount, setFavoritesCount] = useState(0); // Добавляем состояние для счетчика избранного
  const navigate = useNavigate();

  return (
    <div className="App">
      {/* Шапка сайта (общая для всех страниц) */}
      <header className="site-header">
        <div className="header-content">
          <h1 className="logo">ShopName</h1>
          <nav className="main-nav">
            <ul>
              <li><Link to="/">Главная</Link></li>
              <li><Link to="/catalog">Каталог</Link></li>
              <li><Link to="/cart">Моя корзина</Link></li>
              <li><Link to="/favorites">Избранное</Link></li> {/* Добавляем ссылку в навигацию */}
              <li><Link to="/about">О нас</Link></li>
              <li><Link to="/contacts">Контакты</Link></li>
            </ul>
          </nav>
          <div className="user-actions">
            <button className="login-btn">Войти</button>
            <div className="action-buttons">
              <button 
                className="favorites-btn"
                onClick={() => navigate('/favorites')}
                title="Избранное"
              >
                ♡ {favoritesCount > 0 && <span className="count">{favoritesCount}</span>}
              </button>
              <button 
                className="cart-btn"
                onClick={() => navigate('/cart')}
                title="Корзина"
              >
                🛒 {cartItemsCount > 0 && <span className="count">{cartItemsCount}</span>}
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Основное содержимое (меняется в зависимости от страницы) */}
      <main>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/catalog" element={<Catalog />} />
          <Route path="/product/:productId" element={<ProductPage />} />
          <Route path="/cart" element={<Cart setCartItemsCount={setCartItemsCount} />} />
          <Route path="/favorites" element={<Favorites setFavoritesCount={setFavoritesCount} />} /> {/* Добавляем маршрут для избранного */}
          {/* Заглушки для других страниц */}
          <Route path="/about" element={
            <section className="page-content">
              <h2>О нашей компании</h2>
              <p>Информация о нас...</p>
            </section>
          } />
          <Route path="/contacts" element={
            <section className="page-content">
              <h2>Наши контакты</h2>
              <p>Телефон: +7 (123) 456-78-90</p>
              <p>Email: info@shopname.com</p>
            </section>
          } />
        </Routes>
      </main>

      {/* Подвал сайта (общий для всех страниц) */}
      <footer className="site-footer">
        <div className="footer-content">
          <div className="footer-section">
            <h3>О компании</h3>
            <p>Мы работаем с 2010 года</p>
          </div>
          <div className="footer-section">
            <h3>Контакты</h3>
            <p>Email: info@shopname.com</p>
            <p>Телефон: +7 (123) 456-78-90</p>
          </div>
        </div>
        <div className="copyright">
          <p>&copy; 2023 ShopName. Все права защищены.</p>
        </div>
      </footer>
    </div>
  );
}

function AppWrapper() {
  return (
    <Router>
      <App />
    </Router>
  );
}

export default AppWrapper;
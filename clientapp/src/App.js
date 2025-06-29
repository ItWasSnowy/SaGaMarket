import React from 'react';
import './App.css';

function App() {
  return (
    <div className="App">
      {}
      <header className="site-header">
        <div className="header-content">
          <h1 className="logo">ShopName</h1>
          <nav className="main-nav">
            <ul>
              <li><a href="/">Главная</a></li>
              <li><a href="/catalog">Каталог</a></li>
              <li><a href="/about">О нас</a></li>
              <li><a href="/contacts">Контакты</a></li>
            </ul>
          </nav>
          <div className="user-actions">
            <button className="login-btn">Войти</button>
            <button className="cart-btn">Корзина (0)</button>
          </div>
        </div>
      </header>

      {}
      <section className="hero-banner">
        <div className="banner-content">
          <h2>Добро пожаловать в наш магазин</h2>
          <p>Лучшие товары по выгодным ценам</p>
          <button className="cta-button">Перейти в каталог</button>
        </div>
      </section>

      {}
      <section className="info-blocks">
        <div className="info-card">
          <h3>Быстрая доставка</h3>
          <p>Доставка по всей стране за 1-3 дня</p>
        </div>
        <div className="info-card">
          <h3>Гарантия качества</h3>
          <p>Все товары проходят тщательную проверку</p>
        </div>
        <div className="info-card">
          <h3>Поддержка 24/7</h3>
          <p>Наши операторы всегда на связи</p>
        </div>
      </section>

      {}
      <section className="newsletter">
        <h2>Подпишитесь на наши новости</h2>
        <form>
          <input type="email" placeholder="Ваш email" />
          <button type="submit">Подписаться</button>
        </form>
      </section>

      {}
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

export default App;

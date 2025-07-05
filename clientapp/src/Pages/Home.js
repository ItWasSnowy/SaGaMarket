import React from 'react';
import { Link } from 'react-router-dom';
import './Home.css';

function Home() {
  return (
    <div className="home-page">
      <section className="hero-section">
        <div className="hero-content">
          <h1 className="hero-title">Добро пожаловать в наш магазин</h1>
          <p className="hero-subtitle">Откройте для себя лучшие товары по выгодным ценам</p>
          <Link to="/catalog" className="hero-button">
            Перейти в каталог
          </Link>
        </div>
      </section>

      <section className="features-section">
        <h2 className="section-title">Почему выбирают нас</h2>
        <div className="features-grid">
          <div className="feature-card">
            <div className="feature-icon">🚚</div>
            <h3>Быстрая доставка</h3>
            <p>Доставка по всей стране за 1-3 рабочих дня</p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">🔍</div>
            <h3>Гарантия качества</h3>
            <p>Все товары проходят тщательную проверку перед отправкой</p>
          </div>
          <div className="feature-card">
            <div className="feature-icon">📞</div>
            <h3>Поддержка 24/7</h3>
            <p>Наши операторы всегда готовы помочь</p>
          </div>
        </div>
      </section>

      <section className="newsletter-section">
        <div className="newsletter-container">
          <h2>Будьте в курсе новинок</h2>
          <p>Подпишитесь на рассылку и получайте специальные предложения</p>
          <form className="newsletter-form">
            <input 
              type="email" 
              placeholder="Ваш email" 
              className="newsletter-input"
              required
            />
            <button type="submit" className="newsletter-button">
              Подписаться
            </button>
          </form>
        </div>
      </section>
    </div>
  );
}

export default Home;
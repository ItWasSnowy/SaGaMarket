import React from 'react';
import { Link } from 'react-router-dom';

function Home() {
  return (
    <>
      <section className="hero-banner">
        <div className="banner-content">
          <h2>Добро пожаловать в наш магазин</h2>
          <p>Лучшие товары по выгодным ценам</p>
          <Link to="/catalog" className="cta-button">Перейти в каталог</Link>
        </div>
      </section>

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

      <section className="newsletter">
        <h2>Подпишитесь на наши новости</h2>
        <form>
          <input type="email" placeholder="Ваш email" />
          <button type="submit">Подписаться</button>
        </form>
      </section>
    </>
  );
}

export default Home;
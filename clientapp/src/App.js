import React, { useState } from 'react';
import { BrowserRouter, Routes, Route, Link, useNavigate } from 'react-router-dom';
import './App.css';
import Home from './Pages/Home';
import Catalog from './Pages/Catalog';
import ProductPage from './Pages/ProductPage';
import Cart from './Pages/Cart';
import Favorites from './Pages/Favorites';
import AuthPage from './Pages/AuthPage';
import ProfilePage from './Pages/ProfilePage';

function AppContent() {
  const [cartItemsCount, setCartItemsCount] = useState(0);
  const [favoritesCount, setFavoritesCount] = useState(0);
  const navigate = useNavigate();

  const hasUserData = Boolean(localStorage.getItem('userData'));

  const handleLogout = () => {
    localStorage.removeItem('userData');
    navigate('/login');
  };

  return (
    <div className="App">
      <header className="site-header">
        <div className="header-content">
          <h1 className="logo" onClick={() => navigate('/')}>ShopName</h1>
          
          <nav className="main-nav">
            <ul>
              <li><Link to="/">Главная</Link></li>
              <li><Link to="/catalog">Каталог</Link></li>
              <li>
                <Link to="/cart">
                  Корзина {cartItemsCount > 0 && `(${cartItemsCount})`}
                </Link>
              </li>
              <li>
                <Link to="/favorites">
                  Избранное {favoritesCount > 0 && `(${favoritesCount})`}
                </Link>
              </li>
            </ul>
          </nav>

          <div className="user-actions">
            {hasUserData ? (
              <>
                <button className="profile-btn" onClick={() => navigate('/profile')}>
                  Мой профиль
                </button>
                <button className="logout-btn" onClick={handleLogout}>
                  Выйти
                </button>
              </>
            ) : (
              <>
                <button className="login-btn" onClick={() => navigate('/login')}>
                  Войти
                </button>
                <button className="register-btn" onClick={() => navigate('/register')}>
                  Регистрация
                </button>
              </>
            )}
          </div>
        </div>
      </header>

      <main>
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/catalog" element={<Catalog />} />
          <Route 
            path="/product/:productId" 
            element={
              <ProductPage 
                setCartItemsCount={setCartItemsCount}
                setFavoritesCount={setFavoritesCount}
              />
            } 
          />
          <Route path="/cart" element={<Cart setCartItemsCount={setCartItemsCount} />} />
          <Route path="/favorites" element={<Favorites setFavoritesCount={setFavoritesCount} />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/login" element={<AuthPage isLogin={true} />} />
          <Route path="/register" element={<AuthPage isLogin={false} />} />
        </Routes>
      </main>

      <footer className="site-footer">
        <div className="footer-content">
          <p>&copy; {new Date().getFullYear()} ShopName</p>
        </div>
      </footer>
    </div>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AppContent />
    </BrowserRouter>
  );
}

export default App;
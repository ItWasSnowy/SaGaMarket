import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../authContext';
import './NavBar.css';

const NavBar = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/');
  };

  return (
    <nav className="site-header">
      <div className="header-content">
        <h1 className="logo">
          <Link to="/">ShopName</Link>
        </h1>
        
        <div className="main-nav">
          <Link to="/">Главная</Link>
          <Link to="/catalog">Каталог</Link>
          <Link to="/cart">Корзина</Link>
          <Link to="/favorites">Избранное</Link>
          {user && <Link to="/orders">Мои заказы</Link>}
        </div>

        <div className="user-actions">
          {user ? (
            <>
              <Link to="/profile" className="btn profile-btn">Мой профиль</Link>
              <button onClick={handleLogout} className="btn logout-btn">Выйти</button>
            </>
          ) : (
            <>
              <Link to="/login" className="btn login-btn">Войти</Link>
              <Link to="/register" className="btn register-btn">Регистрация</Link>
            </>
          )}
        </div>
      </div>
    </nav>
  );
};

export default NavBar;
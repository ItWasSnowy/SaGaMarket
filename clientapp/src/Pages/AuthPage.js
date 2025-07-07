import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../authContext';
import './AuthPage.css';
import axios from 'axios';
const AuthPage = ({ isLogin }) => {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    name: ''
  });
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { login, checkAuth } = useAuth();

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
  e.preventDefault();
  setError('');
  setIsLoading(true);

  try {
    if (isLogin) {
      const result = await login({
        email: formData.email,
        password: formData.password
      });
      
      if (!result.success) {
        throw new Error(result.message);
      }
    } else {
      // Регистрация
      await axios.post('https://localhost:7182/api/Account/register', formData, {
        withCredentials: true
      });
      
      // Автовход после регистрации
      const loginResult = await login({
        email: formData.email,
        password: formData.password
      });
      
      if (!loginResult.success) {
        throw new Error(loginResult.message);
      }
    }
  } catch (err) {
    setError(err.message);
    console.error('Auth error:', err);
  } finally {
    setIsLoading(false);
  }
};

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h2 className="auth-title">{isLogin ? 'Вход в систему' : 'Регистрация'}</h2>
        
        {error && <div className="auth-error">{error}</div>}
        
        <form onSubmit={handleSubmit} className="auth-form">
          {!isLogin && (
            <div className="form-group">
              <label htmlFor="name" className="form-label">Имя пользователя</label>
              <input
                id="name"
                type="text"
                name="name"
                value={formData.name}
                onChange={handleChange}
                className="form-input"
                required
                minLength={3}
              />
            </div>
          )}
          
          <div className="form-group">
            <label htmlFor="email" className="form-label">Email</label>
            <input
              id="email"
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              className="form-input"
              required
            />
          </div>
          
          <div className="form-group">
            <label htmlFor="password" className="form-label">Пароль</label>
            <input
              id="password"
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              className="form-input"
              required
              minLength={6}
            />
          </div>
          
          <button 
            type="submit" 
            className="auth-button"
            disabled={isLoading}
          >
            {isLoading ? (
              <span className="auth-spinner"></span>
            ) : isLogin ? 'Войти' : 'Зарегистрироваться'}
          </button>
        </form>
        
        <div className="auth-switch">
          {isLogin ? (
            <p>
              Нет аккаунта?{' '}
              <button 
                type="button" 
                className="auth-link"
                onClick={() => navigate('/register')}
              >
                Зарегистрируйтесь
              </button>
            </p>
          ) : (
            <p>
              Уже есть аккаунт?{' '}
              <button 
                type="button" 
                className="auth-link"
                onClick={() => navigate('/login')}
              >
                Войдите
              </button>
            </p>
          )}
        </div>
      </div>
    </div>
  );
};

export default AuthPage;
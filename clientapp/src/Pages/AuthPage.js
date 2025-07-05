import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

const AuthPage = ({ isLogin }) => {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    name: ''
  });
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    try {
      const url = isLogin 
        ? 'https://localhost:7182/api/Account/login' 
        : 'https://localhost:7182/api/Account/register';
      
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData)
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || 'Ошибка запроса');
      }

      // Сохраняем данные пользователя
      localStorage.setItem('userData', JSON.stringify(data));
      navigate('/profile');
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="auth-container">
      <h2>{isLogin ? 'Вход' : 'Регистрация'}</h2>
      {error && <div className="error-message">{error}</div>}
      
      <form onSubmit={handleSubmit}>
        {!isLogin && (
          <div className="form-group">
            <label>Имя:</label>
            <input
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              required
            />
          </div>
        )}
        
        <div className="form-group">
          <label>Email:</label>
          <input
            type="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            required
          />
        </div>
        
        <div className="form-group">
          <label>Пароль:</label>
          <input
            type="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
            required
          />
        </div>
        
        <button type="submit" className="submit-btn">
          {isLogin ? 'Войти' : 'Зарегистрироваться'}
        </button>
      </form>
      
      <div className="auth-switch">
        {isLogin ? (
          <p>Нет аккаунта? <span onClick={() => navigate('/register')}>Зарегистрируйтесь</span></p>
        ) : (
          <p>Уже есть аккаунт? <span onClick={() => navigate('/login')}>Войдите</span></p>
        )}
      </div>
    </div>
  );
};

export default AuthPage;
import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import './ProfilePage.css';

axios.defaults.withCredentials = true;

const ProfilePage = () => {
  const [userData, setUserData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        console.log('Пытаюсь получить профиль... Текущие куки:', document.cookie);
        
        const response = await axios.get('https://localhost:7182/api/Account', {
          withCredentials: true
        });

        if (!response.data?.userId) {
          throw new Error('Не получили данные пользователя');
        }
        
        setUserData(response.data);
      } catch (err) {
        console.error('Ошибка получения профиля:', err);
        navigate('/login');
      } finally {
        setLoading(false);
      }
    };

    fetchProfile();
  }, [navigate]);




  const handleLogout = async () => {
    try {
      await axios.post('https://localhost:7182/api/Account/logout');
    } catch (err) {
      console.error('Ошибка при выходе:', err);
    } finally {
      navigate('/login');
    }
  };

  const handleViewProducts = () => {
    navigate(`/my-products/${userData.userId}`);
  };

  const handleCreateProduct = () => {
     console.log('Кнопка "Создать товар" нажата');
    navigate('/create-product'); // Это перенаправление на страницу создания товара
  };

  if (loading) return <div className="loading">Загрузка профиля...</div>;
  if (error) return <div className="error">Ошибка: {error}</div>;
  if (!userData) return <div>Данные профиля не найдены</div>;

  // Определяем роль пользователя
  const roleMap = {
    0: 'customer',
    1: 'seller',
    3: 'admin'
  };

  const userRole = roleMap[userData.role] || 'unknown'; // Получаем строковое представление роли

  return (
    <div className="profile">
      <div className="profile-header">
        {userData.profilePhotoUrl ? (
          <img 
            src={userData.profilePhotoUrl} 
            alt="Profile" 
            className="profile-photo"
          />
        ) : (
          <div className="no-photo">Фото отсутствует</div>
        )}
        <h2>{userData.username || 'Пользователь'}</h2>
        <p className="member-since">
          Участник с: {new Date(userData.createdAt).toLocaleDateString()}
        </p>
      </div>

      <div className="stats">
        <div className="stat">
          <h3>Роль</h3>
          <p>{userRole}</p>
        </div>
        <div className="stat">
          <h3>Товары</h3>
          <p>{userData.productsForSaleCount}</p>
        </div>
        <div className="stat">
          <h3>Заказы</h3>
          <p>{userData.orderCount}</p>
        </div>
      </div>

      <div className="details">
        <h3>Контактная информация</h3>
        <p><strong>Email:</strong> {userData.email}</p>
        <p><strong>ID:</strong> {userData.userId}</p>
      </div>

      {userRole === 'seller' && (
        <div className="actions">
          <button 
            onClick={handleViewProducts}
            className={`btn`}
          >
            Мои товары 
          </button>
          <button 
            onClick={handleCreateProduct}
            className="btn create"
          >
            Создать товар
          </button>
        </div>
      )}

      <button 
        onClick={handleLogout}
        className="btn logout"
      >
        Выйти
      </button>
    </div>
  );
};

export default ProfilePage;

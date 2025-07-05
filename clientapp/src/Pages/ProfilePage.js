import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

const ProfilePage = () => {
  const [userData, setUserData] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const data = localStorage.getItem('userData');
    if (!data) {
      navigate('/login');
      return;
    }
    setUserData(JSON.parse(data));
  }, [navigate]);

  const handleLogout = () => {
    localStorage.removeItem('userData');
    navigate('/login');
  };

  if (!userData) return <div>Загрузка...</div>;

  return (
    <div className="profile-container">
      <h2>Мой профиль</h2>
      
      <div className="profile-info">
        {userData.userName && (
          <p><strong>Имя:</strong> {userData.userName}</p>
        )}
        <p><strong>Email:</strong> {userData.email}</p>
        {userData.role && (
          <p><strong>Роль:</strong> {userData.role}</p>
        )}
        {userData.userId && (
          <p><strong>ID:</strong> {userData.userId}</p>
        )}
      </div>
      
      <button 
        onClick={handleLogout}
        className="logout-btn"
      >
        Выйти
      </button>
    </div>
  );
};

export default ProfilePage;
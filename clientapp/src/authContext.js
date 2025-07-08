import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  const checkAuth = useCallback(async () => {
    try {
      const response = await axios.get('https://localhost:7182/api/Account', {
        withCredentials: true
      });
      setUser(response.data);
      return response.data;
    } catch (error) {
      setUser(null);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const login = useCallback(async (credentials) => {
  try {
    const response = await axios.post('https://localhost:7182/api/Account/login', credentials, {
      withCredentials: true
    });
    
    // Добавьте эту проверку
    const userData = await checkAuth();
    if (!userData) throw new Error('Не удалось получить данные пользователя');
    
    setUser(userData); // Важно сохранить ВСЕ данные пользователя
    navigate('/');
    return { success: true, user: userData };
  } catch (error) {
    console.error('Login error:', error);
    return { 
      success: false, 
      message: error.response?.data?.message || 'Ошибка входа' 
    };
  }
}, [checkAuth, navigate]);

  const logout = useCallback(async () => {
    try {
      await axios.post('https://localhost:7182/api/Account/logout', {}, {
        withCredentials: true
      });
      setUser(null);
      navigate('/login');
      return true;
    } catch (error) {
      console.error('Ошибка выхода:', error);
      return false;
    }
  }, [navigate]);

  useEffect(() => {
    checkAuth();
  }, [checkAuth]);

  return (
    <AuthContext.Provider value={{ 
      user, 
      loading, 
      login, 
      logout, 
      checkAuth 
    }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
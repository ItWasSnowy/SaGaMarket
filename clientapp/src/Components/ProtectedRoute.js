import React from 'react';
import { useAuth } from '../authContext';
import { Navigate } from 'react-router-dom';

const ProtectedRoute = ({ children, allowedRoles }) => {
  const { user, loading } = useAuth();

  // Отладочная информация
  console.group('ProtectedRoute Debug');
  console.log('User:', user);
  console.log('Allowed roles:', allowedRoles);
  console.groupEnd();

  if (loading) {
    return <div>Загрузка...</div>;
  }

  if (!user) {
    console.log('Redirecting to login: no user');
    return <Navigate to="/login" replace />;
  }

  // Проверяем числовые роли (1 для seller, 3 для admin)
  if (allowedRoles && !allowedRoles.includes(user.role)) {
    console.warn(`Access denied. User role: ${user.role}, Required: ${allowedRoles}`);
    return <Navigate to="/" replace />;
  }

  return children;
};

export default ProtectedRoute;
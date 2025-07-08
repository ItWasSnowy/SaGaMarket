import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './authContext';
import NavBar from './Components/NavBar'; 
import Home from './Pages/Home';
import Catalog from './Pages/Catalog';
import ProductPage from './Pages/ProductPage';
import Cart from './Pages/Cart';
import Favorites from './Pages/Favorites';
import ProfilePage from './Pages/ProfilePage';
import OrdersPage from './Pages/OrdersPage';
import AuthPage from './Pages/AuthPage';
import CreateProduct from './Pages/CreateProduct';
import ProtectedRoute from './Components/ProtectedRoute'
import MyProductsPage from './Pages/MyProductsPage';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <NavBar />
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/catalog" element={<Catalog />} />
          <Route path="/product/:productId" element={<ProductPage />} />
          <Route path="/cart" element={<Cart />} />
          
          <Route path="/favorites" element={
            <ProtectedRoute>
              <Favorites />
            </ProtectedRoute>
          } />
          
          <Route path="/profile" element={
            <ProtectedRoute>
              <ProfilePage />
            </ProtectedRoute>
          } />
          
          <Route path="/orders" element={
            <ProtectedRoute>
              <OrdersPage />
            </ProtectedRoute>
          } />
          
          <Route path="/login" element={<AuthPage isLogin={true} />} />
          <Route path="/register" element={<AuthPage isLogin={false} />} />
          <Route path="/create-product" element={
  <ProtectedRoute allowedRoles={[1]}> {/* 1 = seller */}
    <CreateProduct />
  </ProtectedRoute>
} />
<Route 
  path="/my-products/:sellerId" 
  element={
    <ProtectedRoute allowedRoles={[1]}>
      <MyProductsPage />
    </ProtectedRoute>
  } 
/>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
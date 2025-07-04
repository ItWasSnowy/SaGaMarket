import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Cart.css';

function Cart() {
  const [userId, setUserId] = useState('');
  const [cartItems, setCartItems] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const savedUserId = localStorage.getItem('userId');
    if (savedUserId) {
      setUserId(savedUserId);
      fetchUserCart(savedUserId);
    }
  }, []);

  const fetchProductDetails = async (variantIds) => {
    try {
      const response = await fetch(`https://localhost:7182/api/cart/info?variantIds=${variantIds.join('&variantIds=')}`);
      if (!response.ok) throw new Error('Failed to fetch product details');
      return await response.json();
    } catch (err) {
      console.error('Error fetching product details:', err);
      throw err;
    }
  };

  const fetchUserCart = async (userId) => {
    try {
      setIsLoading(true);
      const response = await fetch(`https://localhost:7182/api/cart/items?userId=${userId}`);
      
      if (!response.ok) throw new Error('Failed to fetch cart');
      
      const variantIds = await response.json();
      console.log('Received variant IDs:', variantIds);
      
      if (!Array.isArray(variantIds) ){
        throw new Error('Expected array of variant IDs');
      }

      if (variantIds.length === 0) {
        setCartItems([]);
        return;
      }

      const productsData = await fetchProductDetails(variantIds);
      console.log('Received product details:', productsData);
      
      setCartItems(productsData || []);
    } catch (err) {
      console.error('Error fetching cart:', err);
      setError(err.message);
    } finally {
      setIsLoading(false);
    }
  };

  const removeFromCart = async (variantId) => {
    try {
      const response = await fetch(`https://localhost:7182/api/cart/remove?userId=${userId}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ variantId })
      });

      if (!response.ok) throw new Error('Failed to remove item');
      fetchUserCart(userId); // Обновляем данные после удаления
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <div className="cart-page">
      <h1>My Shopping Cart</h1>
      
      <div className="user-id-section">
        <form onSubmit={(e) => {
          e.preventDefault();
          if (!userId) {
            setError('Please enter user ID');
            return;
          }
          localStorage.setItem('userId', userId);
          fetchUserCart(userId);
        }}>
          <label>
            User ID:
            <input
              type="text"
              value={userId}
              onChange={(e) => setUserId(e.target.value)}
              placeholder="Enter your user ID"
            />
          </label>
          <button type="submit">Load Cart</button>
        </form>
      </div>

      {error && <div className="error-message">{error}</div>}

      {isLoading ? (
        <div className="loading">Loading cart items...</div>
      ) : cartItems.length > 0 ? (
        <>
          <div className="cart-items-container">
            {cartItems.map((item) => (
              <div key={item.variantId} className="cart-item">
                <div className="item-image">
                  <img 
                    src={item.imageUrl || '/images/default-product.png'} 
                    alt={item.variantName || 'Product'} 
                  />
                </div>
                
                <div className="item-details">
                  <h3>{item.productName}</h3>
                  <p className="variant-name">{item.variantName}</p>
                  <p className="seller">Sold by: {item.sellerName}</p>
                  <p className="price">Price: ${(item.price || 0).toFixed(2)}</p>
                  <p className="stock">Available: {item.availableCount}</p>
                </div>
                
                <div className="item-actions">
                  <button 
                    className="remove-btn"
                    onClick={() => removeFromCart(item.variantId)}
                  >
                    Remove
                  </button>
                </div>
              </div>
            ))}
          </div>
          
          <div className="cart-summary">
            <h3>Order Summary</h3>
            <div className="summary-row">
              <span>Subtotal:</span>
              <span>${cartItems.reduce((sum, item) => sum + (item.price || 0), 0).toFixed(2)}</span>
            </div>
            <div className="summary-row">
              <span>Shipping:</span>
              <span>Free</span>
            </div>
            <div className="summary-row total">
              <span>Total:</span>
              <span>${cartItems.reduce((sum, item) => sum + (item.price || 0), 0).toFixed(2)}</span>
            </div>
            <button className="checkout-btn">Proceed to Checkout</button>
          </div>
        </>
      ) : (
        !isLoading && <div className="empty-cart">Your cart is empty</div>
      )}
    </div>
  );
}

export default Cart;
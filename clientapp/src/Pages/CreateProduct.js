import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import './CreateProduct.css';

axios.defaults.withCredentials = true;

const CreateProduct = () => {
  const navigate = useNavigate();
  const [step, setStep] = useState(1); // 1 - создание товара, 2 - добавление вариантов
  const [productData, setProductData] = useState({
    category: '',
    name: ''
  });
  const [variants, setVariants] = useState([{
    name: '',
    price: 0,
    count: 0
  }]);
  const [productId, setProductId] = useState(null);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Шаг 1: Создание основного товара
  const handleCreateBaseProduct = async (e) => {
    e.preventDefault();
    
    if (!productData.category || !productData.name) {
      setError('Заполните все обязательные поля');
      return;
    }

    try {
      const response = await axios.post('https://localhost:7182/api/Product', {
        category: productData.category,
        name: productData.name
      });
      
      setProductId(response.data.productId);
      setStep(2); // Переходим к добавлению вариантов
      setSuccess('Основной товар успешно создан!');
      setError('');
    } catch (err) {
      console.error('Ошибка создания товара:', err);
      setError(err.response?.data?.message || 'Ошибка при создании товара');
    }
  };

  // Шаг 2: Добавление вариантов
  const handleAddVariants = async (e) => {
  e.preventDefault();
  
  try {
    // Подготовка всех запросов на создание вариантов
    const variantRequests = variants.map(variant => 
      axios.post('https://localhost:7182/api/Variant', {
        productId: productId, // Обязательное поле
        name: variant.name,
        description: variant.description || "Без описания", // Дефолтное значение
        price: variant.price,
        count: variant.count
      }, {
        headers: {
          'Content-Type': 'application/json'
        },
        withCredentials: true
      })
    );

    // Отправка всех запросов
    await Promise.all(variantRequests);
    
    setSuccess('Все варианты успешно добавлены!');
    setTimeout(() => navigate(`/profile`), 2000);
  } catch (err) {
    console.error('Детали ошибки:', {
      status: err.response?.status,
      data: err.response?.data,
      config: err.config
    });
    
    setError(err.response?.data?.message || 'Ошибка при добавлении вариантов');
  }
};

  const handleVariantChange = (index, e) => {
    const { name, value } = e.target;
    const newVariants = [...variants];
    newVariants[index] = {
      ...newVariants[index],
      [name]: name === 'price' || name === 'count' ? parseFloat(value) || 0 : value
    };
    setVariants(newVariants);
  };

  const addVariant = () => {
    setVariants([...variants, { name: '', price: 0, count: 0 }]);
  };

  const removeVariant = (index) => {
    if (variants.length > 1) {
      const newVariants = [...variants];
      newVariants.splice(index, 1);
      setVariants(newVariants);
    }
  };

  return (
    <div className="create-product-container">
      <h2>Создать новый товар</h2>
      
      {error && <div className="error-message">{error}</div>}
      {success && <div className="success-message">{success}</div>}

      {step === 1 ? (
        <form onSubmit={handleCreateBaseProduct}>
          <div className="form-section">
            <h3>Основная информация о товаре</h3>
            
            <div className="form-group">
              <label>Категория*:</label>
              <input
                type="text"
                name="category"
                value={productData.category}
                onChange={(e) => setProductData({...productData, category: e.target.value})}
                required
              />
            </div>

            <div className="form-group">
              <label>Название товара*:</label>
              <input
                type="text"
                name="name"
                value={productData.name}
                onChange={(e) => setProductData({...productData, name: e.target.value})}
                required
              />
            </div>

            <div className="form-actions">
              <button type="submit" className="submit-btn">
                Создать товар
              </button>
              <button
                type="button"
                onClick={() => navigate(`/profile`)}
                className="cancel-btn"
              >
                Отмена
              </button>
            </div>
          </div>
        </form>
      ) : (
        <form onSubmit={handleAddVariants}>
          <div className="form-section">
            <h3>Добавление вариантов для товара</h3>
            <p>ID товара: {productId}</p>
            
            {variants.map((variant, index) => (
              <div key={index} className="variant-group">
                <div className="form-group">
                  <label>Название варианта*:</label>
                  <input
                    type="text"
                    name="name"
                    value={variant.name}
                    onChange={(e) => handleVariantChange(index, e)}
                    required
                  />
                </div>

                <div className="form-group">
                  <label>Цена варианта*:</label>
                  <input
                    type="number"
                    name="price"
                    value={variant.price}
                    onChange={(e) => handleVariantChange(index, e)}
                    min="0"
                    step="0.01"
                    required
                  />
                </div>

                <div className="form-group">
                  <label>Количество*:</label>
                  <input
                    type="number"
                    name="count"
                    value={variant.count}
                    onChange={(e) => handleVariantChange(index, e)}
                    min="0"
                    required
                  />
                </div>

                {variants.length > 1 && (
                  <button
                    type="button"
                    onClick={() => removeVariant(index)}
                    className="remove-variant"
                  >
                    Удалить вариант
                  </button>
                )}
              </div>
            ))}

            <button
              type="button"
              onClick={addVariant}
              className="add-variant"
            >
              Добавить вариант
            </button>

            <div className="form-actions">
              <button type="submit" className="submit-btn">
                Сохранить варианты
              </button>
              <button
                type="button"
                onClick={() => setStep(1)}
                className="cancel-btn"
              >
                Назад
              </button>
            </div>
          </div>
        </form>
      )}
    </div>
  );
};

export default CreateProduct;
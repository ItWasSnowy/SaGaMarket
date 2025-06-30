import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import axios from 'axios';

const VariantDetail = () => {
  const { variantId } = useParams();
  const [variant, setVariant] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchVariant = async () => {
      try {
        const response = await axios.get(`/api/variant/${variantId}`);
        setVariant(response.data);
      } catch (error) {
        console.error('Error fetching variant:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchVariant();
  }, [variantId]);

  if (loading) return <div>Loading...</div>;
  if (!variant) return <div>Variant not found</div>;

  return (
    <div className="variant-detail">
      <h2>{variant.name}</h2>
      <p>{variant.description}</p>
      <p className="price">{variant.price} ₽</p>
      {/* Дополнительная информация о варианте */}
    </div>
  );
};

export default VariantDetail;
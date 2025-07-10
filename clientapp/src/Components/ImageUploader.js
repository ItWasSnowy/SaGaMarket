import React, { useState } from 'react';
import axios from 'axios';

const ImageUploader = ({ variantId, onUploadSuccess }) => {
  const [selectedFile, setSelectedFile] = useState(null);
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState('');

  const handleFileChange = (e) => {
    setSelectedFile(e.target.files[0]);
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      setError('Выберите файл для загрузки');
      return;
    }

    setIsUploading(true);
    setError('');

    try {
      const formData = new FormData();
      formData.append('image', selectedFile);

      const response = await axios.post(
        `https://localhost:7182/api/Media/upload-image/${variantId}`,
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
          withCredentials: true,
        }
      );

      if (response.data && response.data.imageUrl) {
        onUploadSuccess(response.data.imageUrl);
      }
    } catch (err) {
      console.error('Ошибка загрузки изображения:', err);
      setError(err.response?.data?.message || 'Не удалось загрузить изображение');
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="image-uploader">
      <input type="file" accept="image/*" onChange={handleFileChange} />
      <button onClick={handleUpload} disabled={!selectedFile || isUploading}>
        {isUploading ? 'Загрузка...' : 'Загрузить изображение'}
      </button>
      {error && <div className="error-message">{error}</div>}
      {selectedFile && (
        <div className="file-info">
          Выбран файл: {selectedFile.name} ({Math.round(selectedFile.size / 1024)} KB)
        </div>
      )}
    </div>
  );
};

export default ImageUploader;
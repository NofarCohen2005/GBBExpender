import React from 'react';
import './Modal.css';


interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  type: 'success' | 'error';
  title: string;
  message: string;
  subMessage?: string;
}

/**
 * A premium modal popup for displaying success or error notifications.
 * Features a blurred backdrop and luxury animations.
 */
const Modal: React.FC<ModalProps> = ({ isOpen, onClose, type, title, message, subMessage }) => {
  if (!isOpen) return null;

  // Handle clicking on the backdrop to close
  const handleBackdropClick = (e: React.MouseEvent) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <div className="modal-overlay" onClick={handleBackdropClick}>
      <div className="modal-card">
        <div className={`modal-icon ${type}`}>
          {type === 'success' ? '✓' : '✕'}
        </div>
        <h2>{title}</h2>
        <p>{message}</p>
        {subMessage && (
          <p className="modal-sub-message">{subMessage}</p>
        )}
        <button className="modal-close-btn" onClick={onClose} type="button">
          Continue
        </button>
      </div>
    </div>
  );
};

export default Modal;

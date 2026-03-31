import React, { useState, useEffect } from 'react';
import './SettingsModal.css';

interface SettingsModalProps {
  isOpen: boolean;
  onClose: () => void;
  settings: Record<string, string>;
  onSave: (newSettings: Record<string, string>) => Promise<boolean>;
}

const SettingsModal: React.FC<SettingsModalProps> = ({ isOpen, onClose, settings, onSave }) => {
  const [localSettings, setLocalSettings] = useState(settings);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (isOpen) {
      setLocalSettings(settings);
    }
  }, [isOpen, settings]);

  if (!isOpen) return null;

  const handleSave = async () => {
    setIsSaving(true);
    const success = await onSave(localSettings);
    if (success) {
      onClose();
    }
    setIsSaving(false);
  };

  const formatLabel = (key: string) => {
    return key
      .replace(/([A-Z])/g, ' $1')
      .replace(/^Cpp/, 'C++')
      .replace(/^Cs/, 'C#')
      .trim();
  };

  const renderField = (key: string) => (
    <div key={key} className={`settings-field ${key === 'WorkspaceRoot' ? 'full-width' : ''}`}>
      <label>{formatLabel(key)}</label>
      <input 
        value={localSettings[key] || ''} 
        onChange={e => setLocalSettings({ ...localSettings, [key]: e.target.value })}
        placeholder={`Enter path for ${formatLabel(key)}`}
        spellCheck="false"
      />
    </div>
  );

  const workspaceRootKey = 'WorkspaceRoot';

  return (
    <div className="modal-overlay" onClick={e => e.target === e.currentTarget && onClose()}>
      <div className="settings-card">
        <div className="settings-header">
          <div className="header-info">
            <h2>Project Configuration</h2>
            <p>Manage your GBB workspace and file paths</p>
          </div>
          <button className="close-x" onClick={onClose}>&times;</button>
        </div>

        <div className="settings-body">
          {Object.keys(localSettings).length === 0 && !isSaving && (
             <div className="error-message">Unable to load configuration. Please ensure the backend is running.</div>
          )}
          
          <div className="settings-section root-section">
            <div className="section-grid single-field">
              {renderField(workspaceRootKey)}
            </div>
          </div>
        </div>

        <div className="settings-actions">
          <button className="cancel-btn" onClick={onClose} disabled={isSaving}>Cancel</button>
          <button className="save-btn" onClick={handleSave} disabled={isSaving}>
            {isSaving ? (
              <>
                <span className="spinner"></span> Saving...
              </>
            ) : (
              'Save Configuration'
            )}
          </button>
        </div>
      </div>
    </div>
  );
};

export default SettingsModal;

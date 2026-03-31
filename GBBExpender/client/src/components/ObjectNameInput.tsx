import React from 'react';
import './ObjectNameInput.css';

import type { EntryType } from '../types';

interface ObjectNameInputProps {
  objectName: string;
  setObjectName: (name: string) => void;
  entryType: EntryType;
}

/**
 * Renders the input field for the descriptor/message name.
 * 
 * @param {string} objectName - Name of the object being generated.
 * @param {(name: string) => void} setObjectName - Callback to update the object name.
 * @param {EntryType} entryType - Current generation mode (for placeholder text).
 */
const ObjectNameInput: React.FC<ObjectNameInputProps> = ({ objectName, setObjectName, entryType }) => {
  return (
    <div className="form-group">
      <label>{entryType} Name</label>
      <input 
        type="text" 
        placeholder={`Enter name (e.g. My${entryType})`}
        value={objectName}
        onChange={(e) => setObjectName(e.target.value)}
      />
    </div>
  );
};

export default ObjectNameInput;

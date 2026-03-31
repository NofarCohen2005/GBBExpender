import React from 'react';
import './EntryTypeToggle.css';

import type { EntryType } from '../types';

interface EntryTypeToggleProps {
  entryType: EntryType;
  setEntryType: (type: EntryType) => void;
}

/**
 * Renders a toggle button group to switch between 'Descriptor' and 'Message' generation modes.
 * 
 * @param {EntryType} entryType - Current generation mode.
 * @param {(type: EntryType) => void} setEntryType - Callback to update the generation mode.
 */
const EntryTypeToggle: React.FC<EntryTypeToggleProps> = ({ entryType, setEntryType }) => {
  return (
    <div className="toggle-group">
      <button 
        className={`toggle-btn ${entryType === 'Descriptor' ? 'active' : ''}`}
        onClick={() => setEntryType('Descriptor')}
        type="button"
      >
        Descriptor
      </button>
      <button 
        className={`toggle-btn ${entryType === 'Message' ? 'active' : ''}`}
        onClick={() => setEntryType('Message')}
        type="button"
      >
        Message
      </button>
    </div>
  );
};

export default EntryTypeToggle;

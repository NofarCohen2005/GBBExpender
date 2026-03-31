import React from 'react';
import './GeneratorForm.css';

import FormHeader from './components/FormHeader';
import EntryTypeToggle from './components/EntryTypeToggle';
import ObjectNameInput from './components/ObjectNameInput';
import PropertiesSection from './components/PropertiesSection';
import Modal from './components/Modal';
import { useGenerator } from './hooks/useGenerator';

/**
 * Main form component for GBB Code Generation.
 * Uses the custom useGenerator hook to manage state and logic.
 */
const GeneratorForm: React.FC = () => {
  const {
    entryType, setEntryType,
    objectName, setObjectName,
    properties, loading, modal,
    closeModal, handleUpdateProperty,
    handleAddField, handleRemoveField, handleGenerate
  } = useGenerator();

  return (
    <>
      <div className="luxury-card">
        <FormHeader />

        <EntryTypeToggle 
          entryType={entryType} 
          setEntryType={setEntryType} 
        />

        <ObjectNameInput 
          objectName={objectName} 
          setObjectName={setObjectName} 
          entryType={entryType} 
        />

        <PropertiesSection 
          properties={properties}
          onUpdateProperty={handleUpdateProperty}
          onRemoveProperty={handleRemoveField}
          onAddProperty={handleAddField}
        />

        <button 
          className="generate-btn" 
          onClick={handleGenerate}
          disabled={loading || !objectName}
          type="button"
        >
          {loading ? 'Processing...' : `Generate ${entryType}`}
        </button>
      </div>

      <Modal 
        isOpen={modal.isOpen}
        onClose={closeModal}
        type={modal.type}
        title={modal.title}
        message={modal.message}
        subMessage={modal.redSubMessage}
      />
    </>
  );
};

export default GeneratorForm;

import { useState } from 'react';
import type { Property, EntryType, GeneratorRequest } from '../types';

/**
 * Custom hook to manage GBB Generator form state and logic.
 * Handles property updates, validation, and API communication.
 */
export const useGenerator = () => {
  const [entryType, setEntryType] = useState<EntryType>('Descriptor');
  const [objectName, setObjectName] = useState('');
  const [properties, setProperties] = useState<Property[]>([{ name: '', dataType: 'int', defaultValue: '0' }]);
  const [loading, setLoading] = useState(false);
  
  const [modal, setModal] = useState<{
    isOpen: boolean;
    type: 'success' | 'error';
    title: string;
    message: string;
    redSubMessage?: string;
  }>({
    isOpen: false,
    type: 'success',
    title: '',
    message: '',
    redSubMessage: '',
  });

  const showModal = (type: 'success' | 'error', title: string, message: string, redSubMessage?: string) => {
    setModal({ isOpen: true, type, title, message, redSubMessage });
  };

  const closeModal = () => setModal(prev => ({ ...prev, isOpen: false }));

  const handleUpdateProperty = (index: number, field: keyof Property, value: string | number) => {
    const updated = [...properties];
    const newProp = { ...updated[index], [field]: value };
    
    if (field === 'dataType') {
      if (value === 'bool') newProp.defaultValue = 'false';
      else if (['int', 'uint', 'double', 'short', 'string'].includes(value as string)) {
        newProp.defaultValue = '0';
      }
    }
    
    updated[index] = newProp;
    setProperties(updated);
  };

  const handleAddField = () => {
    setProperties([...properties, { name: '', dataType: 'int', defaultValue: '0' }]);
  };

  const handleRemoveField = (index: number) => {
    if (properties.length > 1) {
      setProperties(properties.filter((_, i) => i !== index));
    }
  };

  const handleGenerate = async () => {
    if (!objectName.trim()) {
      showModal('error', 'Validation Error', 'Object Name is required.');
      return;
    }

    if (properties.length === 0 || properties.some(p => !p.name.trim())) {
      showModal('error', 'Validation Error', 'All properties must have a valid name.');
      return;
    }

    setLoading(true);
    try {
      const resp = await fetch('http://localhost:5050/api/gbb/generate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ entryType, objectName, properties }),
      });

      const data = await resp.json();
      if (data.status === 'Success') {
        showModal('success', 'Generation Success', data.message || 'Generated successfully!', 'Dont forget to make the needed changes in GBBEditor');
      } else {
        showModal('error', 'Generation Error', data.message || 'An error occurred.');
      }
    } catch (err) {
      showModal('error', 'Connection Error', 'Failed to connect to the generation server.');
    } finally {
      setLoading(false);
    }
  };

  return {
    entryType, setEntryType,
    objectName, setObjectName,
    properties, loading, modal,
    closeModal, handleUpdateProperty, 
    handleAddField, handleRemoveField, handleGenerate
  };
};

import React, { useState } from 'react';
import PropertyRow from './components/PropertyRow';
import type { Property, EntryType, GeneratorRequest } from './types';

const GeneratorForm: React.FC = () => {
  const [entryType, setEntryType] = useState<EntryType>('Descriptor');
  const [objectName, setObjectName] = useState('');
  const [properties, setProperties] = useState<Property[]>([{ name: '', dataType: 'int', defaultValue: '0' }]);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<any>(null);

  const handleUpdateProperty = (index: number, field: keyof Property, value: string) => {
    const updated = [...properties];
    updated[index][field] = value;
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
    // Validation
    if (!objectName.trim()) {
      setResult({ status: 'Error', message: 'Object Name is required' });
      return;
    }

    if (properties.length === 0) {
      setResult({ status: 'Error', message: 'At least one property is required' });
      return;
    }

    const hasEmptyName = properties.some(p => !p.name.trim());
    if (hasEmptyName) {
      setResult({ status: 'Error', message: 'All properties must have a name' });
      return;
    }

    setLoading(true);
    setResult(null);

    const payload: GeneratorRequest = {
      entryType,
      objectName,
      properties,
    };

    try {
      const resp = await fetch('http://localhost:5050/api/gbb/generate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });

      const data = await resp.json();
      setResult(data);
      console.log('Generated code:', data);
    } catch (err) {
      console.error('Generation failed:', err);
      setResult({ status: 'Error', message: 'Failed to connect to server' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="luxury-card">
      <div className="generator-header">
        <h1>GBB Generator</h1>
        <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
          Automated Code Registration System
        </p>
      </div>

      <div className="toggle-group">
        <button 
          className={`toggle-btn ${entryType === 'Descriptor' ? 'active' : ''}`}
          onClick={() => setEntryType('Descriptor')}
        >
          Descriptor
        </button>
        <button 
          className={`toggle-btn ${entryType === 'Message' ? 'active' : ''}`}
          onClick={() => setEntryType('Message')}
        >
          Message
        </button>
      </div>

      <div className="form-group">
        <label>Object Name</label>
        <input 
          type="text" 
          placeholder={`Enter name (e.g. My${entryType})`}
          value={objectName}
          onChange={(e) => setObjectName(e.target.value)}
        />
      </div>

      <div className="properties-section">
        <label>Fields & Properties</label>
        <div className="properties-list">
          {properties.map((prop, index) => (
            <PropertyRow 
              key={index}
              property={prop}
              index={index}
              onUpdate={handleUpdateProperty}
              onRemove={handleRemoveField}
              showRemove={properties.length > 1}
            />
          ))}
        </div>
        <button type="button" className="add-btn" onClick={handleAddField}>
          + Add New Field
        </button>
      </div>

      <button 
        className="generate-btn" 
        onClick={handleGenerate}
        disabled={loading || !objectName}
      >
        {loading ? 'Processing...' : `Generate ${entryType}`}
      </button>

      {result && (
        <div className={`result-message ${result.status?.toLowerCase()}`}>
          {result.message}
        </div>
      )}
    </div>
  );
};

export default GeneratorForm;

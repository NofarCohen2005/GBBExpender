import React from 'react';
import './PropertyRow.css';

import type { Property } from '../types';
import { DATA_TYPES } from '../types';

interface PropertyRowProps {
  property: Property;
  index: number;
  onUpdate: (index: number, field: keyof Property, value: string | number) => void;
  onRemove: (index: number) => void;
  showRemove: boolean;
}

const PropertyRow: React.FC<PropertyRowProps> = ({ property, index, onUpdate, onRemove, showRemove }) => {
  return (
    <div className="property-row">
      <input
        type="text"
        placeholder="Name"
        className="field-name"
        value={property.name}
        onChange={(e) => onUpdate(index, 'name', e.target.value)}
      />
      <select
        value={property.dataType}
        className="field-type"
        onChange={(e) => onUpdate(index, 'dataType', e.target.value)}
      >
        {DATA_TYPES.map((type) => (
          <option key={type} value={type}>
            {type}
          </option>
        ))}
      </select>
      
      {property.dataType === 'string' && (
        <input
          type="number"
          placeholder="Size"
          className="field-size"
          value={property.size || ''}
          onChange={(e) => onUpdate(index, 'size', parseInt(e.target.value) || 0)}
          style={{ width: '80px' }}
        />
      )}

      {property.dataType === 'bool' ? (
        <select
          className="field-default"
          value={property.defaultValue}
          onChange={(e) => onUpdate(index, 'defaultValue', e.target.value)}
          title="Choose the initial boolean state (defaults to false)."
        >
          <option value="true">true</option>
          <option value="false">false</option>
        </select>
      ) : (
        <input
          type="text"
          placeholder="Default Value"
          className="field-default"
          value={property.defaultValue}
          onChange={(e) => onUpdate(index, 'defaultValue', e.target.value)}
          title="Enter a constant value, or use 'min'/'max' for the type's limit values."
        />
      )}
      {showRemove && (
        <button 
          type="button" 
          className="remove-btn" 
          onClick={() => onRemove(index)}
          title="Remove property"
        >
          ×
        </button>
      )}
    </div>
  );
};

export default PropertyRow;

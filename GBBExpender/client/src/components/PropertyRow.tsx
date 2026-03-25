import React from 'react';
import type { Property } from '../types';
import { DATA_TYPES } from '../types';

interface PropertyRowProps {
  property: Property;
  index: number;
  onUpdate: (index: number, field: keyof Property, value: string) => void;
  onRemove: (index: number) => void;
  showRemove: boolean;
}

const PropertyRow: React.FC<PropertyRowProps> = ({ property, index, onUpdate, onRemove, showRemove }) => {
  return (
    <div className="property-row">
      <input
        type="text"
        placeholder="Name"
        value={property.name}
        onChange={(e) => onUpdate(index, 'name', e.target.value)}
      />
      <select
        value={property.dataType}
        onChange={(e) => onUpdate(index, 'dataType', e.target.value)}
      >
        {DATA_TYPES.map((type) => (
          <option key={type} value={type}>
            {type}
          </option>
        ))}
      </select>
      <input
        type="text"
        placeholder="Default Value"
        value={property.defaultValue}
        onChange={(e) => onUpdate(index, 'defaultValue', e.target.value)}
      />
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

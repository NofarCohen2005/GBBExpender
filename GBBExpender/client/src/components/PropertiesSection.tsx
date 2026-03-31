import React from 'react';
import './PropertiesSection.css';

import PropertyRow from './PropertyRow';
import type { Property } from '../types';

interface PropertiesSectionProps {
  properties: Property[];
  onUpdateProperty: (index: number, field: keyof Property, value: string | number) => void;
  onRemoveProperty: (index: number) => void;
  onAddProperty: () => void;
}

/**
 * Manages the list of property fields and the "Add New Field" action.
 * 
 * @param {Property[]} properties - List of properties to display.
 * @param {Function} onUpdateProperty - Callback to update a property's field.
 * @param {Function} onRemoveProperty - Callback to remove a property.
 * @param {Function} onAddProperty - Callback to add a new property.
 */
const PropertiesSection: React.FC<PropertiesSectionProps> = ({ 
  properties, 
  onUpdateProperty, 
  onRemoveProperty, 
  onAddProperty 
}) => {
  return (
    <div className="properties-section">
      <label>Fields</label>
      <div className="properties-list">
        {properties.map((prop, index) => (
          <PropertyRow 
            key={index}
            property={prop}
            index={index}
            onUpdate={onUpdateProperty}
            onRemove={onRemoveProperty}
            showRemove={properties.length > 1}
          />
        ))}
      </div>
      <button type="button" className="add-btn" onClick={onAddProperty}>
        + Add New Field
      </button>
    </div>
  );
};

export default PropertiesSection;
import React, { useState } from 'react';

export const DATA_TYPES = ['int', 'uint', 'double', 'bool', 'byte', 'string'] as const;
export type DataType = typeof DATA_TYPES[number];

export interface Property {
    name: string;
    dataType: DataType;
    defaultValue: string;
}

interface GeneratorFormProps {
    className?: string;
}

const GeneratorForm: React.FC<GeneratorFormProps> = ({ className }) => {
    const [entryType, setEntryType] = useState<'Descriptor' | 'Message'>('Descriptor');
    const [objectName, setObjectName] = useState('');
    const [properties, setProperties] = useState<Property[]>([{ name: '', dataType: 'int', defaultValue: '0' }]);
    const [loading, setLoading] = useState(false);

    const addProperty = () => {
        setProperties([...properties, { name: '', dataType: 'int', defaultValue: '0' }]);
    };

    const removeProperty = (index: number) => {
        setProperties(properties.filter((_, i) => i !== index));
    };

    const updateProperty = (index: number, field: keyof Property, value: string) => {
        const newProperties = [...properties];
        (newProperties[index] as any)[field] = value;
        setProperties(newProperties);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            const response = await fetch('http://localhost:5050/api/gbb/generate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    entryType,
                    objectName,
                    properties
                })
            });
            const data = await response.json();
            console.log('Generated code:', data);
            alert('Code generated successfully! Check console for details.');
        } catch (error) {
            console.error('Error generating code:', error);
            alert('Failed to generate code. Is the backend running?');
        } finally {
            setLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className={`generator-form ${entryType.toLowerCase()} ${className || ''}`}>
            <EntryTypeToggle entryType={entryType} setEntryType={setEntryType} />
            
            <div className="form-group">
                <label>Object Name</label>
                <input
                    type="text"
                    value={objectName}
                    onChange={(e) => setObjectName(e.target.value)}
                    placeholder="e.g. HealthData"
                    className="main-input"
                    required
                />
            </div>

            <PropertiesManager 
                properties={properties} 
                updateProperty={updateProperty} 
                removeProperty={removeProperty} 
                addProperty={addProperty} 
            />

            <button type="submit" className={`btn-submit ${entryType.toLowerCase()}`} disabled={loading}>
                {loading ? 'Processing...' : `Generate ${entryType}`}
            </button>
        </form>
    );
};

interface EntryTypeToggleProps {
    entryType: 'Descriptor' | 'Message';
    setEntryType: (type: 'Descriptor' | 'Message') => void;
}

const EntryTypeToggle: React.FC<EntryTypeToggleProps> = ({ entryType, setEntryType }) => (
    <div className="form-group">
        <label>Entry Type</label>
        <div className="toggle-group">
            <button
                type="button"
                className={entryType === 'Descriptor' ? 'active descriptor' : 'inactive'}
                onClick={() => setEntryType('Descriptor')}
            >
                <span className="icon">📄</span> Descriptor
            </button>
            <button
                type="button"
                className={entryType === 'Message' ? 'active message' : 'inactive'}
                onClick={() => setEntryType('Message')}
            >
                <span className="icon">✉️</span> Message
            </button>
        </div>
    </div>
);

interface PropertiesManagerProps {
    properties: Property[];
    updateProperty: (index: number, field: keyof Property, value: string) => void;
    removeProperty: (index: number) => void;
    addProperty: () => void;
}

const PropertiesManager: React.FC<PropertiesManagerProps> = ({ properties, updateProperty, removeProperty, addProperty }) => (
    <div className="form-group props-manager">
        <label>Properties (Fields)</label>
        <div className="properties-container">
            {properties.map((prop, index) => (
                <PropertyRow 
                    key={index} 
                    prop={prop} 
                    index={index} 
                    showRemove={properties.length > 1}
                    onUpdate={updateProperty} 
                    onRemove={removeProperty} 
                />
            ))}
        </div>
        <button type="button" className="btn-add" onClick={addProperty}>
            <span>+</span> Add New Field
        </button>
    </div>
);

interface PropertyRowProps {
    prop: Property;
    index: number;
    showRemove: boolean;
    onUpdate: (index: number, field: keyof Property, value: string) => void;
    onRemove: (index: number) => void;
}

const PropertyRow: React.FC<PropertyRowProps> = ({ prop, index, showRemove, onUpdate, onRemove }) => (
    <div className="property-row animate-in">
        <input
            type="text"
            placeholder="Name"
            value={prop.name}
            onChange={(e) => onUpdate(index, 'name', e.target.value)}
            required
        />
        <select
            value={prop.dataType}
            onChange={(e) => onUpdate(index, 'dataType', e.target.value as DataType)}
        >
            {DATA_TYPES.map(type => <option key={type} value={type}>{type}</option>)}
        </select>
        <input
            type="text"
            placeholder="Default"
            value={prop.defaultValue}
            onChange={(e) => onUpdate(index, 'defaultValue', e.target.value)}
            required
        />
        {showRemove && (
            <button type="button" className="btn-remove" onClick={() => onRemove(index)} title="Remove Field">×</button>
        )}
    </div>
);

export default GeneratorForm;

import React from 'react';
import './FormHeader.css';


/**
 * Renders the title and subtitle of the GBB Generator form.
 */
const FormHeader: React.FC = () => {
  return (
    <div className="generator-header">
      <h1>GBB Generator</h1>
      <p style={{ color: 'var(--text-secondary)', fontSize: '0.9rem' }}>
        Automated Code Registration System
      </p>
    </div>
  );
};

export default FormHeader;

import React from 'react';
import './App.css';
import GeneratorForm from './GeneratorForm';

const App: React.FC = () => {
    return (
        <div className="container">
            <main>
                <GeneratorForm />
            </main>
        </div>
    );
};

export default App;

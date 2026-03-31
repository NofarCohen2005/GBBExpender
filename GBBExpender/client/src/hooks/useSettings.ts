import { useState, useEffect } from 'react';

export const useSettings = () => {
  const [settings, setSettings] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchSettings = async (retryCount = 0) => {
    const maxRetries = 10;
    const delay = 2000;

    setLoading(true);
    try {
      const resp = await fetch('http://localhost:5050/api/config');
      if (!resp.ok) throw new Error(`HTTP error! status: ${resp.status}`);
      
      const data = await resp.json();
      setSettings(data);
      setError(null);
    } catch (err) {
      if (retryCount < maxRetries) {
        console.log(`Connection failed. Retrying in ${delay/1000}s... (Attempt ${retryCount + 1}/${maxRetries})`);
        setTimeout(() => fetchSettings(retryCount + 1), delay);
      } else {
        setError('Failed to load settings after multiple attempts');
      }
    } finally {
      if (retryCount === 0) setLoading(false);
    }
  };

  const saveSettings = async (newSettings: Record<string, string>) => {
    setLoading(true);
    try {
      const resp = await fetch('http://localhost:5050/api/config', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newSettings),
      });
      const data = await resp.json();
      if (data.status === 'Success') {
        setSettings(newSettings);
        return true;
      }
      return false;
    } catch (err) {
      setError('Failed to save settings');
      return false;
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchSettings(); }, []);

  return { settings, loading, error, saveSettings, refresh: fetchSettings };
};

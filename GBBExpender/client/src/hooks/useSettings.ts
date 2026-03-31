import { useState, useEffect } from 'react';

export const useSettings = () => {
  const [settings, setSettings] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchSettings = async () => {
    setLoading(true);
    try {
      const resp = await fetch('http://localhost:5050/api/config');
      const data = await resp.json();
      setSettings(data);
    } catch (err) {
      setError('Failed to load settings');
    } finally {
      setLoading(false);
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

import { createContext, useState, useCallback, useEffect, type ReactNode } from 'react';
import { login as loginService } from '../services/auth';

export interface AuthContextValue {
  token: string | null;
  isAuthenticated: boolean;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => {
    const stored = localStorage.getItem('access_token');
    console.log('🔄 AuthProvider inicializando - Token no localStorage:', stored ? `${stored.substring(0, 30)}...` : 'NULL');
    return stored;
  });

  useEffect(() => {
    const storedToken = localStorage.getItem('access_token');
    console.log('🔄 useEffect verificando token:', storedToken ? 'Token encontrado' : 'Token não encontrado');
    if (storedToken && !token) {
      console.log('⚠️ Token estava no localStorage mas não no state, atualizando...');
      setToken(storedToken);
    }
  }, [token]);

  const login = useCallback(async (username: string, password: string) => {
    const res = await loginService(username, password);
    console.log('Login response:', res);
    console.log('Token recebido:', res.token);
    localStorage.setItem('access_token', res.token);
    setToken(res.token);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('access_token');
    setToken(null);
  }, []);

  return (
    <AuthContext.Provider value={{ token, isAuthenticated: !!token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}


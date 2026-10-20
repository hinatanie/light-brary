import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import type { AuthResponse, MeResponse } from '../types';
import { ApiError, authApi } from '../api';

export interface UserSession {
  userId: string;
  fullName: string;
  email?: string;
  phoneNumber?: string | null;
  roles: string[];
  mustChangePassword?: boolean;
}

interface AuthContextType {
  token: string | null;
  user: UserSession | null;
  loading: boolean;
  isAuthenticated: boolean;
  login: (data: AuthResponse) => void;
  logout: () => Promise<void>;
  hasRole: (role: string) => boolean;
  refreshUser: () => Promise<void>;
  updateUserSession: (updated: Partial<UserSession>) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [token, setToken] = useState<string | null>(() => localStorage.getItem('lightbrary_token'));
  const [user, setUser] = useState<UserSession | null>(() => {
    const saved = localStorage.getItem('lightbrary_user');
    if (!saved) return null;
    try {
      const parsed = JSON.parse(saved);
      return {
        userId: parsed.userId,
        fullName: parsed.fullName,
        email: parsed.email,
        phoneNumber: parsed.phoneNumber || null,
        roles: Array.isArray(parsed.roles) && parsed.roles.length > 0 ? parsed.roles : ['Member'],
        mustChangePassword: Boolean(parsed.mustChangePassword),
      };
    } catch {
      return null;
    }
  });
  const [loading, setLoading] = useState<boolean>(true);

  const clearSession = useCallback(() => {
    localStorage.removeItem('lightbrary_token');
    localStorage.removeItem('lightbrary_user');
    setToken(null);
    setUser(null);
  }, []);

  const refreshUser = useCallback(async () => {
    const curToken = localStorage.getItem('lightbrary_token');
    if (!curToken) {
      setUser(null);
      setLoading(false);
      return;
    }

    try {
      const me: MeResponse = await authApi.getMe();
      const roles = me.roles && me.roles.length > 0 ? me.roles : ['Member'];
      const updatedUser: UserSession = {
        userId: me.userId,
        fullName: me.fullName,
        email: me.email,
        phoneNumber: me.phoneNumber,
        roles,
        mustChangePassword: Boolean(me.mustChangePassword),
      };
      setUser(updatedUser);
      localStorage.setItem('lightbrary_user', JSON.stringify(updatedUser));
    } catch (err) {
      console.warn('Failed to fetch user session from /auth/me:', err);
      // If token expired or invalid, log out
      if ((err as any)?.status === 401) {
        clearSession();
      }
    } finally {
      setLoading(false);
    }
  }, [clearSession]);

  useEffect(() => {
    refreshUser();
  }, [refreshUser]);

  const login = (data: AuthResponse) => {
    localStorage.setItem('lightbrary_token', data.token);
    const roles = data.roles && data.roles.length > 0 ? data.roles : ['Member'];
    const session: UserSession = {
      userId: data.userId,
      fullName: data.fullName,
      email: data.email,
      phoneNumber: data.phoneNumber,
      roles,
      mustChangePassword: Boolean(data.mustChangePassword),
    };
    localStorage.setItem('lightbrary_user', JSON.stringify(session));
    setToken(data.token);
    setUser(session);
  };

  const logout = async () => {
    if (localStorage.getItem('lightbrary_token')) {
      try {
        await authApi.logout();
      } catch (err) {
        // An expired or revoked token already has no access to protected APIs.
        if (!(err instanceof ApiError) || err.status !== 401) throw err;
      }
    }
    clearSession();
  };

  const hasRole = (role: string): boolean => {
    if (!user || !user.roles) return false;
    return user.roles.some((r) => r.toLowerCase() === role.toLowerCase());
  };

  const updateUserSession = (updated: Partial<UserSession>) => {
    setUser((prev) => {
      if (!prev) return null;
      const next = { ...prev, ...updated };
      localStorage.setItem('lightbrary_user', JSON.stringify(next));
      return next;
    });
  };

  return (
    <AuthContext.Provider
      value={{
        token,
        user,
        loading,
        isAuthenticated: !!token && !!user,
        login,
        logout,
        hasRole,
        refreshUser,
        updateUserSession,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

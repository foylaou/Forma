/**
 * Auth Store - 認證狀態管理
 * 使用 Zustand 管理登入狀態和使用者資訊
 * Token 由後端 HTTP-only Cookie 管理
 */

import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { authApi } from '@/lib/api/auth';
import type { UserDto, LoginRequest, RegisterRequest, AuthResponseDto } from '@/types/api';

interface AuthState {
  // 狀態
  user: UserDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;

  // 動作
  login: (data: LoginRequest) => Promise<void>;
  loginWithFido2Response: (response: AuthResponseDto) => void;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshAccessToken: () => Promise<boolean>;
  clearError: () => void;
  setLoading: (loading: boolean) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      // 初始狀態
      user: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      // 登入
      login: async (data: LoginRequest) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authApi.login(data);

          set({
            user: response.user,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
        } catch (error) {
          const message = error instanceof Error ? error.message : '登入失敗';
          set({
            isLoading: false,
            error: message,
            isAuthenticated: false,
          });
          throw error;
        }
      },

      // FIDO2 登入完成後設定狀態
      loginWithFido2Response: (response: AuthResponseDto) => {
        set({
          user: response.user,
          isAuthenticated: true,
          isLoading: false,
          error: null,
        });
      },

      // 註冊
      register: async (data: RegisterRequest) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authApi.register(data);

          set({
            user: response.user,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
        } catch (error) {
          const message = error instanceof Error ? error.message : '註冊失敗';
          set({
            isLoading: false,
            error: message,
          });
          throw error;
        }
      },

      // 登出
      logout: async () => {
        try {
          await authApi.logout();
        } catch {
          // 忽略登出 API 錯誤
        } finally {
          set({
            user: null,
            isAuthenticated: false,
            error: null,
          });
        }
      },

      // 刷新 Token
      refreshAccessToken: async () => {
        try {
          const response = await authApi.refreshToken();
          set({
            user: response.user,
          });
          return true;
        } catch {
          set({
            user: null,
            isAuthenticated: false,
          });
          return false;
        }
      },

      // 清除錯誤
      clearError: () => set({ error: null }),

      // 設定載入狀態
      setLoading: (loading: boolean) => set({ isLoading: loading }),
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);

export default useAuthStore;

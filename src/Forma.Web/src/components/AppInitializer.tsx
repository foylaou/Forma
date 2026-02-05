/**
 * AppInitializer - 應用程式初始化組件
 * 負責檢查系統狀態並決定路由
 */

import { useState, useEffect, createContext, useContext, type ReactNode } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Box, CircularProgress, Typography } from '@mui/material';
import { authApi } from '@/lib/api/auth';

interface SystemStatus {
  isInitialized: boolean;
  version: string;
  isLoading: boolean;
  error: string | null;
}

const SystemStatusContext = createContext<SystemStatus>({
  isInitialized: true,
  version: '',
  isLoading: true,
  error: null,
});

export const useSystemStatus = () => useContext(SystemStatusContext);

interface AppInitializerProps {
  children: ReactNode;
}

export function AppInitializer({ children }: AppInitializerProps) {
  const navigate = useNavigate();
  const location = useLocation();
  const [status, setStatus] = useState<SystemStatus>({
    isInitialized: true,
    version: '',
    isLoading: true,
    error: null,
  });

  useEffect(() => {
    checkSystemStatus();
  }, []);

  useEffect(() => {
    // 當狀態載入完成後，檢查是否需要重定向
    if (!status.isLoading && !status.error) {
      if (!status.isInitialized && location.pathname !== '/setup') {
        // 系統未初始化，重定向到設定頁面
        navigate('/setup', { replace: true });
      } else if (status.isInitialized && location.pathname === '/setup') {
        // 系統已初始化，不應該在設定頁面
        navigate('/login', { replace: true });
      }
    }
  }, [status.isLoading, status.isInitialized, location.pathname, navigate]);

  const checkSystemStatus = async () => {
    try {
      const result = await authApi.getSystemStatus();
      setStatus({
        isInitialized: result.isInitialized,
        version: result.version,
        isLoading: false,
        error: null,
      });
    } catch (err) {
      // API 錯誤時假設系統已初始化（避免阻擋用戶）
      console.error('Failed to check system status:', err);
      setStatus({
        isInitialized: true,
        version: '',
        isLoading: false,
        error: null,
      });
    }
  };

  // 顯示載入畫面
  if (status.isLoading) {
    return (
      <Box
        sx={{
          height: '100vh',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          gap: 2,
        }}
      >
        <CircularProgress />
        <Typography color="text.secondary">載入中...</Typography>
      </Box>
    );
  }

  return (
    <SystemStatusContext.Provider value={status}>
      {children}
    </SystemStatusContext.Provider>
  );
}

export default AppInitializer;

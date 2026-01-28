/**
 * LoginPage - 登入頁面
 * 支援登入和註冊切換
 */

import { useState } from 'react';
import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Alert,
  Link,
  InputAdornment,
  IconButton,
  CircularProgress,
  Divider,
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  Email as EmailIcon,
  Lock as LockIcon,
  Person as PersonIcon,
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { useAuthStore } from '@/stores/authStore';

interface LoginFormData {
  emailOrUsername: string;
  password: string;
}

interface RegisterFormData {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const [isRegisterMode, setIsRegisterMode] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const { login, register: registerUser, isLoading, error, clearError } = useAuthStore();

  // 取得重導向目標
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/';

  // 登入表單
  const loginForm = useForm<LoginFormData>({
    defaultValues: {
      emailOrUsername: '',
      password: '',
    },
  });

  // 註冊表單
  const registerForm = useForm<RegisterFormData>({
    defaultValues: {
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
    },
  });

  const handleLogin = async (data: LoginFormData) => {
    try {
      await login(data);
      navigate(from, { replace: true });
    } catch {
      // 錯誤已在 store 中處理
    }
  };

  const handleRegister = async (data: RegisterFormData) => {
    if (data.password !== data.confirmPassword) {
      registerForm.setError('confirmPassword', {
        type: 'manual',
        message: '密碼不一致',
      });
      return;
    }

    try {
      await registerUser(data);
      navigate(from, { replace: true });
    } catch {
      // 錯誤已在 store 中處理
    }
  };

  const toggleMode = () => {
    setIsRegisterMode(!isRegisterMode);
    clearError();
    loginForm.reset();
    registerForm.reset();
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: 'grey.100',
        p: 2,
      }}
    >
      <Paper
        elevation={8}
        sx={{
          width: '100%',
          maxWidth: 420,
          borderRadius: 3,
          overflow: 'hidden',
        }}
      >
        {/* Header */}
        <Box
          sx={{
            background: 'linear-gradient(135deg, #1976d2 0%, #1565c0 100%)',
            color: 'white',
            p: 4,
            textAlign: 'center',
          }}
        >
          <Typography variant="h4" fontWeight="bold" gutterBottom>
            Forma
          </Typography>
          <Typography variant="body2" sx={{ opacity: 0.9 }}>
            {isRegisterMode ? '建立新帳號' : '登入您的帳號'}
          </Typography>
        </Box>

        {/* Form */}
        <Box sx={{ p: 4 }}>
          {error && (
            <Alert severity="error" sx={{ mb: 3 }} onClose={clearError}>
              {error}
            </Alert>
          )}

          {isRegisterMode ? (
            // 註冊表單
            <form onSubmit={registerForm.handleSubmit(handleRegister)}>
              <TextField
                fullWidth
                label="使用者名稱"
                {...registerForm.register('username', {
                  required: '請輸入使用者名稱',
                  minLength: { value: 2, message: '名稱至少 2 個字元' },
                })}
                error={!!registerForm.formState.errors.username}
                helperText={registerForm.formState.errors.username?.message}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <PersonIcon color="action" />
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 2 }}
              />

              <TextField
                fullWidth
                label="Email"
                type="email"
                {...registerForm.register('email', {
                  required: '請輸入 Email',
                  pattern: {
                    value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                    message: '請輸入有效的 Email',
                  },
                })}
                error={!!registerForm.formState.errors.email}
                helperText={registerForm.formState.errors.email?.message}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <EmailIcon color="action" />
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 2 }}
              />

              <TextField
                fullWidth
                label="密碼"
                type={showPassword ? 'text' : 'password'}
                {...registerForm.register('password', {
                  required: '請輸入密碼',
                  minLength: { value: 6, message: '密碼至少 6 個字元' },
                })}
                error={!!registerForm.formState.errors.password}
                helperText={registerForm.formState.errors.password?.message}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <LockIcon color="action" />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowPassword(!showPassword)}
                        edge="end"
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 2 }}
              />

              <TextField
                fullWidth
                label="確認密碼"
                type={showConfirmPassword ? 'text' : 'password'}
                {...registerForm.register('confirmPassword', {
                  required: '請確認密碼',
                })}
                error={!!registerForm.formState.errors.confirmPassword}
                helperText={registerForm.formState.errors.confirmPassword?.message}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <LockIcon color="action" />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        edge="end"
                      >
                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 3 }}
              />

              <Button
                type="submit"
                fullWidth
                variant="contained"
                size="large"
                disabled={isLoading}
                sx={{ mb: 2, py: 1.5 }}
              >
                {isLoading ? <CircularProgress size={24} /> : '註冊'}
              </Button>
            </form>
          ) : (
            // 登入表單
            <form onSubmit={loginForm.handleSubmit(handleLogin)}>
              <TextField
                fullWidth
                label="Email 或使用者名稱"
                {...loginForm.register('emailOrUsername', {
                  required: '請輸入 Email 或使用者名稱',
                })}
                error={!!loginForm.formState.errors.emailOrUsername}
                helperText={loginForm.formState.errors.emailOrUsername?.message}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <EmailIcon color="action" />
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 2 }}
              />

              <TextField
                fullWidth
                label="密碼"
                type={showPassword ? 'text' : 'password'}
                {...loginForm.register('password', {
                  required: '請輸入密碼',
                })}
                error={!!loginForm.formState.errors.password}
                helperText={loginForm.formState.errors.password?.message}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <LockIcon color="action" />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowPassword(!showPassword)}
                        edge="end"
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                sx={{ mb: 3 }}
              />

              <Button
                type="submit"
                fullWidth
                variant="contained"
                size="large"
                disabled={isLoading}
                sx={{ mb: 2, py: 1.5 }}
              >
                {isLoading ? <CircularProgress size={24} /> : '登入'}
              </Button>
            </form>
          )}

          <Divider sx={{ my: 2 }} />

          {/* 切換登入/註冊 */}
          <Typography variant="body2" color="text.secondary" textAlign="center">
            {isRegisterMode ? '已有帳號？' : '還沒有帳號？'}
            <Link
              component="button"
              type="button"
              variant="body2"
              onClick={toggleMode}
              sx={{ ml: 0.5, fontWeight: 600 }}
            >
              {isRegisterMode ? '登入' : '立即註冊'}
            </Link>
          </Typography>
        </Box>
      </Paper>
    </Box>
  );
}

export default LoginPage;

import { ThemeProvider, createTheme, CssBaseline } from '@mui/material';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { FormPreviewPage } from './pages/FormPreviewPage';
import { LoginPage } from './pages/LoginPage';
import { SetupPage } from './pages/SetupPage';
import { DashboardPage } from './pages/DashboardPage';
import { ProjectDetailPage } from './pages/ProjectDetailPage';
import { ProjectMembersPage } from './pages/ProjectMembersPage';
import { ProjectSettingsPage } from './pages/ProjectSettingsPage';
import { FormEditorPage } from './pages/FormEditorPage';
import { FormSubmitPage } from './pages/FormSubmitPage';
import { PublicFormSubmitPage } from './pages/PublicFormSubmitPage';
import { FormSubmissionsPage } from './pages/FormSubmissionsPage';
import { ProjectReportsPage } from './pages/ProjectReportsPage';
import { SettingsPage } from './pages/SettingsPage';
import { ProjectsPage } from './pages/ProjectsPage';
import { OrganizationsPage } from './pages/OrganizationsPage';
import { TemplatesPage } from './pages/TemplatesPage';
import { ProtectedRoute } from './components/auth';
import { AppInitializer } from './components/AppInitializer';
import { NetworkIndicator } from './components/common/NetworkIndicator';
import './App.css';

// 建立 MUI 主題
const theme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
  typography: {
    fontFamily: [
      '-apple-system',
      'BlinkMacSystemFont',
      '"Segoe UI"',
      'Roboto',
      '"Helvetica Neue"',
      'Arial',
      'sans-serif',
    ].join(','),
  },
});

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <NetworkIndicator />
      <BrowserRouter>
        <AppInitializer>
          <Routes>
            {/* 系統設定路由 */}
            <Route path="/setup" element={<SetupPage />} />

            {/* 公開路由 */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/public/forms/:formId" element={<PublicFormSubmitPage />} />

          {/* 受保護路由 */}
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <Navigate to="/dashboard" replace />
              </ProtectedRoute>
            }
          />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <DashboardPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/settings"
            element={
              <ProtectedRoute>
                <SettingsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/projects"
            element={
              <ProtectedRoute>
                <ProjectsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/organizations"
            element={
              <ProtectedRoute>
                <OrganizationsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/templates"
            element={
              <ProtectedRoute>
                <TemplatesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/projects/:projectId"
            element={
              <ProtectedRoute>
                <ProjectDetailPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/projects/:projectId/forms/new"
            element={
              <ProtectedRoute>
                <FormEditorPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/projects/:projectId/reports"
            element={
              <ProtectedRoute>
                <ProjectReportsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/projects/:projectId/members"
            element={
              <ProtectedRoute>
                <ProjectMembersPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/projects/:projectId/settings"
            element={
              <ProtectedRoute>
                <ProjectSettingsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/projects/:projectId/forms/:formId/edit"
            element={
              <ProtectedRoute>
                <FormEditorPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/projects/:projectId/forms/:formId/preview"
            element={
              <ProtectedRoute>
                <FormPreviewPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/forms/:formId/submit"
            element={
              <ProtectedRoute>
                <FormSubmitPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/forms/:formId/submissions"
            element={
              <ProtectedRoute>
                <FormSubmissionsPage />
              </ProtectedRoute>
            }
          />
          </Routes>
        </AppInitializer>
      </BrowserRouter>
    </ThemeProvider>
  );
}

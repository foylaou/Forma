/**
 * MainLayout - 主要應用佈局
 * 包含側邊導航欄（年度/計畫選擇器）和頂部工具列
 */

import { useState, useEffect } from 'react';
import {
  Box,
  Drawer,
  AppBar,
  Toolbar,
  Typography,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  IconButton,
  Divider,
  useTheme,
  useMediaQuery,
  FormControl,
  Select,
  MenuItem,
  InputLabel,
  Skeleton,
  Collapse,
  Badge,
} from '@mui/material';
import {
  Menu as MenuIcon,
  Dashboard as DashboardIcon,
  Description as FormsIcon,
  Add as AddIcon,
  ListAlt as ListIcon,
  Assessment as ReportsIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  Folder as ProjectIcon,
  People as MembersIcon,
  Tune as ProjectSettingsIcon,
  ContentCopy as TemplateIcon,
  Business as OrgIcon,
  FolderOpen as AllProjectsIcon,
  AdminPanelSettings as SystemSettingsIcon,
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { UserMenu } from '@/components/auth';
import { useAuthStore } from '@/stores/authStore';
import { useWorkspaceStore } from '@/stores/workspaceStore';
import { projectsApi } from '@/lib/api/projects';

const DRAWER_WIDTH = 280;

interface MainLayoutProps {
  children: React.ReactNode;
  title?: string;
}

export function MainLayout({ children, title }: MainLayoutProps) {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [mobileOpen, setMobileOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useAuthStore();
  const isAdmin = user ? (BigInt(user.permissions ?? 0) & 7n) === 7n : false;

  const {
    selectedYear,
    selectedProjectId,
    selectedProject,
    setYear,
    setProject,
    formsMenuExpanded,
    projectMgmtMenuExpanded,
    setFormsMenuExpanded,
    setProjectMgmtMenuExpanded,
    projects,
    projectsLoadedYear,
    projectsLoading,
    setProjects,
    setProjectsLoading,
  } = useWorkspaceStore();

  // 判斷當前路由是否為計畫管理相關，用於自動展開
  const isProjectMgmtRoute = location.pathname.includes('/members') ||
    (location.pathname.includes('/settings') && location.pathname.includes('/projects/'));

  // 當進入計畫管理相關路由時自動展開
  useEffect(() => {
    if (isProjectMgmtRoute && !projectMgmtMenuExpanded) {
      setProjectMgmtMenuExpanded(true);
    }
  }, [isProjectMgmtRoute]);

  const currentYear = new Date().getFullYear();
  const yearOptions = Array.from({ length: 5 }, (_, i) => currentYear - i);

  // 載入該年度的計畫（只在年度變更或未載入時）
  useEffect(() => {
    if (projectsLoadedYear !== selectedYear) {
      loadProjects();
    }
  }, [selectedYear, projectsLoadedYear]);

  // 當計畫列表載入後，恢復或自動選擇計畫
  useEffect(() => {
    if (projects.length === 0) return;

    // 嘗試恢復之前選擇的計畫
    if (selectedProjectId) {
      const project = projects.find(p => p.id === selectedProjectId);
      if (project && (!selectedProject || selectedProject.id !== project.id)) {
        setProject(project);
        return;
      }
    }

    // 沒有儲存的選擇，自動選第一個計畫
    if (!selectedProject) {
      setProject(projects[0]);
    }
  }, [projects, selectedProjectId]);

  const loadProjects = async () => {
    setProjectsLoading(true);
    try {
      const response = await projectsApi.getProjects({
        year: selectedYear,
        pageSize: 100,
      });
      setProjects(response.items || [], selectedYear);
    } catch (err) {
      console.error('Failed to load projects:', err);
      setProjects([], selectedYear);
    } finally {
      setProjectsLoading(false);
    }
  };

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleNavClick = (path: string) => {
    navigate(path);
    if (isMobile) {
      setMobileOpen(false);
    }
  };

  const handleYearChange = (year: number) => {
    setYear(year);
  };

  const handleProjectChange = (projectId: string) => {
    const project = projects.find(p => p.id === projectId);
    setProject(project || null);
  };

  const drawerContent = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Logo */}
      <Box
        sx={{
          p: 2,
          display: 'flex',
          alignItems: 'center',
          gap: 1,
        }}
      >
        <FormsIcon color="primary" sx={{ fontSize: 32 }} />
        <Typography variant="h6" fontWeight="bold" color="primary">
          Forma
        </Typography>
      </Box>

      <Divider />

      {/* Year & Project Selectors */}
      <Box sx={{ p: 2 }}>
        {/* Year Selector */}
        <FormControl fullWidth size="small" sx={{ mb: 2 }}>
          <InputLabel>年度</InputLabel>
          <Select
            value={selectedYear}
            label="年度"
            onChange={(e) => handleYearChange(e.target.value as number)}
          >
            {yearOptions.map((year) => (
              <MenuItem key={year} value={year}>{year}</MenuItem>
            ))}
          </Select>
        </FormControl>

        {/* Project Selector */}
        <FormControl fullWidth size="small">
          <InputLabel>計畫</InputLabel>
          {projectsLoading ? (
            <Skeleton variant="rounded" height={40} />
          ) : (
            <Select
              value={selectedProjectId || ''}
              label="計畫"
              onChange={(e) => handleProjectChange(e.target.value)}
              displayEmpty
            >
              <MenuItem value="" disabled>
                <em>請選擇計畫</em>
              </MenuItem>
              {projects.map((project) => (
                <MenuItem key={project.id} value={project.id}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <ProjectIcon fontSize="small" color="action" />
                    <Typography noWrap sx={{ maxWidth: 180 }}>
                      {project.name}
                    </Typography>
                  </Box>
                </MenuItem>
              ))}
              {projects.length === 0 && (
                <MenuItem value="" disabled>
                  <em>此年度無計畫</em>
                </MenuItem>
              )}
            </Select>
          )}
        </FormControl>
      </Box>

      <Divider />

      {/* Main Navigation */}
      <List sx={{ flex: 1, px: 1, py: 1 }}>
        {/* Dashboard */}
        <ListItem disablePadding sx={{ mb: 0.5 }}>
          <ListItemButton
            onClick={() => handleNavClick('/dashboard')}
            selected={location.pathname === '/dashboard'}
            sx={{
              borderRadius: 2,
              '&.Mui-selected': {
                backgroundColor: 'primary.main',
                color: 'white',
                '&:hover': { backgroundColor: 'primary.dark' },
                '& .MuiListItemIcon-root': { color: 'white' },
              },
            }}
          >
            <ListItemIcon sx={{ minWidth: 40 }}>
              <DashboardIcon />
            </ListItemIcon>
            <ListItemText primary="儀表板" />
          </ListItemButton>
        </ListItem>

        {/* Forms Section */}
        <ListItem disablePadding sx={{ mb: 0.5 }}>
          <ListItemButton
            onClick={() => setFormsMenuExpanded(!formsMenuExpanded)}
            sx={{ borderRadius: 2 }}
          >
            <ListItemIcon sx={{ minWidth: 40 }}>
              <Badge
                badgeContent={selectedProject ? undefined : '!'}
                color="warning"
                invisible={!!selectedProject}
              >
                <FormsIcon />
              </Badge>
            </ListItemIcon>
            <ListItemText primary="表單管理" />
            {formsMenuExpanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </ListItemButton>
        </ListItem>

        <Collapse in={formsMenuExpanded} timeout="auto" unmountOnExit>
          <List component="div" disablePadding sx={{ pl: 2 }}>
            {/* Create Form */}
            <ListItem disablePadding sx={{ mb: 0.5 }}>
              <ListItemButton
                onClick={() => {
                  if (selectedProject) {
                    handleNavClick(`/projects/${selectedProject.id}/forms/new`);
                  }
                }}
                disabled={!selectedProject}
                sx={{
                  borderRadius: 2,
                  opacity: selectedProject ? 1 : 0.5,
                }}
              >
                <ListItemIcon sx={{ minWidth: 36 }}>
                  <AddIcon fontSize="small" />
                </ListItemIcon>
                <ListItemText
                  primary="建立表單"
                  primaryTypographyProps={{ fontSize: '0.9rem' }}
                />
              </ListItemButton>
            </ListItem>

            {/* Form List */}
            <ListItem disablePadding sx={{ mb: 0.5 }}>
              <ListItemButton
                onClick={() => {
                  if (selectedProject) {
                    handleNavClick(`/projects/${selectedProject.id}`);
                  }
                }}
                disabled={!selectedProject}
                selected={location.pathname.startsWith('/projects/') && location.pathname.split('/').length === 3}
                sx={{
                  borderRadius: 2,
                  opacity: selectedProject ? 1 : 0.5,
                  '&.Mui-selected': {
                    backgroundColor: 'primary.light',
                    '&:hover': { backgroundColor: 'primary.light' },
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: 36 }}>
                  <ListIcon fontSize="small" />
                </ListItemIcon>
                <ListItemText
                  primary="表單列表"
                  primaryTypographyProps={{ fontSize: '0.9rem' }}
                />
              </ListItemButton>
            </ListItem>

            {/* Reports */}
            <ListItem disablePadding sx={{ mb: 0.5 }}>
              <ListItemButton
                onClick={() => {
                  if (selectedProject) {
                    handleNavClick(`/projects/${selectedProject.id}/reports`);
                  }
                }}
                disabled={!selectedProject}
                sx={{
                  borderRadius: 2,
                  opacity: selectedProject ? 1 : 0.5,
                }}
              >
                <ListItemIcon sx={{ minWidth: 36 }}>
                  <ReportsIcon fontSize="small" />
                </ListItemIcon>
                <ListItemText
                  primary="報表統計"
                  primaryTypographyProps={{ fontSize: '0.9rem' }}
                />
              </ListItemButton>
            </ListItem>
          </List>
        </Collapse>

        {/* Project Management Section */}
        <ListItem disablePadding sx={{ mb: 0.5 }}>
          <ListItemButton
            onClick={() => setProjectMgmtMenuExpanded(!projectMgmtMenuExpanded)}
            sx={{ borderRadius: 2 }}
          >
            <ListItemIcon sx={{ minWidth: 40 }}>
              <Badge
                badgeContent={selectedProject ? undefined : '!'}
                color="warning"
                invisible={!!selectedProject}
              >
                <ProjectIcon />
              </Badge>
            </ListItemIcon>
            <ListItemText primary="計畫管理" />
            {projectMgmtMenuExpanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </ListItemButton>
        </ListItem>

        <Collapse in={projectMgmtMenuExpanded} timeout="auto" unmountOnExit>
          <List component="div" disablePadding sx={{ pl: 2 }}>
            {/* Members */}
            <ListItem disablePadding sx={{ mb: 0.5 }}>
              <ListItemButton
                onClick={() => {
                  if (selectedProject) {
                    handleNavClick(`/projects/${selectedProject.id}/members`);
                  }
                }}
                disabled={!selectedProject}
                selected={location.pathname.endsWith('/members')}
                sx={{
                  borderRadius: 2,
                  opacity: selectedProject ? 1 : 0.5,
                  '&.Mui-selected': {
                    backgroundColor: 'primary.light',
                    '&:hover': { backgroundColor: 'primary.light' },
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: 36 }}>
                  <MembersIcon fontSize="small" />
                </ListItemIcon>
                <ListItemText
                  primary="成員管理"
                  primaryTypographyProps={{ fontSize: '0.9rem' }}
                />
              </ListItemButton>
            </ListItem>

            {/* Project Settings */}
            <ListItem disablePadding sx={{ mb: 0.5 }}>
              <ListItemButton
                onClick={() => {
                  if (selectedProject) {
                    handleNavClick(`/projects/${selectedProject.id}/settings`);
                  }
                }}
                disabled={!selectedProject}
                selected={location.pathname.endsWith('/settings') && location.pathname.includes('/projects/')}
                sx={{
                  borderRadius: 2,
                  opacity: selectedProject ? 1 : 0.5,
                  '&.Mui-selected': {
                    backgroundColor: 'primary.light',
                    '&:hover': { backgroundColor: 'primary.light' },
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: 36 }}>
                  <ProjectSettingsIcon fontSize="small" />
                </ListItemIcon>
                <ListItemText
                  primary="計畫設定"
                  primaryTypographyProps={{ fontSize: '0.9rem' }}
                />
              </ListItemButton>
            </ListItem>
          </List>
        </Collapse>

        {/* No Project Selected Hint */}
        {!selectedProject && (
          <Box sx={{ px: 2, py: 1 }}>
            <Typography variant="caption" color="text.secondary">
              請先選擇計畫以使用表單功能
            </Typography>
          </Box>
        )}

        <Divider sx={{ my: 1 }} />

        {/* All Projects */}
        <ListItem disablePadding sx={{ mb: 0.5 }}>
          <ListItemButton
            onClick={() => handleNavClick('/projects')}
            selected={location.pathname === '/projects'}
            sx={{
              borderRadius: 2,
              '&.Mui-selected': {
                backgroundColor: 'primary.main',
                color: 'white',
                '&:hover': { backgroundColor: 'primary.dark' },
                '& .MuiListItemIcon-root': { color: 'white' },
              },
            }}
          >
            <ListItemIcon sx={{ minWidth: 40 }}>
              <AllProjectsIcon />
            </ListItemIcon>
            <ListItemText primary="所有計畫" />
          </ListItemButton>
        </ListItem>

        {/* Templates */}
        <ListItem disablePadding sx={{ mb: 0.5 }}>
          <ListItemButton
            onClick={() => handleNavClick('/templates')}
            selected={location.pathname === '/templates'}
            sx={{
              borderRadius: 2,
              '&.Mui-selected': {
                backgroundColor: 'primary.main',
                color: 'white',
                '&:hover': { backgroundColor: 'primary.dark' },
                '& .MuiListItemIcon-root': { color: 'white' },
              },
            }}
          >
            <ListItemIcon sx={{ minWidth: 40 }}>
              <TemplateIcon />
            </ListItemIcon>
            <ListItemText primary="範本管理" />
          </ListItemButton>
        </ListItem>

        {/* Organizations (admin only) */}
        {isAdmin && (
          <ListItem disablePadding sx={{ mb: 0.5 }}>
            <ListItemButton
              onClick={() => handleNavClick('/organizations')}
              selected={location.pathname === '/organizations'}
              sx={{
                borderRadius: 2,
                '&.Mui-selected': {
                  backgroundColor: 'primary.main',
                  color: 'white',
                  '&:hover': { backgroundColor: 'primary.dark' },
                  '& .MuiListItemIcon-root': { color: 'white' },
                },
              }}
            >
              <ListItemIcon sx={{ minWidth: 40 }}>
                <OrgIcon />
              </ListItemIcon>
              <ListItemText primary="組織管理" />
            </ListItemButton>
          </ListItem>
        )}
      </List>

      <Divider />

      {/* System Settings (admin only) */}
      {isAdmin && (
        <List sx={{ px: 1 }}>
          <ListItem disablePadding>
            <ListItemButton
              onClick={() => handleNavClick('/system-settings')}
              selected={location.pathname.startsWith('/system-settings')}
              sx={{
                borderRadius: 2,
                '&.Mui-selected': {
                  backgroundColor: 'primary.main',
                  color: 'white',
                  '&:hover': { backgroundColor: 'primary.dark' },
                  '& .MuiListItemIcon-root': { color: 'white' },
                },
              }}
            >
              <ListItemIcon sx={{ minWidth: 40 }}>
                <SystemSettingsIcon />
              </ListItemIcon>
              <ListItemText primary="系統設定" />
            </ListItemButton>
          </ListItem>
        </List>
      )}
    </Box>
  );

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      {/* App Bar */}
      <AppBar
        position="fixed"
        color="default"
        elevation={1}
        sx={{
          width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
          ml: { md: `${DRAWER_WIDTH}px` },
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            edge="start"
            onClick={handleDrawerToggle}
            sx={{ mr: 2, display: { md: 'none' } }}
          >
            <MenuIcon />
          </IconButton>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexGrow: 1 }}>
            <Typography variant="h6" component="div">
              {title || ''}
            </Typography>
            {selectedProject && (
              <Typography variant="body2" color="text.secondary">
                - {selectedProject.name}
              </Typography>
            )}
          </Box>

          <UserMenu />
        </Toolbar>
      </AppBar>

      {/* Drawer - Mobile */}
      <Drawer
        variant="temporary"
        open={mobileOpen}
        onClose={handleDrawerToggle}
        ModalProps={{ keepMounted: true }}
        sx={{
          display: { xs: 'block', md: 'none' },
          '& .MuiDrawer-paper': {
            boxSizing: 'border-box',
            width: DRAWER_WIDTH,
          },
        }}
      >
        {drawerContent}
      </Drawer>

      {/* Drawer - Desktop */}
      <Drawer
        variant="permanent"
        sx={{
          display: { xs: 'none', md: 'block' },
          '& .MuiDrawer-paper': {
            boxSizing: 'border-box',
            width: DRAWER_WIDTH,
            borderRight: '1px solid',
            borderColor: 'divider',
          },
        }}
        open
      >
        {drawerContent}
      </Drawer>

      {/* Main Content */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          width: { md: `calc(100% - ${DRAWER_WIDTH}px)` },
          ml: { md: `${DRAWER_WIDTH}px` },
          minHeight: '100vh',
          backgroundColor: 'grey.50',
        }}
      >
        <Toolbar /> {/* Spacer for AppBar */}
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      </Box>
    </Box>
  );
}

export default MainLayout;

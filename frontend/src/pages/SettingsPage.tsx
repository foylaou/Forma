/**
 * SettingsPage - 設定頁面
 * 包含個人資料、組織管理（管理員）、計畫管理（管理員）
 */

import { useState } from 'react';
import {
  Box,
  Tabs,
  Tab,
  Typography,
  Paper,
} from '@mui/material';
import {
  Person as ProfileIcon,
  Business as OrganizationsIcon,
  Folder as ProjectsIcon,
  People as UsersIcon,
  Security as SecurityIcon,
} from '@mui/icons-material';
import { MainLayout } from '@/components/layout';
import { useAuthStore } from '@/stores/authStore';
import { ProfileSettings } from '@/components/settings/ProfileSettings';
import { OrganizationsSettings } from '@/components/settings/OrganizationsSettings';
import { ProjectsSettings } from '@/components/settings/ProjectsSettings';
import { UsersSettings } from '@/components/settings/UsersSettings';
import { SecuritySettings } from '@/components/settings/SecuritySettings';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <div role="tabpanel" hidden={value !== index}>
      {value === index && <Box sx={{ py: 3 }}>{children}</Box>}
    </div>
  );
}

export function SettingsPage() {
  const { user } = useAuthStore();
  const isAdmin = user?.systemRole === 'SystemAdmin';
  const [activeTab, setActiveTab] = useState(0);

  const handleTabChange = (_: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  return (
    <MainLayout title="設定">
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" fontWeight="bold" gutterBottom>
          設定
        </Typography>
        <Typography variant="body1" color="text.secondary">
          管理您的帳號和系統設定
        </Typography>
      </Box>

      <Paper sx={{ borderRadius: 2 }}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs
            value={activeTab}
            onChange={handleTabChange}
            variant="scrollable"
            scrollButtons="auto"
          >
            <Tab
              icon={<ProfileIcon />}
              iconPosition="start"
              label="個人資料"
            />
            {isAdmin && (
              <Tab
                icon={<OrganizationsIcon />}
                iconPosition="start"
                label="組織管理"
              />
            )}
            {isAdmin && (
              <Tab
                icon={<ProjectsIcon />}
                iconPosition="start"
                label="計畫管理"
              />
            )}
            {isAdmin && (
              <Tab
                icon={<UsersIcon />}
                iconPosition="start"
                label="使用者管理"
              />
            )}
            {isAdmin && (
              <Tab
                icon={<SecurityIcon />}
                iconPosition="start"
                label="安全設定"
              />
            )}
          </Tabs>
        </Box>

        <Box sx={{ p: 3 }}>
          <TabPanel value={activeTab} index={0}>
            <ProfileSettings />
          </TabPanel>
          {isAdmin && (
            <TabPanel value={activeTab} index={1}>
              <OrganizationsSettings />
            </TabPanel>
          )}
          {isAdmin && (
            <TabPanel value={activeTab} index={2}>
              <ProjectsSettings />
            </TabPanel>
          )}
          {isAdmin && (
            <TabPanel value={activeTab} index={3}>
              <UsersSettings />
            </TabPanel>
          )}
          {isAdmin && (
            <TabPanel value={activeTab} index={4}>
              <SecuritySettings />
            </TabPanel>
          )}
        </Box>
      </Paper>
    </MainLayout>
  );
}

export default SettingsPage;

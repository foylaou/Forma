/**
 * SystemSettingsPage - 系統設定頁面
 * 左側導覽列，右側內容面板，類似計畫管理的子頁面風格
 */

import { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Divider,
} from '@mui/material';
import {
  Email as EmailIcon,
  Storage as StorageIcon,
  History as ActionLogIcon,
  Security as SecurityIcon,
  People as UsersIcon,
  Description as TemplateIcon,
  AdminPanelSettings as RolesIcon,
  MailOutline as EmailLogIcon,
  Key as KeyIcon,
} from '@mui/icons-material';
import { MainLayout } from '@/components/layout';
import { EmailSettings } from '@/components/system-settings/EmailSettings';
import { FileStorageSettings } from '@/components/system-settings/FileStorageSettings';
import { EmailTemplatesSettings } from '@/components/system-settings/EmailTemplatesSettings';
import { ActionLogsSettings } from '@/components/system-settings/ActionLogsSettings';
import { EmailLogsSettings } from '@/components/system-settings/EmailLogsSettings';
import { RolesSettings } from '@/components/system-settings/RolesSettings';
import { SecuritySettings } from '@/components/settings/SecuritySettings';
import { UsersSettings } from '@/components/settings/UsersSettings';
import { Fido2Settings } from '@/components/system-settings/Fido2Settings';

interface NavItem {
  key: string;
  label: string;
  icon: React.ReactNode;
  group: string;
}

const NAV_ITEMS: NavItem[] = [
  { key: 'email', label: '電子郵件', icon: <EmailIcon />, group: '系統配置' },
  { key: 'file-storage', label: '檔案儲存', icon: <StorageIcon />, group: '系統配置' },
  { key: 'email-templates', label: '信件範本', icon: <TemplateIcon />, group: '系統配置' },
  { key: 'security', label: '安全設定', icon: <SecurityIcon />, group: '系統配置' },
  { key: 'fido2', label: 'Passkey 設定', icon: <KeyIcon />, group: '系統配置' },
  { key: 'users', label: '使用者管理', icon: <UsersIcon />, group: '權限管理' },
  { key: 'roles', label: '角色管理', icon: <RolesIcon />, group: '權限管理' },
  { key: 'action-logs', label: '操作日誌', icon: <ActionLogIcon />, group: '系統日誌' },
  { key: 'email-logs', label: '郵件日誌', icon: <EmailLogIcon />, group: '系統日誌' },
];

const PANELS: Record<string, React.ReactNode> = {
  'email': <EmailSettings />,
  'file-storage': <FileStorageSettings />,
  'email-templates': <EmailTemplatesSettings />,
  'security': <SecuritySettings />,
  'fido2': <Fido2Settings />,
  'users': <UsersSettings />,
  'roles': <RolesSettings />,
  'action-logs': <ActionLogsSettings />,
  'email-logs': <EmailLogsSettings />,
};

export function SystemSettingsPage() {
  const [activeKey, setActiveKey] = useState('email');

  // Group items for rendering with dividers
  const groups: { group: string; items: NavItem[] }[] = [];
  for (const item of NAV_ITEMS) {
    const last = groups[groups.length - 1];
    if (last && last.group === item.group) {
      last.items.push(item);
    } else {
      groups.push({ group: item.group, items: [item] });
    }
  }

  return (
    <MainLayout title="系統設定">
      <Box sx={{ mb: 3 }}>
        <Typography variant="h4" fontWeight="bold" gutterBottom>
          系統設定
        </Typography>
        <Typography variant="body1" color="text.secondary">
          管理系統配置、信件範本、角色權限與系統日誌
        </Typography>
      </Box>

      <Box sx={{ display: 'flex', gap: 3, alignItems: 'flex-start' }}>
        {/* Left Sidebar */}
        <Paper sx={{ width: 240, flexShrink: 0, borderRadius: 2 }}>
          <List disablePadding>
            {groups.map((g, gi) => (
              <Box key={g.group}>
                {gi > 0 && <Divider />}
                <Typography
                  variant="caption"
                  color="text.secondary"
                  fontWeight="bold"
                  sx={{ px: 2, pt: 1.5, pb: 0.5, display: 'block', textTransform: 'uppercase' }}
                >
                  {g.group}
                </Typography>
                {g.items.map((item) => (
                  <ListItemButton
                    key={item.key}
                    selected={activeKey === item.key}
                    onClick={() => setActiveKey(item.key)}
                    sx={{
                      py: 1,
                      '&.Mui-selected': {
                        backgroundColor: 'primary.light',
                        color: 'primary.main',
                        '& .MuiListItemIcon-root': { color: 'primary.main' },
                      },
                    }}
                  >
                    <ListItemIcon sx={{ minWidth: 36 }}>{item.icon}</ListItemIcon>
                    <ListItemText
                      primary={item.label}
                      primaryTypographyProps={{ fontSize: '0.875rem' }}
                    />
                  </ListItemButton>
                ))}
              </Box>
            ))}
          </List>
        </Paper>

        {/* Right Content */}
        <Paper sx={{ flex: 1, p: 3, borderRadius: 2, minHeight: 500 }}>
          {PANELS[activeKey]}
        </Paper>
      </Box>
    </MainLayout>
  );
}

export default SystemSettingsPage;

import { create } from 'zustand';
import { syncOfflineSubmissions, getPendingCount, getConflictSubmissions, clearSyncedSubmissions } from '@/lib/offlineSubmission';
import type { OfflineSubmission } from '@/lib/db';

interface NetworkState {
  isOnline: boolean;
  isSyncing: boolean;
  pendingCount: number;
  conflictCount: number;
  conflicts: OfflineSubmission[];
  lastSyncedAt: number | null;
  refreshPendingCount: () => Promise<void>;
  refreshConflicts: () => Promise<void>;
  sync: () => Promise<void>;
}

export const useNetworkStore = create<NetworkState>((set, get) => ({
  isOnline: navigator.onLine,
  isSyncing: false,
  pendingCount: 0,
  conflictCount: 0,
  conflicts: [],
  lastSyncedAt: null,

  refreshPendingCount: async () => {
    const count = await getPendingCount();
    set({ pendingCount: count });
  },

  refreshConflicts: async () => {
    const conflicts = await getConflictSubmissions();
    set({ conflicts, conflictCount: conflicts.length });
  },

  sync: async () => {
    if (get().isSyncing || !get().isOnline) return;
    set({ isSyncing: true });
    try {
      const synced = await syncOfflineSubmissions();
      await clearSyncedSubmissions();
      const count = await getPendingCount();
      const conflicts = await getConflictSubmissions();
      set({
        pendingCount: count,
        conflictCount: conflicts.length,
        conflicts,
        lastSyncedAt: synced > 0 ? Date.now() : get().lastSyncedAt,
      });
    } finally {
      set({ isSyncing: false });
    }
  },
}));

// 初始化網路事件監聽
function initNetworkListeners() {
  window.addEventListener('online', () => {
    useNetworkStore.setState({ isOnline: true });
    useNetworkStore.getState().sync();
  });
  window.addEventListener('offline', () => {
    useNetworkStore.setState({ isOnline: false });
  });
  // 啟動時檢查待同步數量
  useNetworkStore.getState().refreshPendingCount();
}

initNetworkListeners();

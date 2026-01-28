import db, { type OfflineSubmission } from './db';
import { apiClient } from './api/client';

export async function saveOfflineSubmission(
  formId: string,
  formVersion: string,
  submissionData: string,
  isPublic: boolean,
): Promise<number> {
  const localId = await db.offlineSubmissions.add({
    formId,
    formVersion,
    submissionData,
    isPublic,
    status: 'pending',
    createdAt: Date.now(),
  });
  return localId as number;
}

export async function getPendingSubmissions(): Promise<OfflineSubmission[]> {
  return db.offlineSubmissions.where('status').equals('pending').toArray();
}

export async function getPendingCount(): Promise<number> {
  return db.offlineSubmissions.where('status').anyOf('pending', 'failed', 'version_conflict').count();
}

export async function getConflictSubmissions(): Promise<OfflineSubmission[]> {
  return db.offlineSubmissions.where('status').equals('version_conflict').toArray();
}

/**
 * 同步離線提交。
 * 若伺服器表單版本與離線時不同，標記為 version_conflict 而非直接送出。
 * 傳入 force=true 可強制送出（忽略版本差異）。
 */
export async function syncOfflineSubmissions(force = false): Promise<number> {
  const pending = await getPendingSubmissions();
  let synced = 0;

  // 批次取得各表單最新版本（去重）
  const formIds = [...new Set(pending.map((s) => s.formId))];
  const serverVersions: Record<string, string> = {};

  if (!force) {
    for (const fid of formIds) {
      try {
        const form = await apiClient.get<{ version: string }>(`/forms/${fid}`);
        serverVersions[fid] = form.version;
      } catch {
        // 無法取得版本時（404 等），讓提交繼續嘗試，由伺服器決定
      }
    }
  }

  for (const sub of pending) {
    try {
      // 版本衝突檢測
      if (!force && serverVersions[sub.formId] && serverVersions[sub.formId] !== sub.formVersion) {
        await db.offlineSubmissions.update(sub.localId!, {
          status: 'version_conflict',
          error: `表單版本已從 ${sub.formVersion} 更新為 ${serverVersions[sub.formId]}`,
        });
        continue;
      }

      await db.offlineSubmissions.update(sub.localId!, { status: 'syncing' });

      if (sub.isPublic) {
        await apiClient.postPublic(`/public/forms/${sub.formId}/submissions`, {
          submissionData: sub.submissionData,
        });
      } else {
        await apiClient.post('/submissions', {
          formId: sub.formId,
          submissionData: sub.submissionData,
        });
      }

      await db.offlineSubmissions.update(sub.localId!, { status: 'synced' });
      synced++;
    } catch (err) {
      await db.offlineSubmissions.update(sub.localId!, {
        status: 'failed',
        error: err instanceof Error ? err.message : String(err),
      });
    }
  }

  return synced;
}

/** 強制送出版本衝突的提交 */
export async function forceSubmitConflicts(): Promise<number> {
  const conflicts = await getConflictSubmissions();
  // 將它們重設為 pending 再送出
  for (const sub of conflicts) {
    await db.offlineSubmissions.update(sub.localId!, { status: 'pending' });
  }
  return syncOfflineSubmissions(true);
}

/** 丟棄版本衝突的提交 */
export async function discardConflicts(): Promise<void> {
  await db.offlineSubmissions.where('status').equals('version_conflict').delete();
}

export async function clearSyncedSubmissions(): Promise<void> {
  await db.offlineSubmissions.where('status').equals('synced').delete();
}

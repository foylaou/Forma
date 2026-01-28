// 系統日誌 API

import { apiClient } from './client';
import type {
  ActionLogDto,
  ActionLogQueryParams,
  ActionLogStatisticsDto,
  PagedResult,
} from '@/types/api';

export const logsApi = {
  // 查詢操作日誌
  getActionLogs: (params?: ActionLogQueryParams) =>
    apiClient.get<PagedResult<ActionLogDto>>('/logs/action', params as Record<string, string | number | boolean | undefined>),

  // 取得操作日誌統計
  getActionLogStatistics: (params?: { startDate?: string; endDate?: string }) =>
    apiClient.get<ActionLogStatisticsDto>('/logs/action/statistics', params as Record<string, string | number | boolean | undefined>),

  // 取得單筆操作日誌
  getActionLog: (id: number) =>
    apiClient.get<ActionLogDto>(`/logs/action/${id}`),
};

export default logsApi;

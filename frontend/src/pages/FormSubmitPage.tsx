/**
 * FormSubmitPage - 表單填寫頁面（需登入）
 */

import { useState, useEffect } from 'react';
import {
  Box,
  CircularProgress,
  Container,
  Alert,
  Button,
} from '@mui/material';
import { useParams, useNavigate } from 'react-router-dom';
import { FormSubmitContent } from '@/components/form-submit';
import { formsApi } from '@/lib/api/forms';
import { submissionsApi } from '@/lib/api/submissions';
import { cacheForm, getCachedForm } from '@/lib/formCache';
import type { FormDto } from '@/types/api/forms';
import type { FormSchema } from '@/types/form';

export function FormSubmitPage() {
  const { formId } = useParams<{ formId: string }>();
  const navigate = useNavigate();

  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState<FormDto | null>(null);
  const [schema, setSchema] = useState<FormSchema | null>(null);
  const [submitted, setSubmitted] = useState(false);
  const [isOfflineCached, setIsOfflineCached] = useState(false);

  useEffect(() => {
    if (formId) loadForm();
  }, [formId]);

  const loadForm = async () => {
    if (!formId) return;
    console.log('[LoadForm] 開始載入表單', { formId, online: navigator.onLine });
    setLoading(true);
    setError(null);

    try {
      const formData = await formsApi.getForm(formId);
      console.log('[LoadForm] 表單載入成功', {
        formId: formData.id,
        name: formData.name,
        version: formData.version,
        isActive: formData.isActive,
        publishedAt: formData.publishedAt,
      });
      setForm(formData);

      // 快取表單供離線使用
      await cacheForm(formData);
      console.log('[LoadForm] 表單已快取至 IndexedDB');

      if (!formData.publishedAt) { setError('此表單尚未發布'); return; }
      if (!formData.isActive) { setError('此表單已停用'); return; }

      try {
        setSchema(JSON.parse(formData.schema));
      } catch {
        setError('表單結構解析失敗');
      }
    } catch (err) {
      const errType = err?.constructor?.name ?? typeof err;
      const errMsg = err instanceof Error ? err.message : String(err);
      console.error('[LoadForm] 表單載入失敗', {
        type: errType,
        message: errMsg,
        online: navigator.onLine,
        stack: err instanceof Error ? err.stack : undefined,
      });

      // 離線時嘗試從 IndexedDB 載入快取
      const cached = await getCachedForm(formId);
      console.log('[LoadForm] IndexedDB 快取查詢', { found: !!cached, formId });
      if (cached) {
        setForm(cached);
        setIsOfflineCached(true);
        console.log('[LoadForm] 使用離線快取', { version: cached.version });
        try {
          setSchema(JSON.parse(cached.schema));
        } catch {
          setError('快取表單結構解析失敗');
        }
      } else {
        const msg = err instanceof Error ? err.message : '';
        const isConnectError = err instanceof TypeError || !navigator.onLine;
        setError(
          isConnectError
            ? '無法連線到伺服器（可能處於離線狀態或不在內網環境），且本機無此表單的快取資料。請先在可連線的環境下開啟此表單一次。'
            : msg || '載入表單失敗',
        );
      }
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (data: Record<string, unknown>) => {
    if (!formId) return;
    setSubmitting(true);
    try {
      const result = await submissionsApi.createSubmission({
        formId,
        submissionData: JSON.stringify(data),
        isDraft: false,
      }, form?.version);
      setSubmitted(true);
      if (result.offline) {
        setError(null); // 清除可能的錯誤，離線提交成功
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleSaveDraft = async (data: Record<string, unknown>) => {
    if (!formId) return;
    setSubmitting(true);
    try {
      await submissionsApi.createSubmission({
        formId,
        submissionData: JSON.stringify(data),
        isDraft: true,
      }, form?.version);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', backgroundColor: 'grey.100' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box sx={{ minHeight: '100vh', backgroundColor: 'grey.100' }}>
        <Container maxWidth="sm" sx={{ py: 8 }}>
          <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>
          <Button variant="contained" onClick={() => navigate(-1)}>返回</Button>
        </Container>
      </Box>
    );
  }

  if (!schema || !form) return null;

  return (
    <>
      {isOfflineCached && (
        <Alert severity="info" sx={{ borderRadius: 0 }}>
          目前使用離線快取的表單（版本 {form.version}），提交後將在恢復連線時自動同步。
        </Alert>
      )}
      <FormSubmitContent
        form={form}
        schema={schema}
        showAppBar
        allowDraft
        onSubmit={handleSubmit}
        onSaveDraft={handleSaveDraft}
        onClose={() => navigate(-1)}
        submitting={submitting}
        submitted={submitted}
      />
    </>
  );
}

export default FormSubmitPage;

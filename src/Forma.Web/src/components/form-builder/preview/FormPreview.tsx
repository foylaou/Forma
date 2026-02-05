/**
 * FormPreview - Preview mode for the form
 */

import { useState, useEffect } from 'react';
import { Box, Paper, Typography, Button, Divider, LinearProgress } from '@mui/material';
import { Close as CloseIcon } from '@mui/icons-material';
import { useForm, FormProvider, useWatch } from 'react-hook-form';
import { FieldRenderer } from '@/components/form-fields';
import { useFormBuilderStore, useActivePageFields, useActivePage } from '@/stores/formBuilderStore';

// Progress tracker component
function ProgressTracker({ fields, control }: { fields: { name: string; required?: boolean }[]; control: unknown }) {
  const [progress, setProgress] = useState(0);
  const [filledCount, setFilledCount] = useState(0);
  const values = useWatch({ control: control as never });

  useEffect(() => {
    if (fields.length === 0) {
      setProgress(100); // No fields means "complete"
      setFilledCount(0);
      return;
    }

    // Count filled fields
    let filled = 0;
    fields.forEach((field) => {
      const value = values?.[field.name];
      if (value !== undefined && value !== null && value !== '' &&
          !(Array.isArray(value) && value.length === 0)) {
        filled++;
      }
    });

    setFilledCount(filled);
    setProgress(Math.round((filled / fields.length) * 100));
  }, [values, fields]);

  return (
    <Box sx={{ px: 2, py: 1.5, backgroundColor: 'grey.100', borderBottom: 1, borderColor: 'divider' }}>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 0.5 }}>
        <Typography variant="body2" color="text.secondary">
          填寫進度
        </Typography>
        <Typography variant="body2" fontWeight="medium" color="primary.main">
          {fields.length === 0 ? '無欄位' : `${filledCount}/${fields.length} (${progress}%)`}
        </Typography>
      </Box>
      <LinearProgress
        variant="determinate"
        value={progress}
        sx={{
          height: 8,
          borderRadius: 4,
          backgroundColor: 'grey.300',
          '& .MuiLinearProgress-bar': {
            borderRadius: 4,
          }
        }}
      />
    </Box>
  );
}

export function FormPreview() {
  const { togglePreviewMode, schema } = useFormBuilderStore();
  const fields = useActivePageFields();
  const activePage = useActivePage();

  // Filter out non-input fields (like panel, html, section)
  const inputFields = fields.filter(f =>
    !['panel', 'paneldynamic', 'html', 'section', 'hidden'].includes(f.type)
  );

  const showProgressBar = schema.settings?.showProgressBar ?? true;

  const methods = useForm({
    mode: 'onChange',
  });

  const handleSubmit = (data: Record<string, unknown>) => {
    console.log('Preview form data:', data);
    alert('表單資料已記錄到 Console');
  };

  return (
    <Box
      sx={{
        position: 'fixed',
        inset: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1300,
        p: 4,
      }}
    >
      <Paper
        sx={{
          width: '100%',
          maxWidth: 800,
          maxHeight: '90vh',
          display: 'flex',
          flexDirection: 'column',
          overflow: 'hidden',
        }}
      >
        {/* Header */}
        <Box
          sx={{
            p: 2,
            borderBottom: 1,
            borderColor: 'divider',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            backgroundColor: 'primary.main',
            color: 'white',
          }}
        >
          <Box>
            <Typography variant="h6" fontWeight="bold">
              表單預覽
            </Typography>
            <Typography variant="body2" sx={{ opacity: 0.8 }}>
              {schema.metadata.title}
            </Typography>
          </Box>
          <Button
            variant="contained"
            color="inherit"
            startIcon={<CloseIcon />}
            onClick={togglePreviewMode}
            sx={{ color: 'primary.main' }}
          >
            關閉預覽
          </Button>
        </Box>

        {/* Progress Bar */}
        {showProgressBar && (
          <ProgressTracker fields={inputFields} control={methods.control} />
        )}

        {/* Content */}
        <Box sx={{ flex: 1, overflow: 'auto', p: 3 }}>
          {/* Form Metadata */}
          {schema.metadata.description && (
            <Box sx={{ mb: 3 }}>
              <Typography variant="body1" color="text.secondary">
                {schema.metadata.description}
              </Typography>
            </Box>
          )}

          {/* Page Title */}
          {activePage?.title && (
            <Box sx={{ mb: 3 }}>
              <Typography variant="h5" gutterBottom>
                {activePage.title}
              </Typography>
              {activePage.description && (
                <Typography variant="body2" color="text.secondary">
                  {activePage.description}
                </Typography>
              )}
              <Divider sx={{ mt: 2 }} />
            </Box>
          )}

          {/* Form Fields */}
          <FormProvider {...methods}>
            <form onSubmit={methods.handleSubmit(handleSubmit)}>
              {fields.length === 0 ? (
                <Box
                  sx={{
                    py: 8,
                    textAlign: 'center',
                    border: '2px dashed',
                    borderColor: 'divider',
                    borderRadius: 2,
                  }}
                >
                  <Typography variant="body1" color="text.secondary">
                    此頁面尚無欄位
                  </Typography>
                </Box>
              ) : (
                <>
                  {fields.map((field) => (
                    <Box key={field.id} sx={{ mb: 3 }}>
                      <FieldRenderer field={field} />
                    </Box>
                  ))}

                  {/* Submit Button */}
                  <Box sx={{ mt: 4, pt: 2, borderTop: 1, borderColor: 'divider' }}>
                    <Button type="submit" variant="contained" size="large">
                      {schema.metadata.submitButtonText || '提交'}
                    </Button>
                  </Box>
                </>
              )}
            </form>
          </FormProvider>
        </Box>
      </Paper>
    </Box>
  );
}

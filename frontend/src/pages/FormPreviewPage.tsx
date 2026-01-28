/**
 * FormPreviewPage - Standalone page for form preview
 * Supports multi-page navigation and card mode for mobile
 */

import { useState, useEffect } from 'react';
import {
  Box,
  Paper,
  Typography,
  Button,
  Divider,
  AppBar,
  Toolbar,
  Container,
  LinearProgress,
  Stepper,
  Step,
  StepLabel,
  ToggleButton,
  ToggleButtonGroup,
  Tooltip,
} from '@mui/material';
import {
  Edit as EditIcon,
  NavigateNext as NextIcon,
  NavigateBefore as PrevIcon,
  Smartphone as PhoneIcon,
  Computer as DesktopIcon,
} from '@mui/icons-material';
import { useForm, FormProvider, useWatch } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router-dom';
import { FieldRenderer } from '@/components/form-fields';
import { CardModePreview } from '@/components/form-preview';
import { UserMenu } from '@/components/auth';
import { useFormBuilderStore } from '@/stores/formBuilderStore';
import type { Field, FormSchema, PageNavigationRule, ConditionalOperator } from '@/types/form';
import type { ResolvedTheme } from '@/hooks/useFormTheme';
import { useFormTheme, parseProjectTheme } from '@/hooks/useFormTheme';
import { projectsApi } from '@/lib/api/projects';

// Evaluate a navigation rule condition
function evaluateCondition(
  fieldValue: unknown,
  operator: ConditionalOperator,
  ruleValue: string | number | boolean
): boolean {
  const strValue = String(fieldValue ?? '');
  const numValue = Number(fieldValue);
  const ruleStrValue = String(ruleValue);
  const ruleNumValue = Number(ruleValue);

  switch (operator) {
    case 'equals':
      return strValue === ruleStrValue;
    case 'notEquals':
      return strValue !== ruleStrValue;
    case 'contains':
      return strValue.includes(ruleStrValue);
    case 'notContains':
      return !strValue.includes(ruleStrValue);
    case 'gt':
      return !isNaN(numValue) && numValue > ruleNumValue;
    case 'gte':
      return !isNaN(numValue) && numValue >= ruleNumValue;
    case 'lt':
      return !isNaN(numValue) && numValue < ruleNumValue;
    case 'lte':
      return !isNaN(numValue) && numValue <= ruleNumValue;
    case 'isEmpty':
      return strValue === '' || fieldValue === null || fieldValue === undefined;
    case 'isNotEmpty':
      return strValue !== '' && fieldValue !== null && fieldValue !== undefined;
    case 'in':
      return ruleStrValue.split(',').map(s => s.trim()).includes(strValue);
    case 'notIn':
      return !ruleStrValue.split(',').map(s => s.trim()).includes(strValue);
    case 'startsWith':
      return strValue.startsWith(ruleStrValue);
    case 'endsWith':
      return strValue.endsWith(ruleStrValue);
    default:
      return false;
  }
}

// Progress tracker component
function ProgressTracker({ fields, control }: { fields: Field[]; control: unknown }) {
  const [progress, setProgress] = useState(0);
  const [filledCount, setFilledCount] = useState(0);
  const values = useWatch({ control: control as never });

  useEffect(() => {
    if (fields.length === 0) {
      setProgress(100);
      setFilledCount(0);
      return;
    }

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
    <Box sx={{ mb: 3 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
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

// Classic mode content component
function ClassicModeContent({
  schema,
  currentPageIndex,
  setCurrentPageIndex,
  allFieldsRequired,
  resolvedTheme,
}: {
  schema: FormSchema;
  currentPageIndex: number;
  setCurrentPageIndex: (index: number) => void;
  allFieldsRequired: boolean;
  resolvedTheme: ResolvedTheme;
}) {
  const navigate = useNavigate();
  const totalPages = schema.pages.length;
  const currentPage = schema.pages[currentPageIndex];
  const isFirstPage = currentPageIndex === 0;
  const isLastPage = currentPageIndex === totalPages - 1;

  const allFields = schema.pages.flatMap((page) => page.fields);
  const inputFields = allFields.filter(f =>
    !['panel', 'paneldynamic', 'html', 'section', 'hidden'].includes(f.type)
  );

  const showProgressBar = schema.settings?.showProgressBar ?? true;

  const methods = useForm({
    mode: 'onChange',
  });

  const handleSubmit = (data: Record<string, unknown>) => {
    console.log('Form submitted:', data);
    alert('表單提交成功！資料已記錄到 Console');
  };

  const handleGoToEdit = () => {
    navigate(-1);
  };

  const canGoPrevious = !isFirstPage && (currentPage?.allowPrevious ?? true);

  const handlePrevPage = () => {
    if (canGoPrevious) {
      setCurrentPageIndex(currentPageIndex - 1);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  };

  const formValues = methods.watch();

  const findTargetPageIndex = (rules: PageNavigationRule[] | undefined): number | null => {
    if (!rules || rules.length === 0) return null;

    for (const rule of rules) {
      const fieldValue = formValues[rule.fieldName];
      if (evaluateCondition(fieldValue, rule.operator, rule.value)) {
        const targetIndex = schema.pages.findIndex(p => p.id === rule.targetPageId);
        if (targetIndex !== -1) {
          return targetIndex;
        }
      }
    }
    return null;
  };

  const handleNextPage = () => {
    if (!isLastPage) {
      const targetIndex = findTargetPageIndex(currentPage?.navigationRules);
      if (targetIndex !== null) {
        setCurrentPageIndex(targetIndex);
      } else {
        setCurrentPageIndex(currentPageIndex + 1);
      }
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  };

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Paper sx={{ p: 4 }}>
        {/* Logo */}
        {resolvedTheme.logo && (
          <Box sx={{ mb: 2, backgroundColor: resolvedTheme.logoBackgroundColor, p: 0.5, borderRadius: `${resolvedTheme.cardBorderRadius ?? 8}px`, display: 'inline-block' }}>
            <Box
              component="img"
              src={resolvedTheme.logo}
              alt="Logo"
              sx={{
                maxWidth: 200,
                maxHeight: 60,
                objectFit: 'contain',
              }}
            />
          </Box>
        )}

        {/* Form Title */}
        <Box sx={{ mb: 4, textAlign: 'center' }}>
          <Typography variant="h4" gutterBottom fontWeight="bold">
            {schema.metadata.title || '表單預覽'}
          </Typography>
          {schema.metadata.description && (
            <Typography variant="body1" color="text.secondary">
              {schema.metadata.description}
            </Typography>
          )}
        </Box>

        <Divider sx={{ mb: 4 }} />

        {/* Page Stepper */}
        {totalPages > 1 && (
          <Box sx={{ mb: 4 }}>
            <Stepper activeStep={currentPageIndex} alternativeLabel>
              {schema.pages.map((page, index) => (
                <Step key={page.id}>
                  <StepLabel
                    sx={{ cursor: 'pointer' }}
                    onClick={() => {
                      setCurrentPageIndex(index);
                      window.scrollTo({ top: 0, behavior: 'smooth' });
                    }}
                  >
                    {page.title || `第 ${index + 1} 頁`}
                  </StepLabel>
                </Step>
              ))}
            </Stepper>
          </Box>
        )}

        {/* Progress Bar */}
        {showProgressBar && (
          <ProgressTracker fields={inputFields} control={methods.control} />
        )}

        {/* Form Fields */}
        <FormProvider {...methods}>
          <div>
            {currentPage && (
              <Box>
                {/* Page Title */}
                {totalPages > 1 && (
                  <Box sx={{ mb: 3 }}>
                    <Typography variant="h5" gutterBottom>
                      {currentPage.title || `第 ${currentPageIndex + 1} 頁`}
                    </Typography>
                    {currentPage.description && (
                      <Typography variant="body2" color="text.secondary">
                        {currentPage.description}
                      </Typography>
                    )}
                    <Divider sx={{ mt: 2 }} />
                  </Box>
                )}

                {/* Page Fields */}
                {currentPage.fields.length === 0 ? (
                  <Box
                    sx={{
                      py: 4,
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
                  currentPage.fields.map((field) => (
                    <Box key={field.id} sx={{ mb: 3 }}>
                      <FieldRenderer field={field} allFieldsRequired={allFieldsRequired} />
                    </Box>
                  ))
                )}
              </Box>
            )}

            {/* Navigation Buttons */}
            <Box
              sx={{
                mt: 4,
                pt: 3,
                borderTop: 1,
                borderColor: 'divider',
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
              }}
            >
              <Button
                type="button"
                variant="outlined"
                startIcon={<PrevIcon />}
                onClick={handlePrevPage}
                disabled={!canGoPrevious}
                sx={{ visibility: isFirstPage ? 'hidden' : 'visible' }}
              >
                上一頁
              </Button>

              {totalPages > 1 && (
                <Typography variant="body2" color="text.secondary">
                  {currentPageIndex + 1} / {totalPages}
                </Typography>
              )}

              {isLastPage ? (
                <Button
                  type="button"
                  variant="contained"
                  size="large"
                  sx={{ minWidth: 120 }}
                  onClick={() => methods.handleSubmit(handleSubmit)()}
                >
                  {schema.metadata.submitButtonText || '提交'}
                </Button>
              ) : (
                <Button
                  type="button"
                  variant="contained"
                  endIcon={<NextIcon />}
                  onClick={handleNextPage}
                >
                  下一頁
                </Button>
              )}
            </Box>
          </div>
        </FormProvider>

        {/* Empty State */}
        {allFields.length === 0 && (
          <Box
            sx={{
              py: 8,
              textAlign: 'center',
            }}
          >
            <Typography variant="h6" color="text.secondary" gutterBottom>
              表單尚未建立任何欄位
            </Typography>
            <Typography variant="body2" color="text.disabled" gutterBottom>
              請先到編輯頁面新增欄位
            </Typography>
            <Button
              variant="contained"
              startIcon={<EditIcon />}
              onClick={handleGoToEdit}
              sx={{ mt: 2 }}
            >
              前往編輯
            </Button>
          </Box>
        )}
      </Paper>

      {/* Footer */}
      <Box sx={{ mt: 4, textAlign: 'center' }}>
        <Typography variant="caption" color="text.disabled">
          由 Forma 表單建構器產生
        </Typography>
      </Box>
    </Container>
  );
}

export function FormPreviewPage() {
  const navigate = useNavigate();
  const { projectId } = useParams<{ projectId: string }>();
  const { schema } = useFormBuilderStore();
  const [currentPageIndex, setCurrentPageIndex] = useState(0);
  const [projectSettings, setProjectSettings] = useState<string | undefined>();

  useEffect(() => {
    if (projectId) {
      projectsApi.getProject(projectId).then((p) => {
        setProjectSettings(p.settings ?? undefined);
      }).catch(() => {});
    }
  }, [projectId]);

  const projectTheme = parseProjectTheme(projectSettings);
  const resolvedTheme = useFormTheme(projectTheme, schema.settings);

  const [displayMode, setDisplayMode] = useState<'classic' | 'card'>(() => {
    const defaultMode = schema.settings?.displayMode?.default;
    // 'hybrid' 模式預設使用 'classic'
    return defaultMode === 'card' ? 'card' : 'classic';
  });

  const allFieldsRequired = schema.settings?.allFieldsRequired ?? false;

  const handleDisplayModeChange = (
    _: React.MouseEvent<HTMLElement>,
    newMode: 'classic' | 'card' | null
  ) => {
    if (newMode !== null) {
      setDisplayMode(newMode);
    }
  };

  const handleGoToEdit = () => {
    navigate(-1);
  };

  const handleCardSubmit = (data: Record<string, unknown>) => {
    console.log('Form submitted:', data);
    alert('表單提交成功！資料已記錄到 Console');
  };

  return (
    <Box sx={{ minHeight: '100vh', backgroundColor: resolvedTheme.backgroundColor || schema.settings?.backgroundColor || 'grey.100', display: 'flex', flexDirection: 'column' }}>
      {/* Top Bar */}
      <AppBar position="static" color="primary" elevation={1}>
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            表單預覽
          </Typography>

          <Button
            variant="contained"
            color="inherit"
            startIcon={<EditIcon />}
            onClick={handleGoToEdit}
            sx={{ color: 'primary.main' }}
          >
            返回編輯
          </Button>

          {/* User Menu */}
          <UserMenu />
        </Toolbar>
      </AppBar>

      {/* Main Content */}
      <Box
        sx={{
          flex: 1,
          position: 'relative',
          minHeight: 0,
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        {displayMode === 'classic' ? (
          <ClassicModeContent
            schema={schema}
            currentPageIndex={currentPageIndex}
            setCurrentPageIndex={setCurrentPageIndex}
            allFieldsRequired={allFieldsRequired}
            resolvedTheme={resolvedTheme}
          />
        ) : (
          <Box sx={{ position: 'absolute', inset: 0 }}>
            <CardModePreview
              schema={schema}
              onSubmit={handleCardSubmit}
              resolvedTheme={resolvedTheme}
            />
          </Box>
        )}
      </Box>

      {/* Device Toggle - Fixed at bottom */}
      <Box
        sx={{
          position: 'fixed',
          bottom: 16,
          left: '50%',
          transform: 'translateX(-50%)',
          zIndex: 1000,
        }}
      >
        <Paper
          elevation={4}
          sx={{
            borderRadius: 3,
            overflow: 'hidden',
          }}
        >
          <ToggleButtonGroup
            value={displayMode}
            exclusive
            onChange={handleDisplayModeChange}
            size="small"
          >
            <ToggleButton value="card" aria-label="手機模式">
              <Tooltip title="卡片模式 (手機)">
                <PhoneIcon />
              </Tooltip>
            </ToggleButton>
            <ToggleButton value="classic" aria-label="桌面模式">
              <Tooltip title="經典模式 (桌面)">
                <DesktopIcon />
              </Tooltip>
            </ToggleButton>
          </ToggleButtonGroup>
        </Paper>
      </Box>
    </Box>
  );
}

export default FormPreviewPage;

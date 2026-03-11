import { Typography, Box } from '@mui/material';
import { useTranslation } from 'react-i18next';

export default function Dashboard() {
  const { t } = useTranslation();

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        {t('dashboard.title')}
      </Typography>
      <Typography color="text.secondary">
        {t('dashboard.welcome')}
      </Typography>
    </Box>
  );
}
import { Typography, Box } from '@mui/material';

export default function Dashboard() {
  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>
      <Typography color="text.secondary">
        Welcome to Autoverleih admin. Select a section from the sidebar.
      </Typography>
    </Box>
  );
}

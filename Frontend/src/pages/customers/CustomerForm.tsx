import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Box,
  Button,
  TextField,
  Typography,
  Alert,
  CircularProgress,
} from '@mui/material';
import axios from 'axios';
import { customerService } from '../../api/customerService';
import type { CreateCustomerRequest } from '../../types/customer';

export default function CustomerForm() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const [form, setForm] = useState<CreateCustomerRequest>({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
  });
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    customerService
      .getById(id)
      .then((customer) => {
        setForm({
          firstName: customer.firstName,
          lastName: customer.lastName,
          email: customer.email,
          phoneNumber: customer.phoneNumber ?? '',
        });
      })
      .catch(() => setError('Failed to load customer'))
      .finally(() => setLoading(false));
  }, [id]);

  const handleChange = (field: keyof CreateCustomerRequest) =>
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setForm((prev) => ({ ...prev, [field]: e.target.value }));
    };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);

    const payload = {
      ...form,
      phoneNumber: form.phoneNumber || undefined,
    };

    try {
      if (isEdit) {
        await customerService.update(id!, payload);
      } else {
        await customerService.create(payload);
      }
      navigate('/customers');
    } catch (err: unknown) {
      if (axios.isAxiosError(err) && err.response?.data) {
        const data = err.response.data;
        setError(typeof data === 'string' ? data : data.error ?? data.title ?? 'Failed to save customer');
      } else {
        setError('Failed to save customer');
      }
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ maxWidth: 600 }}>
      <Typography variant="h4" gutterBottom>
        {isEdit ? 'Edit Customer' : 'New Customer'}
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Box component="form" onSubmit={handleSubmit} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <TextField
          label="First Name"
          value={form.firstName}
          onChange={handleChange('firstName')}
          required
        />
        <TextField
          label="Last Name"
          value={form.lastName}
          onChange={handleChange('lastName')}
          required
        />
        <TextField
          label="Email"
          type="email"
          value={form.email}
          onChange={handleChange('email')}
          required
        />
        <TextField
          label="Phone Number"
          value={form.phoneNumber}
          onChange={handleChange('phoneNumber')}
        />
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button type="submit" variant="contained" disabled={saving}>
            {saving ? 'Saving...' : isEdit ? 'Update' : 'Create'}
          </Button>
          <Button variant="outlined" onClick={() => navigate('/customers')}>
            Cancel
          </Button>
        </Box>
      </Box>
    </Box>
  );
}

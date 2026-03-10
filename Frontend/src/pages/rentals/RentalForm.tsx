import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Box,
  Button,
  TextField,
  Typography,
  Alert,
  CircularProgress,
  MenuItem,
} from "@mui/material";
import axios from "axios";
import { rentalService } from "../../api/rentalService";
import { customerService } from "../../api/customerService";
import { carService } from "../../api/carService";
import type { CreateRentalRequest } from "../../types/rental";
import type { CustomerResponse } from "../../types/customer";
import type { CarResponse } from "../../types/car";

export default function RentalForm() {
  const navigate = useNavigate();

  const [customers, setCustomers] = useState<CustomerResponse[]>([]);
  const [cars, setCars] = useState<CarResponse[]>([]);
  const [form, setForm] = useState<CreateRentalRequest>({
    customerId: "",
    carId: "",
    startDate: new Date().toISOString().slice(0, 10),
    endDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000)
      .toISOString()
      .slice(0, 10),
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setLoading(true);
    Promise.all([customerService.getAll(), carService.getAll()])
      .then(([custs, carsData]) => {
        setCustomers(custs);
        setCars(carsData);
      })
      .catch(() => setError("Failed to load customers or cars"))
      .finally(() => setLoading(false));
  }, []);

  const handleChange =
    (field: keyof CreateRentalRequest) =>
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setForm((prev) => ({ ...prev, [field]: e.target.value }));
    };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);

    try {
      await rentalService.create(form);
      navigate("/rentals");
    } catch (err: unknown) {
      if (axios.isAxiosError(err) && err.response?.data) {
        const data = err.response.data;
        setError(
          typeof data === "string"
            ? data
            : (data.error ?? data.title ?? "Failed to create rental"),
        );
      } else {
        setError("Failed to create rental");
      }
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", mt: 4 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ maxWidth: 600 }}>
      <Typography variant="h4" gutterBottom>
        New Rental
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Box
        component="form"
        onSubmit={handleSubmit}
        sx={{ display: "flex", flexDirection: "column", gap: 2 }}
      >
        <TextField
          select
          label="Customer"
          value={form.customerId}
          onChange={handleChange("customerId")}
          required
        >
          {customers.map((c) => (
            <MenuItem key={c.id} value={c.id}>
              {c.firstName} {c.lastName}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          label="Car"
          value={form.carId}
          onChange={handleChange("carId")}
          required
        >
          {cars.map((c) => (
            <MenuItem key={c.id} value={c.id} disabled={!c.isAvailable}>
              {c.make} {c.model} ({c.licensePlate})
            </MenuItem>
          ))}
        </TextField>
        <TextField
          label="Start Date"
          type="date"
          value={form.startDate}
          onChange={handleChange("startDate")}
          InputLabelProps={{ shrink: true }}
          required
        />
        <TextField
          label="End Date"
          type="date"
          value={form.endDate}
          onChange={handleChange("endDate")}
          InputLabelProps={{ shrink: true }}
          required
        />
        <Box sx={{ display: "flex", gap: 2 }}>
          <Button type="submit" variant="contained" disabled={saving}>
            {saving ? "Saving..." : "Create"}
          </Button>
          <Button variant="outlined" onClick={() => navigate("/rentals")}>
            Cancel
          </Button>
        </Box>
      </Box>
    </Box>
  );
}

import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  Box,
  Button,
  TextField,
  Typography,
  Alert,
  CircularProgress,
} from "@mui/material";
import axios from "axios";
import { useTranslation } from "react-i18next";
import { carService } from "../../api/carService";
import type { CreateCarRequest } from "../../types/car";

export default function CarForm() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const isEdit = Boolean(id);

  const [form, setForm] = useState<CreateCarRequest>({
    make: "",
    model: "",
    licensePlate: "",
    year: new Date().getFullYear(),
  });
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    carService
      .getById(id)
      .then((car) => {
        setForm({
          make: car.make,
          model: car.model,
          licensePlate: car.licensePlate,
          year: car.year,
        });
      })
      .catch(() => setError(t("cars.failedToLoadOne")))
      .finally(() => setLoading(false));
  }, [id]);

  const handleChange =
    (field: keyof CreateCarRequest) =>
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const value =
        field === "year" ? parseInt(e.target.value, 10) || 0 : e.target.value;
      setForm((prev) => ({ ...prev, [field]: value }));
    };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError(null);

    try {
      if (isEdit) {
        await carService.update(id!, form);
      } else {
        await carService.create(form);
      }
      navigate("/cars");
    } catch (err: unknown) {
      if (axios.isAxiosError(err) && err.response?.data) {
        const data = err.response.data;
        setError(
          typeof data === "string"
            ? data
            : (data.error ?? data.title ?? t("cars.failedToSave")),
        );
      } else {
        setError(t("cars.failedToSave"));
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
        {isEdit ? t("cars.editCar") : t("cars.newCar")}
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
          label={t("cars.make")}
          value={form.make}
          onChange={handleChange("make")}
          required
        />
        <TextField
          label={t("cars.model")}
          value={form.model}
          onChange={handleChange("model")}
          required
        />
        <TextField
          label={t("cars.licensePlate")}
          value={form.licensePlate}
          onChange={handleChange("licensePlate")}
          required
        />
        <TextField
          label={t("cars.year")}
          type="number"
          value={form.year}
          onChange={handleChange("year")}
          required
        />
        <Box sx={{ display: "flex", gap: 2 }}>
          <Button type="submit" variant="contained" disabled={saving}>
            {saving
              ? t("common.saving")
              : isEdit
                ? t("common.update")
                : t("common.create")}
          </Button>
          <Button variant="outlined" onClick={() => navigate("/cars")}>
            {t("common.cancel")}
          </Button>
        </Box>
      </Box>
    </Box>
  );
}
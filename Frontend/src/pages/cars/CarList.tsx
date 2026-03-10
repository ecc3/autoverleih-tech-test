import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Alert,
  Box,
  Button,
  TextField,
  Typography,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
} from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import type { GridColDef } from "@mui/x-data-grid";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";
import SearchIcon from "@mui/icons-material/Search";
import { carService } from "../../api/carService";
import type { CarResponse } from "../../types/car";

export default function CarList() {
  const navigate = useNavigate();
  const [cars, setCars] = useState<CarResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<CarResponse | null>(null);

  const fetchCars = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await carService.getAll();
      setCars(data);
    } catch {
      setError("Failed to load cars");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCars();
  }, []);

  const filteredCars = useMemo(() => {
    if (!search) return cars;
    const lower = search.toLowerCase();
    return cars.filter(
      (c) =>
        c.make.toLowerCase().includes(lower) ||
        c.model.toLowerCase().includes(lower) ||
        c.licensePlate.toLowerCase().includes(lower),
    );
  }, [cars, search]);

  const handleDelete = async () => {
    if (!deleteTarget) return;
    try {
      await carService.delete(deleteTarget.id);
      setDeleteTarget(null);
      fetchCars();
    } catch {
      setError("Failed to delete car");
    }
  };

  const columns: GridColDef[] = [
    { field: "make", headerName: "Make", flex: 1 },
    { field: "model", headerName: "Model", flex: 1 },
    { field: "licensePlate", headerName: "License Plate", flex: 1 },
    { field: "year", headerName: "Year", width: 100 },
    {
      field: "isAvailable",
      headerName: "Available",
      width: 120,
      valueFormatter: (value: boolean) => (value ? "Yes" : "No"),
    },
    {
      field: "createdAt",
      headerName: "Created",
      flex: 1,
      valueFormatter: (value: string) => new Date(value).toLocaleDateString(),
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 120,
      sortable: false,
      renderCell: (params) => (
        <>
          <IconButton
            size="small"
            onClick={() => navigate(`/cars/${params.row.id}`)}
          >
            <EditIcon />
          </IconButton>
          <IconButton size="small" onClick={() => setDeleteTarget(params.row)}>
            <DeleteIcon />
          </IconButton>
        </>
      ),
    },
  ];

  return (
    <Box>
      <Box sx={{ display: "flex", justifyContent: "space-between", mb: 2 }}>
        <Typography variant="h4">Cars</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate("/cars/new")}
        >
          Add Car
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <TextField
        placeholder="Search by make, model, or plate..."
        size="small"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        slotProps={{
          input: {
            startAdornment: (
              <SearchIcon sx={{ mr: 1, color: "text.secondary" }} />
            ),
          },
        }}
        sx={{ mb: 2, width: 300 }}
      />

      <DataGrid
        rows={filteredCars}
        columns={columns}
        loading={loading}
        pageSizeOptions={[10, 25, 50]}
        initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
        disableRowSelectionOnClick
      />

      <Dialog open={!!deleteTarget} onClose={() => setDeleteTarget(null)}>
        <DialogTitle>Delete Car</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete {deleteTarget?.make}{" "}
            {deleteTarget?.model}? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteTarget(null)}>Cancel</Button>
          <Button onClick={handleDelete} color="error">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

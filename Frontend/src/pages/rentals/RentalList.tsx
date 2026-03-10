import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Alert,
  Box,
  Button,
  Chip,
  TextField,
  Typography,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  FormControlLabel,
  Switch,
} from "@mui/material";
import { DataGrid } from "@mui/x-data-grid";
import type { GridColDef } from "@mui/x-data-grid";
import AddIcon from "@mui/icons-material/Add";
import SearchIcon from "@mui/icons-material/Search";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import CancelIcon from "@mui/icons-material/Cancel";
import { rentalService } from "../../api/rentalService";
import { customerService } from "../../api/customerService";
import { carService } from "../../api/carService";
import type { RentalResponse } from "../../types/rental";

type EnrichedRental = RentalResponse & {
  customerName: string;
  carLabel: string;
};

const statusChipProps: Record<
  string,
  { color: "info" | "success" | "error" | "default" }
> = {
  Active: { color: "info" },
  Completed: { color: "success" },
  Cancelled: { color: "error" },
};

export default function RentalList() {
  const navigate = useNavigate();
  const [rentals, setRentals] = useState<EnrichedRental[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [activeOnly, setActiveOnly] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [actionTarget, setActionTarget] = useState<{
    rental: EnrichedRental;
    type: "return" | "cancel";
  } | null>(null);
  const [returnKm, setReturnKm] = useState("");

  const fetchData = async () => {
    setLoading(true);
    setError(null);
    try {
      const [rentalsData, custs, carsData] = await Promise.all([
        rentalService.getAll(),
        customerService.getAll(),
        carService.getAll(),
      ]);
      const enriched = rentalsData.map((r) => {
        const cust = custs.find((c) => c.id === r.customerId);
        const car = carsData.find((c) => c.id === r.carId);
        return {
          ...r,
          customerName: cust ? `${cust.firstName} ${cust.lastName}` : "Unknown",
          carLabel: car ? `${car.make} ${car.model}` : "Unknown",
        };
      });
      setRentals(enriched);
    } catch {
      setError("Failed to load rentals");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const filteredRentals = useMemo(() => {
    let result = rentals;
    if (activeOnly) {
      result = result.filter((r) => r.status === "Active");
    }
    if (search) {
      const lower = search.toLowerCase();
      result = result.filter(
        (r) =>
          r.customerName.toLowerCase().includes(lower) ||
          r.carLabel.toLowerCase().includes(lower) ||
          r.status.toLowerCase().includes(lower),
      );
    }
    return result;
  }, [rentals, search, activeOnly]);

  const handleAction = async () => {
    if (!actionTarget) return;
    try {
      if (actionTarget.type === "return") {
        const km = parseInt(returnKm, 10);
        if (!km || km <= 0) return;
        await rentalService.returnRental(actionTarget.rental.id, km);
      } else {
        await rentalService.cancelRental(actionTarget.rental.id);
      }
      setActionTarget(null);
      setReturnKm("");
      fetchData();
    } catch {
      setError("Failed to perform action");
    }
  };

  const handleOpenAction = (
    rental: EnrichedRental,
    type: "return" | "cancel",
  ) => {
    setActionTarget({ rental, type });
    setReturnKm("");
  };

  const columns: GridColDef[] = [
    { field: "customerName", headerName: "Customer", flex: 1 },
    { field: "carLabel", headerName: "Car", flex: 1 },
    {
      field: "startDate",
      headerName: "Start",
      flex: 1,
      valueFormatter: (value: string) => new Date(value).toLocaleDateString(),
    },
    {
      field: "endDate",
      headerName: "End",
      flex: 1,
      valueFormatter: (value: string) => new Date(value).toLocaleDateString(),
    },
    {
      field: "kilometersDriven",
      headerName: "KM Driven",
      width: 120,
      valueFormatter: (value: number | null) =>
        value != null ? `${value.toLocaleString()} km` : "\u2014",
    },
    {
      field: "status",
      headerName: "Status",
      width: 130,
      renderCell: (params) => {
        const status = params.value as string;
        const props = statusChipProps[status] ?? { color: "default" as const };
        return <Chip label={status} size="small" color={props.color} />;
      },
    },
    {
      field: "actions",
      headerName: "Actions",
      width: 120,
      sortable: false,
      renderCell: (params) => {
        const rental = params.row as EnrichedRental;
        if (rental.status !== "Active") return null;
        return (
          <>
            <IconButton
              size="small"
              color="success"
              onClick={() => handleOpenAction(rental, "return")}
            >
              <CheckCircleIcon />
            </IconButton>
            <IconButton
              size="small"
              color="error"
              onClick={() => handleOpenAction(rental, "cancel")}
            >
              <CancelIcon />
            </IconButton>
          </>
        );
      },
    },
  ];

  const isReturnDialog = actionTarget?.type === "return";
  const returnKmValid = isReturnDialog ? parseInt(returnKm, 10) > 0 : true;

  return (
    <Box>
      <Box sx={{ display: "flex", justifyContent: "space-between", mb: 2 }}>
        <Typography variant="h4">Rentals</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate("/rentals/new")}
        >
          New Rental
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Box sx={{ display: "flex", gap: 2, alignItems: "center", mb: 2 }}>
        <TextField
          placeholder="Search by customer, car or status..."
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
          sx={{ width: 300 }}
        />
        <FormControlLabel
          control={
            <Switch
              checked={activeOnly}
              onChange={(e) => setActiveOnly(e.target.checked)}
              size="small"
            />
          }
          label="Active only"
        />
      </Box>

      <DataGrid
        rows={filteredRentals}
        columns={columns}
        loading={loading}
        pageSizeOptions={[10, 25, 50]}
        initialState={{ pagination: { paginationModel: { pageSize: 10 } } }}
        disableRowSelectionOnClick
      />

      <Dialog open={!!actionTarget} onClose={() => setActionTarget(null)}>
        <DialogTitle>
          {isReturnDialog ? "Return Rental" : "Cancel Rental"}
        </DialogTitle>
        <DialogContent>
          <DialogContentText>
            {isReturnDialog
              ? `${actionTarget?.rental.carLabel} rented by ${actionTarget?.rental.customerName}`
              : `Are you sure you want to cancel the rental for ${actionTarget?.rental.carLabel} by ${actionTarget?.rental.customerName}?`}
          </DialogContentText>
          {isReturnDialog && (
            <TextField
              autoFocus
              label="Kilometers Driven"
              type="number"
              fullWidth
              required
              value={returnKm}
              onChange={(e) => setReturnKm(e.target.value)}
              sx={{ mt: 2 }}
              slotProps={{ htmlInput: { min: 1 } }}
            />
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setActionTarget(null)}>Cancel</Button>
          <Button
            onClick={handleAction}
            color={isReturnDialog ? "primary" : "error"}
            disabled={isReturnDialog && !returnKmValid}
          >
            {isReturnDialog ? "Confirm Return" : "Yes, Cancel"}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

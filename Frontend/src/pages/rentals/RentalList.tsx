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

export default function RentalList() {
  const navigate = useNavigate();
  const [rentals, setRentals] = useState<EnrichedRental[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [actionTarget, setActionTarget] = useState<{
    rental: EnrichedRental;
    type: "return" | "cancel";
  } | null>(null);

  const fetchData = async () => {
    setLoading(true);
    setError(null);
    try {
      const [rentalsData, custs, carsData] = await Promise.all([
        rentalService.getAll(),
        customerService.getAll(),
        carService.getAll(),
      ]);
      const enriched = rentalsData.map((r) => ({
        ...r,
        customerName:
          custs.find((c) => c.id === r.customerId)?.firstName +
          " " +
          custs.find((c) => c.id === r.customerId)?.lastName,
        carLabel:
          carsData.find((c) => c.id === r.carId)?.make +
          " " +
          carsData.find((c) => c.id === r.carId)?.model,
      }));
      setRentals(enriched as EnrichedRental[]);
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
    if (!search) return rentals;
    const lower = search.toLowerCase();
    return rentals.filter(
      (r) =>
        r.customerName.toLowerCase().includes(lower) ||
        r.carLabel.toLowerCase().includes(lower) ||
        r.status.toLowerCase().includes(lower),
    );
  }, [rentals, search]);

  const handleAction = async () => {
    if (!actionTarget) return;
    try {
      if (actionTarget.type === "return") {
        await rentalService.returnRental(actionTarget.rental.id);
      } else {
        await rentalService.cancelRental(actionTarget.rental.id);
      }
      setActionTarget(null);
      fetchData();
    } catch {
      setError("Failed to perform action");
    }
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
    { field: "status", headerName: "Status", width: 120 },
    {
      field: "actions",
      headerName: "Actions",
      width: 160,
      sortable: false,
      renderCell: (params) => {
        const rental = params.row as EnrichedRental;
        return (
          <>
            {rental.status === "Active" && (
              <>
                <IconButton
                  size="small"
                  onClick={() => setActionTarget({ rental, type: "return" })}
                >
                  <CheckCircleIcon />
                </IconButton>
                <IconButton
                  size="small"
                  onClick={() => setActionTarget({ rental, type: "cancel" })}
                >
                  <CancelIcon />
                </IconButton>
              </>
            )}
          </>
        );
      },
    },
  ];

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
        sx={{ mb: 2, width: 300 }}
      />

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
          {actionTarget?.type === "return" ? "Return Rental" : "Cancel Rental"}
        </DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to {actionTarget?.type} the rental for{" "}
            {actionTarget?.rental.carLabel} by{" "}
            {actionTarget?.rental.customerName}?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setActionTarget(null)}>No</Button>
          <Button onClick={handleAction} color="primary">
            Yes
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Alert,
  Box,
  Card,
  CardContent,
  CircularProgress,
  List,
  ListItemButton,
  ListItemText,
  Paper,
  Typography,
} from "@mui/material";
import PeopleIcon from "@mui/icons-material/People";
import DirectionsCarIcon from "@mui/icons-material/DirectionsCar";
import AssignmentIcon from "@mui/icons-material/Assignment";
import SpeedIcon from "@mui/icons-material/Speed";
import { useTranslation } from "react-i18next";
import { customerService } from "../api/customerService";
import { carService } from "../api/carService";
import { rentalService } from "../api/rentalService";
import type { CarResponse } from "../types/car";
import type { CustomerResponse } from "../types/customer";
import type { RentalResponse } from "../types/rental";

interface Stats {
  totalCustomers: number;
  totalCars: number;
  rentedCars: number;
  availableCars: number;
  activeRentals: number;
  totalKm: number;
  recentRentals: {
    id: string;
    carLabel: string;
    customerName: string;
    startDate: string;
    status: string;
  }[];
}

export default function Dashboard() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [stats, setStats] = useState<Stats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([
      customerService.getAll(),
      carService.getAll(),
      rentalService.getAll(),
    ])
      .then(([customers, cars, rentals]: [CustomerResponse[], CarResponse[], RentalResponse[]]) => {
        const rentedCars = cars.filter((c) => !c.isAvailable).length;
        const activeRentals = rentals.filter((r) => r.status === "Active");
        const totalKm = cars.reduce((sum, c) => sum + (c.totalKilometers ?? 0), 0);

        const custMap = new Map(customers.map((c) => [c.id, `${c.firstName} ${c.lastName}`]));
        const carMap = new Map(cars.map((c) => [c.id, `${c.make} ${c.model}`]));

        const recent = [...rentals]
          .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
          .slice(0, 5)
          .map((r) => ({
            id: r.id,
            carLabel: carMap.get(r.carId) ?? t("common.unknown"),
            customerName: custMap.get(r.customerId) ?? t("common.unknown"),
            startDate: new Date(r.startDate).toLocaleDateString(),
            status: r.status,
          }));

        setStats({
          totalCustomers: customers.length,
          totalCars: cars.length,
          rentedCars,
          availableCars: cars.length - rentedCars,
          activeRentals: activeRentals.length,
          totalKm,
          recentRentals: recent,
        });
      })
      .catch(() => setError(t("dashboard.failedToLoad")))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", mt: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mt: 2 }}>
        {error}
      </Alert>
    );
  }

  const cards = [
    {
      label: t("dashboard.totalCustomers"),
      value: stats!.totalCustomers,
      icon: <PeopleIcon sx={{ fontSize: 40, color: "primary.main" }} />,
      path: "/customers",
    },
    {
      label: t("dashboard.totalCars"),
      value: stats!.totalCars,
      icon: <DirectionsCarIcon sx={{ fontSize: 40, color: "primary.main" }} />,
      path: "/cars",
    },
    {
      label: t("dashboard.currentlyRented"),
      value: stats!.rentedCars,
      icon: <AssignmentIcon sx={{ fontSize: 40, color: "warning.main" }} />,
      path: "/rentals",
    },
    {
      label: t("dashboard.totalKmDriven"),
      value: stats!.totalKm.toLocaleString(),
      icon: <SpeedIcon sx={{ fontSize: 40, color: "info.main" }} />,
      path: "/cars",
    },
  ];

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        {t("dashboard.title")}
      </Typography>

      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: {
            xs: "1fr",
            sm: "1fr 1fr",
            md: "1fr 1fr 1fr 1fr",
          },
          gap: 2,
          mb: 4,
        }}
      >
        {cards.map((card) => (
          <Card
            key={card.label}
            sx={{ cursor: "pointer", "&:hover": { boxShadow: 4 } }}
            onClick={() => navigate(card.path)}
          >
            <CardContent
              sx={{
                display: "flex",
                alignItems: "center",
                gap: 2,
              }}
            >
              {card.icon}
              <Box>
                <Typography variant="h4" fontWeight="bold">
                  {card.value}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {card.label}
                </Typography>
              </Box>
            </CardContent>
          </Card>
        ))}
      </Box>

      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          {t("dashboard.recentRentals")}
        </Typography>
        {stats!.recentRentals.length === 0 ? (
          <Typography color="text.secondary">
            {t("dashboard.noRecentRentals")}
          </Typography>
        ) : (
          <List disablePadding>
            {stats!.recentRentals.map((r) => (
              <ListItemButton
                key={r.id}
                onClick={() => navigate("/rentals")}
                divider
              >
                <ListItemText
                  primary={`${r.carLabel} — ${t("dashboard.rentedBy")} ${r.customerName}`}
                  secondary={`${r.startDate} · ${r.status}`}
                />
              </ListItemButton>
            ))}
          </List>
        )}
      </Paper>
    </Box>
  );
}
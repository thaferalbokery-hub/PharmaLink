# PharmaLink - Pharmacy and Medicine Availability Platform

A complete ASP.NET Core MVC application for connecting pharmacies with customers.

## Features

- **Medicine Search** - Search by scientific or commercial name
- **Real-Time Availability** - Available, Low Stock, Out of Stock
- **Price Transparency** - Compare prices across pharmacies
- **Pharmacy Status** - Open Now / Closed
- **Role-Based Access** - Admin, Pharmacist, Customer
- **Inventory Management** - Full CRUD with audit trail
- **Reviews and Ratings** - Customer feedback system
- **Favorites** - Save pharmacies and medicines
- **Reports** - Admin and Pharmacist dashboards
- **Image Management** - Upload/manage medicine images

## Technology Stack

- ASP.NET Core MVC (.NET 9)
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Bootstrap 5
- Razor Views

## Quick Start

```bash
# Restore packages
dotnet restore

# Add initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update

# Run application
dotnet run
```

## Default Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@pharmalink.com | Admin@123 |
| Pharmacist | pharmacist@pharmalink.com | Pharm@123 |
| Customer | customer@pharmalink.com | Cust@123 |

## Project Structure

```
PharmaLink/
├── Controllers/     (10 controllers)
├── Models/          (18 models)
├── ViewModels/      (5 ViewModel files)
├── Data/            (ApplicationDbContext)
├── Services/        (8 service interfaces + implementations)
├── Views/           (40+ Razor views)
├── wwwroot/         (CSS, JS, uploads)
├── Program.cs
└── appsettings.json
```

## Configuration

Edit `appsettings.json`:
- `ConnectionStrings:DefaultConnection` - SQL Server connection
- `AppSettings:Currency` - Currency code (default: SAR)
- `AppSettings:LowStockThreshold` - Low stock threshold (default: 10)
- `AppSettings:MaxImageSizeMB` - Max image upload size (default: 5)

## License

This project is for educational purposes.
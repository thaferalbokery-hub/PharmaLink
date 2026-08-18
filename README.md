# PharmaLink - Pharmacy Management System

## Overview
PharmaLink is a comprehensive Pharmacy Management System built with **ASP.NET Core MVC (.NET 9)**, **Entity Framework Core**, **SQL Server**, and **ASP.NET Identity**. It provides complete CRUD operations, search/filtering, reports, dashboards, and image upload capabilities.

## 10 Domain Models

| # | Model | Description | Relationships |
|---|-------|-------------|---------------|
| 1 | **ApplicationUser** | System users (Admin, Pharmacist, Customer) | 1:1 with Pharmacy, 1:N with Prescriptions & Sales |
| 2 | **Pharmacy** | Pharmacy stores | 1:1 with Owner (User), 1:N with Inventory & Sales |
| 3 | **Medicine** | Medicines/drugs catalog | 1:N with Inventory, PrescriptionItems, SaleItems, SupplierMedicines |
| 4 | **Supplier** | Medicine suppliers | M:N with Medicine (through SupplierMedicine) |
| 5 | **Inventory** | Stock per pharmacy per medicine | N:1 with Pharmacy, N:1 with Medicine |
| 6 | **Prescription** | Patient prescriptions | N:1 with User, 1:N with PrescriptionItems |
| 7 | **PrescriptionItem** | Individual prescription line items | N:1 with Prescription, N:1 with Medicine |
| 8 | **Sale** | Sales transactions | N:1 with User, N:1 with Pharmacy, 1:N with SaleItems |
| 9 | **SaleItem** | Individual sale line items | N:1 with Sale, N:1 with Medicine |
| 10 | **SupplierMedicine** | M:N join table (Supplier ↔ Medicine) | N:1 with Supplier, N:1 with Medicine |

## Relationships Summary
- **1:1** → ApplicationUser ↔ Pharmacy (Owner)
- **1:N** → User→Prescriptions, User→Sales, Pharmacy→Inventory, Pharmacy→Sales, Medicine→Inventory, Prescription→PrescriptionItems, Sale→SaleItems
- **M:N** → Supplier ↔ Medicine (through SupplierMedicine)

## Features
- ✅ **Authentication & Authorization** (3 roles: Admin, Pharmacist, Customer)
- ✅ **Full CRUD** for all entities
- ✅ **Image Upload** (Medicine & Pharmacy with GUID naming + validation)
- ✅ **Search & Filtering** (by name, category, city, status, date range)
- ✅ **Reports Dashboard** (revenue, top medicines, low stock, monthly sales)
- ✅ **Admin Dashboard** (users, pharmacies, medicines, suppliers stats)
- ✅ **Pharmacist Dashboard** (inventory, prescriptions, sales)
- ✅ **Seed Data** (sample users, pharmacies, medicines, suppliers, inventory, sales)
- ✅ **Responsive UI** (Bootstrap 5 + Font Awesome)

## Tech Stack
- ASP.NET Core MVC (.NET 9)
- Entity Framework Core 9 (Code-First)
- SQL Server (LocalDB)
- ASP.NET Identity (Authentication/Authorization)
- Bootstrap 5 + Font Awesome 6
- Razor Views

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB or full instance)

### Run the Project
```bash
# Clone the repository
git clone https://github.com/thaferalbokery-hub/PharmaLink.git
cd PharmaLink

# Update connection string in appsettings.json if needed

# Run migrations and start
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

### Default Accounts
| Role | Email | Password |
|------|-------|----------|
| Admin | admin@pharmalink.com | Admin@123 |
| Pharmacist | pharmacist@pharmalink.com | Pharm@123 |
| Customer | customer@pharmalink.com | Cust@123 |

## Project Structure
```
PharmaLink/
├── Controllers/          # MVC Controllers (10 controllers)
├── Data/                 # ApplicationDbContext + Fluent API
├── Models/               # 10 Domain Models
├── ViewModels/           # Login/Register ViewModels
├── Views/                # Razor Views (CRUD + Dashboards + Reports)
├── Services/             # ImageService + SeedDataService
├── wwwroot/              # Static files (CSS, JS, uploads)
├── Program.cs            # App configuration
└── appsettings.json      # Connection strings & settings
```

## Class Diagram (Simplified)
```
ApplicationUser (1)──────(1) Pharmacy
       │                        │
       │ 1:N                    │ 1:N
       ▼                        ▼
  Prescription              Inventory ◄── Medicine
       │                                      │
       │ 1:N                                  │ M:N
       ▼                                      ▼
  PrescriptionItem              SupplierMedicine ──► Supplier
                                      
  Sale ──────► SaleItem ──────► Medicine
```

## License
This project is developed for educational purposes (Visual Programming Course).
# PharmaLink - Complete Project Documentation

## 1. General Project Overview
PharmaLink is a comprehensive Pharmacy Management System built with ASP.NET Core MVC (.NET 9). It provides a complete solution for managing pharmacies, medicines, suppliers, inventory, prescriptions, and sales with role-based access control.

## 2. Objectives
- Provide a centralized platform for pharmacy management
- Enable medicine availability tracking across multiple pharmacies
- Implement prescription management with approval workflows
- Track sales with automatic inventory updates
- Manage supplier relationships and supply chain
- Generate comprehensive reports and analytics

## 3. Problem Domain
The healthcare sector requires efficient pharmacy management systems to:
- Track medicine inventory across multiple locations
- Process prescriptions safely with proper validation
- Manage supplier relationships and pricing
- Generate sales reports for business decisions
- Ensure proper authorization for sensitive operations

## 4. Technologies
| Technology | Version | Purpose |
|-----------|---------|---------|
| ASP.NET Core MVC | .NET 9 | Web framework |
| C# | 13 | Programming language |
| Entity Framework Core | 9.0 | ORM / Database access |
| SQL Server | Latest | Database |
| ASP.NET Core Identity | 9.0 | Authentication & Authorization |
| Razor Views | - | Server-side rendering |
| Bootstrap | 5.3 | UI framework |
| Font Awesome | 6.5 | Icons |
| HTML5 / CSS3 | - | Markup & styling |

## 5. System Architecture
```
┌─────────────────────────────────────────────┐
│              Presentation Layer              │
│    (Razor Views + Bootstrap 5 + JS)         │
├─────────────────────────────────────────────┤
│              Controller Layer                │
│  (MVC Controllers with Authorization)       │
├─────────────────────────────────────────────┤
│              Service Layer                   │
│    (ImageService, SeedDataService)          │
├─────────────────────────────────────────────┤
│              Data Access Layer               │
│  (EF Core + ApplicationDbContext)           │
├─────────────────────────────────────────────┤
│              Database Layer                  │
│         (SQL Server + Identity)             │
└─────────────────────────────────────────────┘
```

## 6. Database Design
The database uses Entity Framework Core Code-First approach with Fluent API configuration for relationships, indexes, and constraints.

### Connection String
```json
"Server=(localdb)\\mssqllocaldb;Database=PharmaLinkDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

## 7. Exactly 10 Domain Models

| # | Model | Description | Key Properties |
|---|-------|-------------|----------------|
| 1 | ApplicationUser | System users extending IdentityUser | FirstName, LastName, City, Address, IsActive |
| 2 | Pharmacy | Pharmacy stores | Name, Address, City, Phone, Email, ImageUrl, IsOpen |
| 3 | Medicine | Medicine catalog | Name, Category, Price, Quantity, ExpiryDate, ImageUrl, RequiresPrescription |
| 4 | Supplier | Medicine suppliers | Name, ContactPerson, Phone, Email, Address |
| 5 | Inventory | Stock per pharmacy/medicine | PharmacyId, MedicineId, Quantity, MinimumStockLevel |
| 6 | Prescription | Patient prescriptions | UserId, PrescriptionDate, Status, DoctorName, Notes |
| 7 | PrescriptionItem | Prescription line items | PrescriptionId, MedicineId, Quantity, DosageInstructions |
| 8 | Sale | Sales transactions | UserId, PharmacyId, SaleDate, TotalAmount, Status |
| 9 | SaleItem | Sale line items | SaleId, MedicineId, Quantity, UnitPrice |
| 10 | SupplierMedicine | M:N join (Supplier↔Medicine) | SupplierId, MedicineId, SupplyPrice, AvailableQuantity |

## 8. Detailed Relationships

### One-to-One (1:1)
- **ApplicationUser ↔ Pharmacy**: A pharmacist user owns exactly one pharmacy.
  - Configured via Fluent API: `HasOne(u => u.OwnedPharmacy).WithOne(p => p.Owner)`

### One-to-Many (1:N)
- Pharmacy → Inventory (one pharmacy has many inventory items)
- Medicine → Inventory (one medicine appears in many pharmacy inventories)
- ApplicationUser → Prescription (one user has many prescriptions)
- Prescription → PrescriptionItem (one prescription has many items)
- Medicine → PrescriptionItem (one medicine in many prescription items)
- ApplicationUser → Sale (one user has many sales)
- Pharmacy → Sale (one pharmacy has many sales)
- Sale → SaleItem (one sale has many items)
- Medicine → SaleItem (one medicine in many sale items)

### Many-to-Many (M:N)
- **Supplier ↔ Medicine** through SupplierMedicine join entity
  - Contains additional data: SupplyPrice, AvailableQuantity, LastSupplyDate
  - Unique constraint on (SupplierId, MedicineId)

## 9. Authentication
- ASP.NET Core Identity with custom ApplicationUser
- Cookie-based authentication
- Email confirmation (auto-confirmed for seed users)
- Password requirements: 6+ chars, uppercase, lowercase, digit

## 10. Authorization
| Role | Permissions |
|------|------------|
| Admin | Full access to all features: users, pharmacies, medicines, suppliers, inventory, prescriptions, sales, reports |
| Pharmacist | Manage medicines, inventory, prescriptions, sales, suppliers, view reports |
| Customer | Browse medicines/pharmacies, create/view own prescriptions, view own sales |

Implementation: `[Authorize(Roles = "Admin,Pharmacist")]` attributes on controllers/actions.

## 11. CRUD Operations
All 10 entities have complete CRUD:
- **Index** (List with filtering)
- **Details** (Single item with related data via Include/ThenInclude)
- **Create** (With validation)
- **Edit** (With authorization checks)
- **Delete** (With confirmation and cascade handling)

## 12. Image Handling
- **Upload**: Medicines and Pharmacies support image upload
- **Validation**: Extension (.jpg, .jpeg, .png, .webp), MIME type, file size (max 5MB)
- **Storage**: `wwwroot/uploads/` with GUID-based unique filenames
- **Update**: Old image deleted from server when replaced
- **Delete**: Image file removed when entity is deleted
- **Display**: Responsive images with fallback icons

## 13. Filtering (Row-Level)
| Entity | Filters |
|--------|---------|
| Medicine | Search (name/description), Category, Availability |
| Pharmacy | Search (name/address), City |
| Inventory | Pharmacy, Low stock, Medicine name |
| Prescription | Status, Date range |
| Sale | Status, Date range, Pharmacy |
| Supplier | Name/contact search |

Implementation: LINQ `Where()` clauses applied to IQueryable before execution.

## 14. Column Projection
Implemented in `ReportController.Index()` using `.Select()`:
- `MedicineReportDto`: Only Id, Name, TotalQuantitySold, TotalRevenue
- `PharmacySalesDto`: Only PharmacyName, TotalSales, TotalRevenue
- `LowStockDto`: Only PharmacyName, MedicineName, CurrentStock, MinimumLevel
- `MonthlySalesDto`: Only Month, TotalSales, TotalRevenue

These DTOs are NOT database entities - they exist only for query projection.

## 15. Eager Loading
Examples of Include/ThenInclude usage:
```csharp
// Prescription with items and medicines
.Include(p => p.PrescriptionItems).ThenInclude(pi => pi.Medicine)

// Sale with items and medicines
.Include(s => s.SaleItems).ThenInclude(si => si.Medicine)

// Inventory with pharmacy and medicine
.Include(i => i.Pharmacy).Include(i => i.Medicine)

// SupplierMedicine with both sides
.Include(sm => sm.Supplier).Include(sm => sm.Medicine)
```

## 16. ViewBag/ViewData Usage
- Category dropdowns in Medicine filtering
- Pharmacy dropdowns in Inventory/Sale creation
- Supplier dropdowns in SupplierMedicine
- Dashboard statistics (counts, totals, averages)
- Filter metadata (current search, selected category)
- Page titles via ViewBag.Title

## 17. Partial Views
| Partial | Purpose |
|---------|---------|
| _Navbar | Navigation with role-based menu items |
| _Alerts | Success/error alert messages |
| _ValidationScriptsPartial | Client-side validation scripts |
| _DashboardCards | Reusable statistics card component |
| _Layout | Main layout template |

## 18. Reporting
Reports include:
- Total medicines, pharmacies, suppliers, users
- Total sales count and revenue (Sum)
- Average sale amount (Average)
- Low stock and out-of-stock counts
- Prescription statistics by status
- Top selling medicines (GroupBy + Sum)
- Sales by pharmacy (GroupBy)
- Monthly sales trends (GroupBy year/month)

## 19. Business Logic

### Inventory Management
- Quantity cannot be negative
- Low stock detection (quantity ≤ minimum level)
- Out of stock detection (quantity = 0)
- Unique constraint: one entry per pharmacy-medicine pair
- Sales automatically decrease inventory
- Deleted sales restore inventory

### Prescription Processing
- Status workflow: Pending → Approved → Dispensed (or Rejected/Cancelled)
- Medicine validation (must exist and require prescription)
- Quantity validation (must be > 0)
- Authorization: customers see only their own prescriptions
- Admin/Pharmacist can update status and add/remove items

### Sales Processing
- Stock validation before sale completion
- Server-side price calculation (never trust browser)
- Automatic total calculation
- Inventory update on sale completion
- Inventory restoration on sale deletion
- Authorization: customers see only their own sales

## 20. Installation

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Steps
```bash
git clone https://github.com/thaferalbokery-hub/PharmaLink.git
cd PharmaLink
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

## 21. Database Migration
The project uses EF Core Code-First migrations:
```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update

# List migrations
dotnet ef migrations list
```

The migration creates all tables for the 10 domain models plus Identity tables.

## 22. Seed Accounts
| Role | Email | Password |
|------|-------|----------|
| Admin | admin@pharmalink.com | Admin@123 |
| Pharmacist | pharmacist@pharmalink.com | Pharm@123 |
| Customer | customer@pharmalink.com | Cust@123 |

Passwords are hashed using ASP.NET Identity's UserManager (never stored as plain text).

## 23. User Guide

### Admin Guide
1. Login with admin@pharmalink.com / Admin@123
2. Access Admin Dashboard from navbar
3. Manage Users: View all registered users
4. Manage Pharmacies: Create/Edit/Delete pharmacies with images
5. Manage Medicines: Full CRUD with image upload and categories
6. Manage Suppliers: Add/edit supplier information
7. Manage Inventory: Add stock, update quantities, monitor levels
8. Manage Prescriptions: Review, approve/reject, add items
9. Manage Sales: Create sales, track revenue
10. View Reports: Analytics dashboard with charts and statistics

### Pharmacist Guide
1. Login with pharmacist@pharmalink.com / Pharm@123
2. Access Pharmacist Dashboard
3. Manage owned pharmacy inventory
4. Process prescriptions (approve/reject)
5. Create and manage sales
6. Add/manage supplier relationships
7. View reports and analytics

### Customer Guide
1. Register at /Account/Register
2. Login with credentials
3. Browse medicines with search and category filters
4. View pharmacy locations and availability
5. Create prescriptions (select medicines, add dosage info)
6. View prescription status
7. View purchase history

## 24. Testing Checklist
- [x] ASP.NET Core MVC (.NET 9)
- [x] Entity Framework Core with SQL Server
- [x] ASP.NET Core Identity (Register/Login/Logout)
- [x] Role-based Authorization (Admin, Pharmacist, Customer)
- [x] Exactly 10 domain models
- [x] 1:1 relationship (User ↔ Pharmacy)
- [x] 1:N relationships (multiple)
- [x] M:N relationship (Supplier ↔ Medicine via SupplierMedicine)
- [x] Complete Seed Data (roles, users, all entities)
- [x] Full CRUD for all entities
- [x] Image Upload/Display/Update/Delete
- [x] Data Annotations ([Required], [StringLength], [Range], etc.)
- [x] Server & Client validation
- [x] Tag Helpers (asp-for, asp-action, asp-route-*, asp-validation-*)
- [x] Row-level filtering with LINQ Where()
- [x] Column-level projection with .Select()
- [x] Eager loading with Include/ThenInclude
- [x] ViewBag/ViewData usage
- [x] Partial Views
- [x] Reporting with Count/Sum/Average/GroupBy
- [x] Inventory business logic
- [x] Prescription business logic
- [x] Sales business logic
- [x] Responsive Bootstrap 5 UI
- [x] EF Core Migration support
- [x] Documentation
- [x] Class Diagram
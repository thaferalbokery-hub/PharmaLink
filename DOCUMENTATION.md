# PharmaLink - Complete Documentation

## 1. Project Overview

**PharmaLink** is a Pharmacy and Medicine Availability Platform built with ASP.NET Core MVC (.NET 9). It connects pharmacies with customers, enabling users to search for medicines, check availability, view prices, and find nearby pharmacies.

## 2. Problem Statement

Finding medicine availability information across multiple pharmacies is time-consuming. Customers need to call or visit pharmacies to check if a specific medicine is in stock. PharmaLink solves this by providing a centralized digital platform.

## 3. Project Objectives

1. Simplify medicine searching (by scientific or commercial name)
2. Real-time availability updates (Available, Low Stock, Out of Stock)
3. Price transparency across pharmacies
4. Pharmacy status visibility (Open/Closed)
5. Improve access to pharmacy information

## 4. Project Scope

- User registration and authentication
- Role-based authorization (Admin, Pharmacist, Customer)
- Pharmacy management with CRUD operations
- Medicine management with categories and brands
- Inventory management with real-time updates
- Medicine and pharmacy search with filtering
- Favorites system
- Review system
- Notifications
- Search history
- Reporting

## 5. System Users

| Role | Capabilities |
|------|-------------|
| Admin | Full system management |
| Pharmacist | Manage own pharmacy, inventory, prices |
| Customer | Search, browse, favorites, reviews |

## 6. Technologies Used

- ASP.NET Core MVC (.NET 9)
- C#
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Razor Views
- Bootstrap 5
- HTML5 / CSS3
- JavaScript
- Font Awesome Icons

## 7. System Architecture

```
PharmaLink/
├── Controllers/        # MVC Controllers
├── Models/            # Entity Models
├── ViewModels/        # View Models / DTOs
├── Data/              # DbContext
├── Services/          # Business Logic
├── Views/             # Razor Views
├── Migrations/        # EF Core Migrations
├── wwwroot/           # Static Files
├── Program.cs         # Application Entry Point
└── appsettings.json   # Configuration
```

## 8. Database Design

### Models (18 total):
1. ApplicationUser - Extended Identity user
2. UserProfile - Additional user information (1:1 with User)
3. Pharmacy - Pharmacy entity
4. PharmacyAddress - Detailed address (1:1 with Pharmacy)
5. Medicine - Medicine entity
6. MedicineCategory - Categories for medicines
7. MedicineBrand - Brand information
8. Inventory - Stock records (M:N Medicine-Pharmacy)
9. InventoryTransaction - Audit trail
10. MedicineImage - Medicine images
11. FavoritePharmacy - User-Pharmacy favorites (M:N)
12. FavoriteMedicine - User-Medicine favorites (M:N)
13. Review - Pharmacy reviews
14. PharmacyWorkingHour - Operating hours
15. PharmacyContact - Contact information
16. Notification - User notifications
17. SearchHistory - Search tracking
18. AppSettings - Configuration model

## 9. Database Relationships

### One-to-One:
- ApplicationUser → UserProfile
- Pharmacy → PharmacyAddress

### One-to-Many:
- Pharmacy → Inventory
- Pharmacy → WorkingHours
- Pharmacy → Contacts
- Pharmacy → Reviews
- Medicine → Images
- MedicineCategory → Medicines
- MedicineBrand → Medicines
- Medicine → InventoryTransactions
- ApplicationUser → Notifications
- ApplicationUser → SearchHistory

### Many-to-Many:
- Medicine ↔ Pharmacy (through Inventory)
- Customer ↔ Pharmacy (through FavoritePharmacy)
- Customer ↔ Medicine (through FavoriteMedicine)

## 10. Authentication

- ASP.NET Core Identity
- Password hashing (built-in)
- Cookie-based authentication
- Registration, Login, Logout
- Email confirmation (configurable)

### Default Accounts:
| Role | Email | Password |
|------|-------|----------|
| Admin | admin@pharmalink.com | Admin@123 |
| Pharmacist | pharmacist@pharmalink.com | Pharm@123 |
| Customer | customer@pharmalink.com | Cust@123 |

## 11. Authorization

- [Authorize] attribute for authenticated users
- [Authorize(Roles = "Admin")] for admin-only
- [Authorize(Roles = "Pharmacist")] for pharmacist-only
- Ownership checks prevent cross-pharmacy access

## 12. CRUD Operations

All entities support full CRUD:
- Pharmacies (Admin creates, Pharmacist edits own)
- Medicines (Admin full CRUD)
- Categories (Admin full CRUD)
- Brands (Admin full CRUD)
- Inventory (Pharmacist manages own)
- Reviews (Customer creates, owner/admin deletes)

## 13. Image Management

- Upload to wwwroot/uploads/
- GUID filenames for security
- Validation: .jpg, .jpeg, .png, .webp
- Max size: 5MB (configurable)
- Old file deletion on replacement

## 14. Search

- Medicine search by scientific/commercial name
- Pharmacy search by name/city/address
- LINQ Where() + Contains() queries
- Search history recording

## 15. Filtering

- Row-level: Where() clauses
- Column-level: Select() projections
- Medicines: by category, brand, availability
- Pharmacies: by city, open/closed status

## 16. Eager Loading

- Include() and ThenInclude() used throughout
- Pharmacy.Include(p => p.Inventory).ThenInclude(i => i.Medicine)
- Medicine.Include(m => m.Category).Include(m => m.Brand)

## 17. Validation

- Data Annotations: [Required], [StringLength], [Range], [EmailAddress], [Phone]
- Server-side validation in controllers
- Client-side validation with jQuery Validation

## 18. Tag Helpers

- asp-for, asp-controller, asp-action
- asp-route-id, asp-validation-for
- asp-validation-summary
- Used throughout all Razor views

## 19. ViewBag / ViewData

- ViewBag.Title for page titles
- ViewData["SearchResultCount"] for result counts
- ViewBag for dashboard statistics
- ViewBag for dropdown data

## 20. Partial Views

- _Navbar.cshtml - Navigation bar
- _Footer.cshtml - Footer
- _MedicineCard.cshtml - Medicine card component
- _PharmacyCard.cshtml - Pharmacy card component
- _ValidationScriptsPartial.cshtml - Validation scripts

## 21-25. Management Features

- Inventory: Add, Update, Remove with transaction tracking
- Pharmacy: Full lifecycle with status toggle
- Medicine: Complete CRUD with image support
- Availability: Automatic calculation based on stock thresholds
- Price: Per-pharmacy pricing with currency configuration

## 26-31. Additional Features

- Reports: Admin and Pharmacist dashboards with real statistics
- Favorites: Pharmacy and Medicine favorites
- Reviews: Rating and comment system
- Notifications: Database-backed notification system
- Search History: Track and clear search history
- Working Hours: Per-day schedule management

## Installation Instructions

### Prerequisites:
1. .NET 9 SDK
2. SQL Server (LocalDB, Express, or full)
3. Visual Studio 2022/2024 (recommended)

### Steps:

```bash
# 1. Clone or copy the project
cd PharmaLink

# 2. Restore packages
dotnet restore

# 3. Update connection string in appsettings.json

# 4. Create database and apply migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# 5. Run the application
dotnet run
```

### Database Setup:
The application automatically seeds the database on first run with:
- Roles (Admin, Pharmacist, Customer)
- Default users
- Categories and Brands
- Sample medicines and pharmacies
- Sample inventory data
- Working hours

## Class Diagram

```
ApplicationUser (IdentityUser)
├── 1:1 → UserProfile
├── 1:1 → Pharmacy (as Owner)
├── 1:N → FavoritePharmacy
├── 1:N → FavoriteMedicine
├── 1:N → Review
├── 1:N → Notification
└── 1:N → SearchHistory

Pharmacy
├── 1:1 → PharmacyAddress
├── 1:N → Inventory → Medicine (M:N)
├── 1:N → PharmacyWorkingHour
├── 1:N → PharmacyContact
├── 1:N → Review
└── 1:N → FavoritePharmacy

Medicine
├── N:1 → MedicineCategory
├── N:1 → MedicineBrand
├── 1:N → MedicineImage
├── 1:N → Inventory → Pharmacy (M:N)
├── 1:N → InventoryTransaction
└── 1:N → FavoriteMedicine
```
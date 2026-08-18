# PharmaLink - Class Diagram

## Domain Models (Exactly 10)

```mermaid
classDiagram
    class ApplicationUser {
        +string Id (PK)
        +string FirstName
        +string LastName
        +string? Address
        +string? City
        +DateTime CreatedAt
        +bool IsActive
        +int? PharmacyId (FK)
        +Pharmacy? OwnedPharmacy
        +ICollection~Prescription~ Prescriptions
        +ICollection~Sale~ Sales
    }

    class Pharmacy {
        +int Id (PK)
        +string Name
        +string? Description
        +string Address
        +string City
        +string? Phone
        +string? Email
        +string? ImageUrl
        +bool IsActive
        +bool IsOpen
        +DateTime CreatedAt
        +string? OwnerId (FK)
        +ApplicationUser? Owner
        +ICollection~Inventory~ Inventories
        +ICollection~Sale~ Sales
    }

    class Medicine {
        +int Id (PK)
        +string Name
        +string? Description
        +string Category
        +decimal Price
        +int Quantity
        +DateTime? ExpiryDate
        +string? ImageUrl
        +bool RequiresPrescription
        +bool IsActive
        +DateTime CreatedAt
        +ICollection~Inventory~ Inventories
        +ICollection~PrescriptionItem~ PrescriptionItems
        +ICollection~SaleItem~ SaleItems
        +ICollection~SupplierMedicine~ SupplierMedicines
    }

    class Supplier {
        +int Id (PK)
        +string Name
        +string ContactPerson
        +string Phone
        +string Email
        +string? Address
        +bool IsActive
        +DateTime CreatedAt
        +ICollection~SupplierMedicine~ SupplierMedicines
    }

    class Inventory {
        +int Id (PK)
        +int PharmacyId (FK)
        +int MedicineId (FK)
        +int Quantity
        +int MinimumStockLevel
        +DateTime LastUpdated
        +Pharmacy Pharmacy
        +Medicine Medicine
        +bool IsLowStock [computed]
    }

    class Prescription {
        +int Id (PK)
        +string UserId (FK)
        +DateTime PrescriptionDate
        +PrescriptionStatus Status
        +string? Notes
        +string? DoctorName
        +DateTime CreatedAt
        +ApplicationUser User
        +ICollection~PrescriptionItem~ PrescriptionItems
    }

    class PrescriptionItem {
        +int Id (PK)
        +int PrescriptionId (FK)
        +int MedicineId (FK)
        +int Quantity
        +string? DosageInstructions
        +Prescription Prescription
        +Medicine Medicine
    }

    class Sale {
        +int Id (PK)
        +string UserId (FK)
        +int PharmacyId (FK)
        +DateTime SaleDate
        +decimal TotalAmount
        +SaleStatus Status
        +string? Notes
        +DateTime CreatedAt
        +ApplicationUser User
        +Pharmacy Pharmacy
        +ICollection~SaleItem~ SaleItems
    }

    class SaleItem {
        +int Id (PK)
        +int SaleId (FK)
        +int MedicineId (FK)
        +int Quantity
        +decimal UnitPrice
        +decimal Subtotal [computed]
        +Sale Sale
        +Medicine Medicine
    }

    class SupplierMedicine {
        +int Id (PK)
        +int SupplierId (FK)
        +int MedicineId (FK)
        +decimal SupplyPrice
        +int AvailableQuantity
        +DateTime? LastSupplyDate
        +Supplier Supplier
        +Medicine Medicine
    }

    %% === RELATIONSHIPS ===

    %% 1:1 - ApplicationUser owns one Pharmacy
    ApplicationUser "1" -- "0..1" Pharmacy : owns

    %% 1:N - ApplicationUser has many Prescriptions
    ApplicationUser "1" -- "*" Prescription : has

    %% 1:N - ApplicationUser has many Sales
    ApplicationUser "1" -- "*" Sale : has

    %% 1:N - Pharmacy has many Inventories
    Pharmacy "1" -- "*" Inventory : contains

    %% 1:N - Pharmacy has many Sales
    Pharmacy "1" -- "*" Sale : processes

    %% 1:N - Medicine has many Inventories
    Medicine "1" -- "*" Inventory : stocked_in

    %% 1:N - Medicine has many PrescriptionItems
    Medicine "1" -- "*" PrescriptionItem : prescribed_in

    %% 1:N - Medicine has many SaleItems
    Medicine "1" -- "*" SaleItem : sold_in

    %% 1:N - Prescription has many PrescriptionItems
    Prescription "1" -- "*" PrescriptionItem : contains

    %% 1:N - Sale has many SaleItems
    Sale "1" -- "*" SaleItem : contains

    %% M:N - Supplier <-> Medicine (through SupplierMedicine)
    Supplier "1" -- "*" SupplierMedicine : supplies
    Medicine "1" -- "*" SupplierMedicine : supplied_by
```

## Relationship Summary

| Type | From | To | Description |
|------|------|----|-------------|
| 1:1 | ApplicationUser | Pharmacy | User owns one pharmacy |
| 1:N | ApplicationUser | Prescription | User has many prescriptions |
| 1:N | ApplicationUser | Sale | User has many sales |
| 1:N | Pharmacy | Inventory | Pharmacy has many inventory items |
| 1:N | Pharmacy | Sale | Pharmacy processes many sales |
| 1:N | Medicine | Inventory | Medicine stocked in many pharmacies |
| 1:N | Medicine | PrescriptionItem | Medicine in many prescription items |
| 1:N | Medicine | SaleItem | Medicine in many sale items |
| 1:N | Prescription | PrescriptionItem | Prescription has many items |
| 1:N | Sale | SaleItem | Sale has many items |
| M:N | Supplier ↔ Medicine | SupplierMedicine | Many suppliers supply many medicines |

## Enumerations

### PrescriptionStatus
- Pending (0)
- Approved (1)
- Dispensed (2)
- Rejected (3)
- Cancelled (4)

### SaleStatus
- Pending (0)
- Completed (1)
- Cancelled (2)
- Refunded (3)
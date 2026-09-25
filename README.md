# Zuripro Medical Agency — Solution Scaffold

Clean-architecture ASP.NET Core solution: Web (Razor MVC) → Infrastructure
(EF Core + Identity) → Core (entities, interfaces — no framework dependencies).

```
MedicalSupplies.sln
├── MedicalSupplies.Core            Entities, enums, repository interfaces
├── MedicalSupplies.Infrastructure  EF Core DbContext, Fluent API configs,
│                                   Identity, repositories, DI wiring
├── MedicalSupplies.Web             Razor MVC app, admin Area, Program.cs
└── MedicalSupplies.Tests           xUnit tests (EF Core InMemory)
```

## EF Core is the source of truth

The database is generated *from* the C# entities in `MedicalSupplies.Core`,
not the other way around — this now targets **PostgreSQL via Supabase**
(see "Database: PostgreSQL via Supabase" below for the full migration
report). The earlier `ZuriproMedicalDB_Schema.sql` script is doubly
stale at this point — it was T-SQL syntax for a SQL Server database that
no longer exists — and can be deleted or ignored; nothing reads it.

## First-time setup

1. Open `MedicalSupplies.sln` in Visual Studio, or from the CLI:
   ```
   dotnet restore
   ```
2. Set the real Supabase connection string via User Secrets — **never**
   commit real credentials into `appsettings.json` (it only holds a
   placeholder):
   ```
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true" --project MedicalSupplies.Web
   ```
   `UserSecretsId` is already set in `MedicalSupplies.Web.csproj`, so this
   works out of the box.
3. Generate and apply the migration — see "Database: PostgreSQL via
   Supabase" below for the exact commands and everything about this
   change.
4. Run the app:
   ```
   dotnet run --project MedicalSupplies.Web
   ```
   On first startup, `SeedData.SeedRolesAsync` creates the five roles:
   `SuperAdmin`, `Admin`, `Sales`, `InventoryManager`, `Customer`.
   No default admin user is seeded — create the first SuperAdmin
   through registration + a one-off role assignment, rather than a
   hard-coded password in source control.

## Product Catalogue V1 (built)

**Admin (`/Admin/...`, role-protected):**
- `Categories`, `Brands` — full CRUD, block delete when products still reference them
- `Products` — CRUD with category/brand dropdowns, specifications, reorder level,
  a `RequiresBatchTracking` toggle, and multi-image upload (saved to
  `wwwroot/images/products`, one flagged primary) via `IFileUploadService`
- `InventoryBatches` — record a received batch (number, expiry, quantity,
  supplier); this also writes a `StockMovement` row, so stock and its audit
  trail move together from day one
- Product list shows live stock (summed from active batches) with a
  "Low stock" badge when it's at/under `ReorderLevel`; batch list highlights
  anything expiring within 90 days

**Public:**
- `/Products` — search by name/code, filter by category and brand
- `/Products/Details/{id}` — images, specs, "Price: Request a Quote", related
  products from the same category
- `/Quotation` — add-to-list from the details page (no login required),
  a cart-style review page, then a contact-details form that creates a real
  `Customer` (matched by email) + `Quotation` + `QuotationDetails` row —
  this is a working request-for-quotation flow, not a stub

## Phase 2 — Admin Quotation Management (built)

`QuotationStatus` is now `Pending → Pricing → Sent → Accepted / Rejected / Expired → ConvertedToOrder`.

**`/Admin/Quotations`:**
- List filterable by status
- Pricing screen: per-line admin price against the requested quantity
  ("Requested Price" column is always "—" — customers request quantities,
  never propose a price), a flat discount, delivery location, an internal
  notes field, and a validity date. Subtotal/discount/grand total recompute
  live in the browser as you type; the server recalculates authoritatively
  on save regardless of what the client sent.
- **Save Quote** → status `Pricing`. **Send to Customer** → status `Sent`
  (also stamps `PreparedDate`). Once a quotation is `Accepted`, `Rejected`,
  `Expired`, or `ConvertedToOrder`, the pricing form becomes read-only.
- **Accept / Reject** appear once a quotation has been sent — these record
  that *you* were told the customer accepted or declined (by phone, email,
  WhatsApp — there's no customer-facing accept/reject UI yet).
- **Convert to Order** is its own explicit button, shown only when a
  quotation is `Accepted` — exactly as you asked: accepting a quotation
  never auto-creates an order, so there's no risk of a duplicate order
  from a page refresh, and every order has a real audit trail back to
  the quotation and the admin action that created it. Order numbers are
  generated as `MED-{year}-{6-digit sequence}`.

**`/Admin/Orders`:** read-only for now — list and a details page showing
line items and the grand total, just enough to confirm `Convert to Order`
worked. Status changes (Processing/Dispatched/Delivered), payment
tracking, etc. are Phase 3, Order Management.

## Phase 3 — Order Management (built)

`PaymentStatus` is now `Pending / PartiallyPaid / Paid / Refunded`, kept
fully independent of `OrderStatus` — an order can be `Dispatched` with
payment still `PartiallyPaid`, exactly as you described. The old
`DeliveryStatus` enum was dropped as redundant once `OrderStatus` itself
covers Dispatched/Delivered; `DispatchedDate`/`DeliveredDate` timestamps
plus `DeliveryNotes` replace it.

**`/Admin/Orders`:**
- List filterable by order number, customer, status, payment status, and
  a date range
- Details page: line items, subtotal/discount/total, customer info,
  current status and payment side by side, and a full **status history**
  table (`OrderStatusHistory` — one row per change, with who changed it
  and any notes) — the audit trail you asked for
- **Status transitions are whitelisted, not arbitrary:**
  `Created → Processing → Dispatched → Delivered`, with `Processing →
  Cancelled` also allowed and nothing else. The dropdown on the details
  page only ever offers the statuses actually reachable from where the
  order is now.
- **Payment is a separate form** — status + amount paid, saved
  independently of order status.

**Inventory deduction — the important part:** stock is untouched at
quotation creation *and* at order creation. It's only deducted the moment
an order moves to **Processing**, allocated earliest-expiry-first across
that product's active batches, with a `StockMovement` (type
`OrderProcessing`) written per batch touched — `StockMovements` is the
sole record of which batch(es) fulfilled a line (see "StockMovements as
the single source of truth" below). If stock is insufficient, the
transition is rejected outright — nothing is partially deducted. If a
`Processing` order is then **Cancelled**, the original movements are
looked up and reversed with brand-new `OrderCancellation` movements
(the originals are never edited). An order cancelled before it ever
reached `Processing` never touched stock in the first place.

## Supplier Management (built)

`Supplier` gained `SupplierCode` (unique), `TaxNumber`, and `Notes`; the old
`Country`-after-name field order is now Contact → Phone/Email → Address →
Country → Tax Number, matching the details layout below.

**`/Admin/Suppliers`:**
- List with a search box (name/code) and an Active/Inactive status filter
- Add / Edit — `SupplierCode` is validated unique on both
- Details — contact info, then two separate sections: **Supplied
  Products** (distinct products + batch count + current available
  quantity — "which products did we buy from this supplier?") and
  **Inventory Batches** (every individual batch — "which specific batches
  came from this supplier?")
- **Deactivate / Reactivate** instead of delete — a batch's `Supplier`
  link (e.g. "Meditech Ltd.") stays intact and readable even after
  Meditech itself is deactivated, exactly as intended
- The supplier dropdown on `/Admin/InventoryBatches/Create` now has
  something to select

## Customer Accounts (built)

Uses ASP.NET Core Identity's actual auth — no parallel password system.
`AccountController` sits at the framework's default cookie paths
(`/Account/Login`, `/Account/Register`, `/Account/AccessDenied`), so no
extra cookie configuration was needed.

- **Register** — full name, organization (optional — covers hospitals,
  clinics, labs, pharmacies, NGOs, distributors and individual
  professionals alike), email, phone, address, password. Creates the
  `ApplicationUser`, adds them to the `Customer` role, and links (or
  creates) their `Customer` record. If they'd already requested a
  quotation as a guest with the same email, that existing `Customer` row
  gets linked instead of a duplicate being created.
- **Login / Logout** — standard Identity sign-in.
- **`/Account/Dashboard`** — quotation/order/pending counts, recent orders
- **`/Account/Quotations`** + **`/Account/QuotationDetails/{id}`** — their
  own quotations only, admin pricing screen fields (internal notes, etc.)
  simply aren't in this view model at all
- **`/Account/Orders`** + **`/Account/OrderDetails/{id}`** — order
  progress shown as filled/empty circles per your sketch; the details
  view model has no `Supplier`, purchase price, `StockMovement`, or admin
  notes fields to leak in the first place
- **`/Account/Profile`** — edit contact details (email itself is
  read-only here — changing it needs its own confirmation-token flow,
  not built yet)

**Ownership is enforced at the query, not after the fact.** Every lookup
is `.Where(x => x.CustomerId == <the signed-in user's own Customer row>)`
— `order.Customer.UserId == currentUserId` is baked into the query itself,
so `/Account/OrderDetails/25` for someone else's order returns a plain
404, never a 403 that would confirm order 25 exists.

**`Quotation`/`Order` link to the customer through `Customer.UserId`**,
not a separate `UserId` column on each — one normalized 1:1 relationship
(`AspNetUsers ↔ Customer`) rather than duplicating the id everywhere.
Functionally identical to what you sketched, just without the redundant
column.

The public "Request a Quotation" flow now recognizes a signed-in visitor:
the form pre-fills from their account and the quotation links straight to
their existing `Customer` row instead of being matched by email.

## Supplier Purchase Orders (built)

`PurchaseOrderStatus` is now `Draft → Submitted → Approved →
PartiallyReceived → Received`, cancellable any time before `Received`.
`StockMovementType` gained `PurchaseReceipt` — kept distinct from the
existing `Purchase` (an ad-hoc batch entered by hand, no PO behind it),
so the two procurement paths stay traceable separately.

**`/Admin/PurchaseOrders`:**
- List, filterable by status
- **Create** — pick a supplier, add product lines (product/quantity/unit
  cost) via a dynamic add/remove table with a live-updating subtotal,
  plus delivery cost, tax, currency, and notes. Saves as **Draft**.
- **Edit** — Draft only; lines are wholesale-replaceable since nothing's
  been received yet. Once `Submitted`, lines lock — editing redirects
  back to Details with an error instead.
- **Status transitions are whitelisted**, same pattern as Orders:
  `Draft → Submitted → Approved`, `Cancelled` from anywhere before
  `Received`. **`PartiallyReceived` and `Received` are never reachable
  from this dropdown at all** — the only way to reach them is to actually
  receive goods.
- **Payment to supplier** tracked separately from receiving status —
  same `Pending/PartiallyPaid/Paid/Refunded` enum as customer orders,
  updated independently.

**Receiving — the part that matters:** each PO line gets its own
**Receive** button (shown once the PO is `Approved` or `PartiallyReceived`,
and only while that line still has quantity outstanding). The receiving
screen shows Ordered / Already Received / Remaining and a dynamic table
of batch rows (batch number, expiry, quantity) — add as many as the
delivery actually arrived in. The total entered:
- is **rejected server-side** if it's ≤ 0 or exceeds what's still
  outstanding on that line (client-side JS flags this red as you type,
  but the server check is what actually stops it)
- on success, creates **one `InventoryBatch` per row** (`PurchaseOrderId`
  and `SupplierId` set for traceability, `PurchasePrice` taken from the
  PO line's unit cost) and **one `StockMovement` per batch**
  (`PurchaseReceipt`, `PurchaseOrderId` set)
- updates that line's `QuantityReceived`, then recomputes the **whole
  PO's** status from every line's totals — `Received` only once every
  line is fully received, `PartiallyReceived` otherwise. This is why a
  1,000-glove order delivered as 600-then-400-later works exactly as
  described: each receiving session only has to account for what's
  arriving *in that session*, not the line's full original quantity.

**Traceability, end to end:** `Supplier → PurchaseOrder →
PurchaseOrderDetail → InventoryBatch → StockMovement`, and from there
straight into the existing FEFO allocation and reversal logic from
Order Management — completely unchanged by this phase, exactly as asked.

**Financial data never reaches the public/customer side** — not because
anything was filtered out, but because `PurchaseOrder`, `Supplier`
purchase prices, and `StockMovement` were never referenced by
`ProductsController`, `QuotationController`, or `AccountController` in
the first place. There's nothing to leak because the public and customer
code paths simply don't touch those tables.

## StockMovements as the single source of truth (built)

`OrderDetail` no longer has a `BatchId`. It never should have carried one
alongside `StockMovements`, which already had the authoritative per-batch
breakdown — two places that could theoretically disagree about which
batch fulfilled a line. Now there's exactly one:

```
Order
 └── OrderDetail (OrderDetailId, OrderId, ProductId, Quantity, UnitPrice, TotalPrice)
       └── Product

StockMovement (OrderId, OrderDetailId, BatchId, ProductId, QuantityChange, MovementType, ...)
```

A 500-unit line split across three batches now shows up exactly as you'd
want: one `OrderDetail` row for the 500, and three `StockMovement` rows
underneath it (Batch A → -200, Batch B → -200, Batch C → -100), each
tagged `OrderDetailId` so it's traceable back to the specific line.

**`StockMovement` itself changed too** — the old generic
`ReferenceType`/`ReferenceId` string+int pair is gone, replaced with
typed nullable FKs: `OrderId`, `OrderDetailId`, `PurchaseOrderId`. Exactly
one of `Order`/`PurchaseOrder` is populated depending on `MovementType`;
a purely manual batch entry (no PO behind it) leaves all three null.

**Movement types renamed** for clarity: `Sale` → `OrderProcessing`,
`Return` → `OrderCancellation` (kept distinct from a plain `Return`,
which is now reserved for a *physical* return of already-delivered
goods — a different real-world event, not built yet). `Purchase`,
`PurchaseReceipt`, `Adjustment`, `Damage`, `Expiry` are unchanged.

**Reversals now point back at what they reverse.** `StockMovement`
gained a self-referencing `ReversesStockMovementId`. Cancelling a
`Processing` order still never edits the original `OrderProcessing`
row — it creates a new `OrderCancellation` row with
`ReversesStockMovementId` set to the original's id, so the audit trail
reads as an actual chain (`#108 OrderCancellation +200, reverses #101`)
rather than requiring you to infer the link by matching quantities.

`OrderFulfillmentService`, `InventoryBatchesController` (manual batch
entry), and `PurchaseOrdersController` (PO receiving) were all updated
to the new fields — the FEFO allocation logic itself (sort by expiry,
check-then-allocate, all-or-nothing) is unchanged.

## Deliberately left for later

- Email confirmation / password reset for customer accounts
- No NuGet packages are restored/built in this environment — restore and
  build locally where you have network access to nuget.org and to your
  Supabase Postgres database
- Task 2 (Supabase Storage for product images) and Task 3 (Supabase Auth)
  — deliberately not started; ASP.NET Core Identity and local disk image
  storage are both still exactly as they were

## Database: PostgreSQL via Supabase (built)

Switched the data layer from SQL Server to PostgreSQL (Supabase-hosted),
via the `Npgsql.EntityFrameworkCore.PostgreSQL` provider. ASP.NET Core
Identity, the MVC architecture, and all business logic are untouched —
this was a database-layer swap only, per the brief.

**A. Every file changed**
- `MedicalSupplies.Infrastructure/MedicalSupplies.Infrastructure.csproj`
  — swapped the package reference
- `MedicalSupplies.Infrastructure/DependencyInjection.cs` — `UseSqlServer`
  → `UseNpgsql`
- `MedicalSupplies.Web/appsettings.json` — connection string replaced
  with a Postgres-format placeholder (no real credentials)
- `MedicalSupplies.Core/Entities/PurchaseOrder.cs` — see "issues found," below
- `MedicalSupplies.Web/Areas/Admin/Controllers/PurchaseOrdersController.cs`
  — three call sites updated to match the fix above
- `MedicalSupplies.Web/Areas/Admin/Controllers/OrdersController.cs` — two
  lines fixed for Npgsql's strict UTC handling, see below

**B. NuGet packages**
- Removed: `Microsoft.EntityFrameworkCore.SqlServer` (8.0.8)
- Added: `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.11) — the current
  published version at the time of writing; since this environment can't
  reach nuget.org, verify it's still current when you `dotnet restore`
  locally and bump it if a newer 8.0.x patch exists
- Everything else (`Microsoft.EntityFrameworkCore.Design`,
  `Microsoft.EntityFrameworkCore.Tools`,
  `Microsoft.AspNetCore.Identity.EntityFrameworkCore`,
  `Microsoft.EntityFrameworkCore.InMemory` in Tests) is provider-agnostic
  and unchanged

**C. PostgreSQL-specific model/configuration changes**
Reviewed the entire entity model and every `Configurations/*.cs` file
against your checklist. Result: almost nothing needed to change, because
nothing in the model ever used a SQL Server–specific type, annotation, or
raw SQL function.
- **Decimal precision** — all 17 `HasColumnType("decimal(18,2)")` calls
  work unchanged: PostgreSQL treats `decimal` as a synonym for `numeric`,
  so this string is valid Postgres DDL as-is.
- **DateTime/UTC handling** — every entity's non-nullable `DateTime`
  already defaulted to `DateTime.UtcNow` (`Kind = Utc`), which Npgsql
  requires for `timestamp with time zone` columns. Two places did **not**
  meet that bar and were genuine bugs, not just style — see "issues
  found" below.
- **Guid/UUID** — the model has none (all primary keys are `int`
  identity columns; Identity's own keys are `string`). Nothing to do —
  Npgsql generates PostgreSQL identity columns for `int` PKs by the same
  EF Core convention SQL Server used, no entity changes needed.
- **String lengths, nullability, booleans, indexes, foreign keys,
  cascade/restrict behavior** — all expressed through provider-agnostic
  Fluent API (`HasMaxLength`, nullable `?` types, `HasIndex`,
  `OnDelete(DeleteBehavior.…)`), which Npgsql translates natively.
  The `Restrict`/`SetNull` choices made throughout (particularly around
  `StockMovement`'s multiple FKs, to avoid a multiple-cascade-paths error)
  apply identically on Postgres.
- **Identity tables** — standard ASP.NET Core Identity + Npgsql is a
  well-supported combination; no changes needed beyond the provider swap.
- **PostgreSQL reserved words** — checked every table/column name against
  Postgres's reserved list. The only near-miss is the `Orders` table —
  `ORDER` (singular) is reserved, `ORDERS` is not, so no collision. In any
  case, EF Core's Npgsql provider double-quotes every identifier it
  generates, which sidesteps reserved-word conflicts entirely regardless.
- **No SQL Server–specific column types or annotations existed anywhere**
  in the model (no `nvarchar`, `rowversion`, `GETUTCDATE()`, `NEWID()`, or
  raw `IDENTITY(1,1)`) — confirmed by grepping the whole solution.

**Issues found and fixed while reviewing (not cosmetic — these would have
blocked the build or failed at runtime under Postgres):**
1. **`PurchaseOrder.ExpectedDeliveryDate` was typed `DateTime?`** while
   both ViewModels that read/write it (`PurchaseOrderFormViewModel`,
   `PurchaseOrderDetailsViewModel`) were `DateOnly?`. The Create action
   (`ExpectedDeliveryDate = vm.ExpectedDeliveryDate`) assigned `DateOnly?`
   directly to a `DateTime?` property — **not valid C#, there's no
   implicit conversion** — so the solution could not have compiled as it
   stood. The other three call sites papered over the mismatch with
   `.ToDateTime(TimeOnly.MinValue)` / `DateOnly.FromDateTime(...)`, which
   would have compiled but produced `Kind = Unspecified` DateTimes — fine
   for SQL Server, but Npgsql throws at runtime writing those into a
   `timestamptz` column. Fixed by changing the entity property to
   `DateOnly?` (consistent with `Quotation.ValidUntil` and
   `InventoryBatch.ExpiryDate`, which were already `DateOnly?`) and
   removing the now-unnecessary conversions at all four call sites.
2. **`OrdersController.Index`'s date-range filter** built its `from`/`to`
   bounds with `filter.FromDate.Value.ToDateTime(TimeOnly.MinValue)` —
   also `Kind = Unspecified`. Since these get parameterized into the
   `WHERE o.OrderDate >= @from` query, this would throw the same Npgsql
   exception the moment anyone filtered orders by date. Fixed with
   `DateTime.SpecifyKind(..., DateTimeKind.Utc)`.

Neither of these is a SQL-Server-vs-Postgres design difference — they're
pre-existing defects that SQL Server's looser typing let slide and
Postgres's stricter `timestamptz` handling would have caught immediately.
Fixing them was in scope under your rule 9 ("unless required for a
compile fix").

**D. Exact connection-string configuration**

`appsettings.json` now holds only a placeholder:
```
Host=YOUR-SUPABASE-HOST;Port=5432;Database=postgres;Username=postgres;Password=YOUR-PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```
Set the real value via User Secrets (already wired — `UserSecretsId` has
existed in `MedicalSupplies.Web.csproj` since Phase 1) or an environment
variable, never in a committed file:
```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=...;SSL Mode=Require;Trust Server Certificate=true" --project MedicalSupplies.Web
```
or in production, the environment-variable form ASP.NET Core reads
automatically: `ConnectionStrings__DefaultConnection`.

Get the actual host/username from Supabase → Project Settings →
Database → Connection string → "ADO.NET" (or build it from the
Host/Session-pooler values shown there — see item G).

**E. Exact command to create the initial PostgreSQL migration**

There is currently **no migration history at all** — the `Migrations/`
folder is empty. Every migration named in earlier README revisions
(`InitialCreate`, `AddQuotationPricingAndOrderManagement`, etc.) was
planning language for commands to run locally; none were ever actually
generated in this environment (no `dotnet`/NuGet access here). So there's
nothing to delete — just generate one clean migration against the
current, final entity model:
```
dotnet ef migrations add InitialCreate --project MedicalSupplies.Infrastructure --startup-project MedicalSupplies.Web
```

**F. Exact command to apply it to Supabase**
```
dotnet ef database update --project MedicalSupplies.Infrastructure --startup-project MedicalSupplies.Web
```
This creates every application table plus the ASP.NET Core Identity
tables (`AspNetUsers`, `AspNetRoles`, etc.) directly in your Supabase
Postgres database, using the connection string from User Secrets/the
environment.

**G. Manual Supabase dashboard configuration**
- No manual table/schema setup needed — the migration creates everything.
- Get the connection details from **Project Settings → Database**. Use
  the **direct connection** (port `5432`) for this app, not the
  Transaction-mode pooler (port `6543`) — see the pooler warning below.
- Nothing else is required for this task specifically — no storage
  buckets, no RLS policies, no Auth configuration (those are Tasks 2 and
  3, deliberately not touched here). If your Supabase project has Row
  Level Security enabled by default on new tables, note that RLS is a
  Postgres feature that applies to *any* connection, including this
  app's — but since the app connects as the Postgres role and issues
  ordinary SQL through EF Core (not through Supabase's REST/PostgREST
  layer), RLS policies don't apply to it unless you've specifically
  configured the connecting role to be subject to them. Worth confirming
  in the dashboard if table access behaves unexpectedly.

**H. Potential issues found**
- **Connection pooler mode.** Supabase offers a PgBouncer connection
  pooler in *transaction* mode (port 6543), which does not support
  prepared statements the way EF Core/Npgsql use them by default —
  you'll see errors like `prepared statement "..." already exists`
  under load. For a normal long-running ASP.NET Core app (not a
  serverless function), connect directly on port `5432`, or if you must
  use the pooler, use its *session*-mode endpoint, or add
  `Max Auto Prepare=0` to the Npgsql connection string to disable
  server-side prepared statements.
- **SSL is mandatory.** Supabase Postgres refuses plain connections;
  `SSL Mode=Require` is not optional in the connection string.
- **The two DateTime bugs above** were real and would have surfaced the
  first time someone filtered orders by date or set a PO's expected
  delivery date — worth being aware these existed even though they're
  fixed now.
- **Case-sensitive identifiers.** EF Core's Npgsql provider preserves and
  quotes your PascalCase table/column names (`"Products"`,
  `"ProductCode"`), so raw SQL you write by hand later (in Supabase's SQL
  editor, for instance) needs matching double-quotes — `SELECT * FROM
  "Products"`, not `SELECT * FROM Products` (which Postgres would
  otherwise fold to lowercase `products` and fail to find).
- **Not tested against a live Supabase database.** This environment has
  no network access to Supabase or to nuget.org, so nothing above has
  actually been run — no `dotnet restore`, no `dotnet ef migrations add`,
  no connection attempt. Everything here is a source-code and
  configuration change, reviewed carefully, but only you running the
  commands in E and F locally will confirm it actually connects and
  migrates cleanly.

**I. Code that still assumes SQL Server**
None, after the changes above. The full-solution grep for
`UseSqlServer`, `SqlServer` package references, `nvarchar`,
`rowversion`, `GETUTCDATE()`, `NEWID()`, and raw `IDENTITY(...)` syntax
came back empty except for the one `UseSqlServer` call and the one
package reference, both now changed. `MedicalSupplies.Tests` uses EF
Core's InMemory provider — never touched SQL Server and needs no change
for this migration.

## Next step

The core loop your original spec described is now complete end to end:
catalogue → quotation → admin pricing → customer acceptance → order →
FEFO stock allocation → dispatch → delivery → customer order history,
with the supplier side (PO → receiving → batch → stock) feeding the same
inventory. From here it's your call — candidates include email
confirmation/password reset, admin reporting (sales, inventory, expiring
stock), or WhatsApp/email notifications at key workflow steps.

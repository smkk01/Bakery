# Bakery API

ASP.NET Core 8 Web API with a clean-architecture split, JSON-file product persistence, Swagger, Serilog, CORS, global problem-details exception handling, and a health endpoint.

## Projects

- `Bakery.Domain` — product entity and domain types.
- `Bakery.Application` — product use cases, repository contract, and application DI.
- `Bakery.Infrastructure` — JSON-file repository and infrastructure DI.
- `Bakery.Api` — HTTP controllers, middleware, hosting, and API configuration.

The domain includes validated `Category`, `Product`, `Customer`, `Order`, and `OrderItem` models, a shared `BaseEntity`, and the `OrderStatus` enum. Application DTOs and mapping extensions are in `Bakery.Application/DTOs` and `Bakery.Application/Mapping`.

## Run locally

```powershell
dotnet restore Bakery.sln
dotnet run --project Bakery.Api
```

Swagger UI is available at `/swagger`; the health endpoint is `/health`. Configure local frontend origins in `Bakery.Api/appsettings.Development.json`.

## React frontend

The React, TypeScript, and Vite frontend is in `Bakery.Client`. It uses Bootstrap 5, Tailwind CSS 4, Axios, React Router, TanStack React Query, and Context API. Routes include Home (`/`), Products (`/products`), Product Details (`/products/:id`), Cart (`/cart`), Checkout (`/checkout`), Login (`/login`), and Admin (`/admin`). Shared layout components include Navbar, Footer, Breadcrumb, and LoadingSpinner. Configure `VITE_API_BASE_URL` in `Bakery.Client/.env.local` (copy `.env.example`; the default API address is `http://localhost:5247`), then run:

```powershell
cd Bakery.Client
npm install
npm run dev
```

Open the Vite URL printed by the dev server (normally `http://localhost:5173`). The storefront reads the public product catalog; login, customer registration, cart requests (including quantity updates and item removal), checkout, and admin catalog/order management use the API. The JWT-protected admin dashboard provides searchable, sortable, paginated product, category, and order tables; product/category CRUD and order status updates require an Admin token. Checkout routes are `/checkout/shipping`, `/checkout/review`, and `/checkout/confirmation`; the customer and non-empty basket are required. Shipping information is validated and held in memory for this flow because the current order API accepts only customer ID and product quantities, so the address is not persisted or transmitted as part of the order. Cart and checkout require a Customer token. Authentication state is kept in memory and clears on page refresh/sign-out. Ensure the API's CORS allowed origins include the Vite origin when using a different port.

JSON-backed `IProductRepository`, `ICategoryRepository`, `IOrderRepository`, and `ICustomerRepository` implementations are registered by infrastructure DI. Each provides asynchronous CRUD operations over its corresponding JSON file. The product API supports `GET /api/products`, `GET /api/products/{id}`, `POST /api/products`, `PUT /api/products/{id}`, and `DELETE /api/products/{id}`. The category API supports `GET /api/categories`, `GET /api/categories/{id}`, `POST /api/categories`, `PUT /api/categories/{id}`, and `DELETE /api/categories/{id}`. Category request bodies require a name (up to 100 characters) and description (up to 1000 characters). Swagger includes example request bodies for products, categories, and cart operations.

At startup, `StartupDataSeeder` seeds five predefined categories (Cakes, Cupcakes, Cookies, Bread, Pastries) and three products per category using stable IDs. If any categories already exist, category insertion is skipped. Product seeding runs after category handling and is skipped if any products already exist. If categories already exist without the predefined IDs, products are matched to existing categories by name; products for missing categories are logged and skipped rather than being created with invalid category references. Seeding uses asynchronous repository calls and logs insertions and skip decisions.

## Authentication and authorization

- Register customers at `POST /api/auth/register` and sign in at `POST /api/auth/login`. Registration always assigns the `Customer` role; clients cannot self-assign `Admin`. Passwords must be 12-128 characters and contain a letter and a digit. Password hashes are PBKDF2-SHA256 hashes; plaintext passwords are never saved.
- Configure `JWT__SIGNINGKEY` with a random secret of at least 32 UTF-8 bytes outside development. Without a key, development generates a temporary key at startup; tokens from that process stop working after restart. Production startup fails if the key is missing. JWT issuer, audience, and access-token lifetime are configured in `Jwt`.
- Optionally bootstrap one administrator by configuring all of `BootstrapAdmin__FullName`, `BootstrapAdmin__Email`, `BootstrapAdmin__Phone`, and `BootstrapAdmin__Password` before startup. To explicitly promote an existing customer account instead, set `AdminPromotion__Email` to that account's email; its current password is retained. Remove the promotion setting after the app restarts. Use secure App Service settings/Key Vault and do not commit secrets.
- Product and category write endpoints require `Admin`. Cart endpoints require `Customer` and only allow access to the authenticated customer's cart. Order creation requires `Customer`; customers can only list/read their own orders. Public product/category reads remain anonymous; order reads require authentication. Use the Swagger **Authorize** button with `Bearer <accessToken>`.
- Register and log in with JSON such as `{"fullName":"Alex Baker","email":"alex@example.com","phone":"+15551234567","password":"BakerPass2026!"}` and `{"email":"alex@example.com","password":"BakerPass2026!"}` respectively. The response includes `accessToken`; send it as `Authorization: Bearer <accessToken>`.

Cart routes use a `customerId` to access the customer's persistent cart. Use `GET /api/cart?customerId={customerId}`, `POST /api/cart/add`, `PUT /api/cart/update`, and `DELETE /api/cart/remove`. Add/update bodies contain `customerId`, `productId`, and a quantity from 1 to 10,000; remove bodies contain `customerId` and `productId`. Before adding to a cart, create a catalog product with `POST /api/products` and use the returned `id`, or select an existing ID from `GET /api/products`. The Swagger example GUIDs are placeholders and are not pre-seeded products; an unknown product ID returns `404 Product not found`. Adding an existing product increments its quantity. Product name and unit price are read from the catalog rather than accepted from the client. Cart totals are calculated from the saved unit-price snapshots; `total` currently equals `subtotal` because tax and delivery rules are not configured. Carts are persisted in `data/carts.json`.

Order management supports `POST /api/orders`, `GET /api/orders`, `GET /api/orders/{id}`, and `GET /api/orders/number/{orderNumber}`. `GET /api/orders?customerId={customerId}` filters orders by customer. Create requests provide a customer ID and product IDs with quantities. The API validates that products exist and are available, calculates totals from catalog prices, snapshots product names and prices into order items, and generates a `BK-YYYYMMDD-{GUID}` order number. The order is persisted in `data/orders.json`.

A product create/update body looks like:

```json
{
  "categoryId": "11111111-1111-1111-1111-111111111111",
  "name": "Sourdough loaf",
  "description": "Slow-fermented country sourdough",
  "price": 8.50,
  "imageUrl": "https://example.com/sourdough.jpg",
  "isAvailable": true
}
```

## Configuration and Azure deployment

- Set `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc. to the exact HTTPS origins of the deployed frontend. The default production list is empty (same-origin requests continue to work; cross-origin requests are denied).
- Production settings are in `Bakery.Api/appsettings.Production.json`. Set Azure App Service's `ASPNETCORE_ENVIRONMENT` to `Production`; do not put deployment secrets in this file.
- Set `JWT__SIGNINGKEY` to a high-entropy secret of at least 32 UTF-8 bytes. Configure all `BootstrapAdmin__FullName`, `BootstrapAdmin__Email`, `BootstrapAdmin__Phone`, and `BootstrapAdmin__Password` settings to create a new initial administrator, or set `AdminPromotion__Email` to explicitly promote an existing customer account. Remove `AdminPromotion__Email` after the app restarts. Store secrets as App Service application settings or Key Vault references.
- Set `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc. to the exact HTTPS frontend origins. Production's default CORS list is empty.
- Set `Data__DirectoryPath` to a persistent writable directory: `/home/data` for Linux App Service or `D:\home\data` for Windows App Service. The local default is `data` under the API content root and contains `products.json`, `categories.json`, `orders.json`, `customers.json`, and `carts.json`. Ensure App Service storage is enabled. Keep the app to one instance when using JSON files; this store is not shared across scaled-out workers.
- Swagger is disabled by default in `appsettings.Production.json`. To expose it, explicitly set `Swagger__Enabled=true` and protect the documentation appropriately.
- App Service terminates HTTPS. Enable HTTPS Only and use the App Service ASP.NET Core hosting integration for forwarded headers when enabling HTTPS redirection behind the proxy.
- JSON-file persistence is suitable for a single instance with a persistent writable volume. It is not a shared database or multi-instance store; use a managed database before scaling the app horizontally.

Serilog writes structured logs to the console, which integrates with Azure App Service log streaming and Azure Monitor.

### Azure App Service deployment

1. Create an App Service with the **.NET 8** runtime stack. Choose Linux or Windows and use the corresponding persistent data path above. Enable HTTPS Only, Always On (recommended for production), and App Service logging/application logging as appropriate.
2. Under **Configuration > Application settings**, add:
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `JWT__SIGNINGKEY` = a random secret (at least 32 bytes)
   - `Data__DirectoryPath` = `/home/data` (Linux) or `D:\home\data` (Windows)
   - `Swagger__Enabled` = `false` (recommended default)
   - `Cors__AllowedOrigins__0` = your frontend origin, for example `https://shop.example.com`
   - Optionally, all four `BootstrapAdmin__FullName`, `BootstrapAdmin__Email`, `BootstrapAdmin__Phone`, and `BootstrapAdmin__Password` settings to seed an initial administrator, or `AdminPromotion__Email` to promote an existing customer
   
   Add secrets through App Service settings or Key Vault references, never source files. App Service application settings are exposed to .NET configuration with `__` representing `:`. Remove `AdminPromotion__Email` after promotion and restart the app.
3. Save settings and restart the App Service. Check `https://YOUR-APP.azurewebsites.net/health` for `Healthy`. Review **Log stream** or Azure Monitor if startup fails; missing production JWT signing keys and incomplete bootstrap-admin settings intentionally fail startup.
4. Configure secrets for the GitHub `production` environment (or repository secrets) `AZURE_WEBAPP_NAME` (the App Service name) and `AZURE_WEBAPP_PUBLISH_PROFILE` (the complete XML file downloaded from the target App Service's **Get publish profile** action). The deployment workflow uses this publish profile directly, so it does not need a separate `azure/login` step. Do not paste the XML into workflow files or logs.
5. Push to `main` or run **Actions > Build and deploy Bakery API > Run workflow**. [.github/workflows/azure-webapp.yml](.github/workflows/azure-webapp.yml) restores, builds, tests, publishes the API, and deploys the publish output with `azure/webapps-deploy`.
6. After deployment, test `/health`, `/swagger` (disabled unless enabled in settings), and the authentication endpoints. Deploy a managed database before adding multiple App Service instances or requiring durable transactional storage.

#### Publish profile structure

The checked-in [AzureAppService.pubxml.example](Bakery.Api/Properties/PublishProfiles/AzureAppService.pubxml.example) contains only non-secret placeholders and shows the profile shape. Replace the app name, resource group, and SCM URL for a local Visual Studio publish profile if needed. Do not add a downloaded `.PublishSettings` file or a profile containing passwords/tokens to source control. For GitHub Actions, store the actual downloaded publish-profile XML only in the protected `AZURE_WEBAPP_PUBLISH_PROFILE` repository/environment secret; the workflow deploys with that secret and does not require a credential-bearing `.pubxml` file.

# GPermiter

Gallagher permitting solution with ASP.NET Core Identity-based role access.

## Roles
- **EndUser**: self-registration enabled.
- **User**: managed account intended for internal user workflows.
- **Admin**: can create and delete users.

## Default admin bootstrap
Set `AdminAccount:Email` and `AdminAccount:Password` in configuration. On startup, the app seeds roles and creates the admin user (if missing).

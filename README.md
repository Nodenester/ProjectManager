# ProjectManager

> **Built in December 2024** -- This is an archived project and is no longer actively maintained.

---

A GitHub project management dashboard built with ASP.NET Core 8 and Blazor Server. Authenticate via GitHub OAuth, browse your repositories, view commits, contributors, and file contents -- all from a single-page web UI.

## Features

- **GitHub OAuth login** with whitelist-based access control
- **Repository browser** -- lists all repos for the authenticated user
- **Project details view** with tabbed interface (Info, Create, Test)
- **Commit history** and contributor stats per repository
- **File explorer** -- browse repository contents via the GitHub API
- **Resizable sidebar panels** (chat sidebar, commits sidebar) with drag handles

## Tech Stack

- **Framework**: ASP.NET Core 8 / Blazor Server (interactive SSR)
- **Auth**: GitHub OAuth via `AspNet.Security.OAuth.GitHub`
- **Database**: SQL Server (LocalDB) with Entity Framework Core 9
- **UI**: Bootstrap 5, custom CSS
- **Language**: C# 12

## Setup

1. Register a GitHub OAuth App and set the callback URL to `/signin-github`
2. Update `appsettings.json` with your `ClientId` and `ClientSecret`
3. Run migrations: `dotnet ef database update`
4. Start: `dotnet run`

## License

MIT

---

*This project is archived and provided as-is. December 2024.*

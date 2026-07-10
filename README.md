# james-habits-app

A simple personal habit tracker. One screen: "did I do my habits today?" —
tick the checkboxes, type the numbers (sleep hours, drinks), see your streaks.

> Goal (issue #1): _"keep simple, at the core just track my habits."_

**Design & scope:** [`docs/DESIGN.md`](docs/DESIGN.md).

## Stack

Built on the org web-template — .NET 10 + EF Core + PostgreSQL backend,
React + Vite + TypeScript + RTK Query frontend, Helm/Kubernetes deploy via
oke-fleet (ArgoCD).

## Layout

- `backend/` — layered .NET solution (`Balenthiran.Habits.slnx`): Abstractions,
  DataModels, EntityModels, DomainModels, Database (EF), Services, WebApi.
- `frontend/` — React SPA with a generated API client.
- `docs/` — [`DESIGN.md`](docs/DESIGN.md) plus the web-template process specs.
- `helm/` — deployment chart.

## Develop

Backend build/test in this sandbox go through the ICU wrapper
(`node <brain>/tools/dotnet.js build backend/Balenthiran.Habits.slnx`); on a
normal machine plain `dotnet` works. Frontend: `cd frontend && npm ci && npm run dev`.

Branch → PR into `dev` (never straight into `main` — branch protection blocks it).

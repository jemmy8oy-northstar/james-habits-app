# james-habits-app — Design

_Personal habit tracker. Goal (James, issue #1): **"keep simple, at the core
just track my habits."** Web app first (iterate fast), mobile-friendly layout,
single user. Idea + greenlight: issue #1; process context: claude-code-bot#11._

## Product shape

One screen that answers "did I do my habits today?" and lets James log the
whole day in a few seconds. A habit is either a **checkbox** ("Ran", "Read")
or a **number** ("Sleep 7.5h", "Drinks 2"). Tap to tick, type a number, done.
Streaks and a small history give the sense of momentum that makes tracking
stick.

The **daily loop is the product** — everything else (managing the habit list,
history) is secondary. The design constraint that outranks any feature: **the
full day must be loggable from one screen in under ~20 seconds.**

## Bold assumptions (per claude-code-bot#11 — flagged, not blocking)

| # | Assumption | Why |
|---|---|---|
| A1 | **Two habit types cover v1: `boolean` and `numeric`.** Boolean = did/didn't (Ran, Meditated). Numeric = a value with an optional unit and optional daily target (Sleep `h`, Water `glasses`, Drinks). | This is the smallest type system that covers every example James gave (checkbox habits + sleep hours + drink count). More types (time-of-day, duration, rating) can be added later without reshaping the model. |
| A2 | **One entry per habit per day**, keyed by calendar date (date-only, no time). Logging is an idempotent upsert — re-tapping toggles/overwrites. Back-filling past days is allowed. | Habits are a daily cadence; a date-keyed upsert keeps the model and the UI trivially simple and lets him fix a missed day. |
| A3 | **Completion rule.** Boolean: complete when ticked. Numeric with a target: complete when `value >= target`. Numeric without a target: complete when any value is entered. | Gives streaks a clear definition for both types while letting "just log the number" habits work without forcing a target. |
| A4 | **Streak = consecutive days complete, counting back from today** (today not yet logged doesn't break it until the day ends). Also surface a simple "last 30 days" completion grid per habit. | Streaks are the one bit of gamification James's notes ask for; the grid gives at-a-glance history with zero charting infra. |
| A5 | **Habits are user-configured**, seeded with a small sensible starter set (e.g. Sleep, Exercise, Read, Water) James can rename/add/archive. Archive, don't hard-delete, so history survives. | Keeps first-run non-empty and useful; archiving preserves streak history if he drops a habit. |
| A6 | **Single user, but keep the template's auth as-is.** No per-user columns beyond what the template already gives. | Zero extra work; multi-user comes free if it ever goes public. |
| A7 | **Local calendar day** is whatever date the client sends (`YYYY-MM-DD`); server stores it verbatim. | Avoids timezone math in v1; the client already knows "today" for the user. |

## Explicitly deferred (roadmap, not v1)

These are real ideas from James's vault/issue — parked to keep v1 shippable, not
dropped:

- **Sleep wheel** (Apple-rings-style dial input) — the one component with real
  design work; v1 uses a plain number field.
- **Auto-tick rules** ("Ran ⇒ Exercised") — a small rules layer, easy to add
  once the daily loop exists.
- Notes / photos per day, calendar view, "word of the month", goals &
  progress bars, reminders/push notifications, weekly review.
- Charts beyond the 30-day grid; export.

## MVP scope (v1, deployable)

1. **Habit CRUD**: create/rename/reorder/archive habits (name, type, unit?,
   target?).
2. **Daily loop**: `GET /api/days/{date}` → the day's habits each with its
   entry (or none) and current streak; `PUT /api/entries` → upsert one entry.
3. **Today screen**: the day's habits in order; checkbox habits tick inline,
   numeric habits have a number field; streak badge per habit; date stepper to
   move to previous days. Instant optimistic save.
4. **Manage screen**: add/edit/archive habits.
5. **History**: per-habit 30-day completion grid + current/longest streak.

That's the whole of v1 — a daily checklist with numbers and streaks. Nothing
more until it's in James's hands.

## Architecture

Follows web-template exactly (backend .NET layered projects + React/TS
frontend with RTK-Query generated client, Helm chart, oke-fleet ArgoCD
deploy). App-specific pieces:

**Entities**
- `Habit` (id, name, `HabitType` {Boolean, Numeric}, unit?, target?,
  sortOrder, isArchived, createdAt)
- `HabitEntry` (id, habitId, date [date-only], value [double — `1`/`0` for
  boolean, the number for numeric], updatedAt) — unique on `(habitId, date)`.

**Domain / services**
- `HabitService` — CRUD + ordering/archival.
- `DayService` — assembles a day view (habits + entries + streaks) and upserts
  entries. Completion + streak logic lives here (per A3/A4), unit-tested with a
  golden set of multi-day fixtures, mirroring language-vocab's ScoreEngine
  test style.

**API (minimal-API route groups, like the scaffold's `/status`)**
- `GET /api/habits`, `POST /api/habits`, `PUT /api/habits/{id}`,
  `DELETE /api/habits/{id}` (archive)
- `GET /api/days/{date}` — daily loop payload
- `PUT /api/entries` — upsert `{ habitId, date, value }`
- `GET /api/habits/{id}/history?days=30`

**Frontend**
- `Today` page (default route): date stepper + habit rows + streak badges.
- `Manage` page: habit list editor.
- `History` page (or per-habit expand): 30-day grid + streaks.
- RTK-Query hooks off the generated OpenAPI client.

## Delivery plan (small PRs into `dev`, like language-vocab)

- **PR 1 (this):** scaffold from web-template + this design doc.
- **PR 2:** domain core — `Habit`/`HabitEntry` entities, DbContext + EF
  migration, `HabitService`/`DayService` with completion/streak logic, seed
  loader for the starter habit set, unit tests (golden multi-day fixtures).
- **PR 3:** WebApi routes + OpenAPI, then the frontend Today loop (Manage &
  History follow if PR 3 gets large).

Assumptions above are deliberately made rather than asked (per #11); anything
James disagrees with, change here and the code follows.

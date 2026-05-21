# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Skills — Always Use Before Writing Code Manually

Global skills live in `~/.claude/skills/`. The shared patterns file `~/.claude/skills/_patterns.md` is the single source of truth for paths, templates, and rules.

| Task | Skill |
|---|---|
| Analyze WCF file | `/wcf-analyze [file]` — נתיב אופציונלי, נשמר ב-memory |
| Estimate WCF method effort | `/wcf-estimate <method> [path] [class]` |
| Migrate WCF method | `/wcf-migrate <method> [path] [class]` — נתיב אופציונלי, נשמר ב-memory |
| Generate spec document | `/wcf-spec [service]` — מופעל אוטומטית בסוף `/wcf-migrate` |
| Add DTO | `/net10-dto <class>` |
| Add controller endpoint | `/net10-controller <method> [External\|Internal\|Incoming]` |
| Add BL method | `/net10-bl <method>` |
| Add DAL method | `/net10-dal <method>` |
| Add SP / DbContext | `/net10-yaml` then `/net10-dbcontext` |
| Add Postman request | `/net10-postman <method> <type>` |
| Add Proxy | `/net10-proxy <source>` |
| Add EventUtil | `/net10-eventutil <file>` |
| Add Planar Job | `/net10-planar <file>` |
| Add Validation | `/net10-validation <dto>` |

## Architecture

This is a CustomsCloud .NET 10 microservice following the standard CRM layered architecture:

```
API/CustomsCloud.CRM.{S}.WebApi/                    ← Controllers (HTTP entry points)
API/CustomsCloud.CRM.{S}.BL/                        ← Business logic
API/CustomsCloud.CRM.{S}.BL/Proxies/               ← Proxy adapters (BaseMicroServiceProxyAdapter)
API/CustomsCloud.CRM.{S}.BL/Validations/           ← FluentValidation validators
API/CustomsCloud.CRM.{S}.DAL/                        ← Data access (EF Core + Dapper)
API/CustomsCloud.CRM.{S}.Model/                      ← DTOs, DbContext, YAML schema
Planar/                                              ← Scheduled jobs (BaseJob)
Postman/                                             ← Postman collection
```

`{S}` = service name — detect from `.sln` / project folder names at the start of each session.

## Build

```bash
dotnet build
```

After build failure: fix obvious issues (missing usings, missing interface methods) and rebuild once. If still failing — show errors and ask.

## Key Conventions (from `_patterns.md`)

- **HTTP verb:** method prefix `Get/Find/Search/Is/Check/Load` → `[HttpGet]` + `[FromQuery]`; everything else → `[HttpPost]` + `[FromBody]`
- **`void` return** → `Task<bool>`, return `Ok(true)` or `Created(true)`
- **No `Async` suffix** on method names — applies to BL, DAL, and Controller uniformly
- **Style:** always `var result = await ...; return result;` — never `=> await ...`
- **`if` blocks** always use braces `{ }` — even single-line bodies
- **DAL reads** → `ReadOnlyContext` only; **DAL writes** → `Context` with Fetch & Merge
- **BL** always delegates via `DataLayer.` (not `DAL.` directly)
- **CA1707:** no underscores in type/member names — `PR_FRM1020_Foo` → `Frm1020Foo`
- **Soft-delete filter:** `.Where(e => e.State != 99)` + `.ExcludeInterceptor("T7e0Y38X2y")`
- **`IParametersUtil` / `ILookupUtil`** — constructor injection only, never `Resolve<>()`

## WCF Pattern Quick-Reference

| WCF pattern | .NET 10 replacement |
|---|---|
| `EventUtil.RaiseEvent(...)` | `IEventUtil` builder — run `/net10-eventutil` |
| `PushUtil.*` | `#error MIGRATION: Install NuGet CustomsCloud.Infrastructure.Notifications` |
| `SystemTablesUtil.GetCodeById<T>(id)` | `await lookupUtil.Get<T>(id)` via `ILookupUtil` |
| `Configuration.GetConfig<T>("key")` | `await parametersUtil.Get<T>("key")` via `IParametersUtil` |
| `Container.Resolve<T>()` | `Resolve<T>()` |
| `new XxxAdapter` / `BaseServiceAdapter` | Run `/net10-proxy` |
| `BaseScheduleTask` | Run `/net10-planar` |

## Git Rules

- **Commit to the feature branch only** — never directly to `master`/`main`
- The developer reviews and merges via PR

## Controller Types

Controllers are added on demand via `/net10-controller`. Three types exist:
- `External` — exposed to external clients / internet
- `Internal` — inter-service communication
- `Incoming` — inbound form/data submissions from external systems

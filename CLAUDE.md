# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Skills — Always Use Before Writing Code Manually

Global skills live in `~/.claude/skills/`. The shared patterns file `~/.claude/skills/_patterns.md` is the single source of truth for paths, templates, and rules.

| Task | Skill |
|---|---|
| Analyze WCF file | `/wcf-analyze <file>` |
| Migrate WCF method | `/wcf-migrate <method> <path> <class>` |
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
API/CustomsCloud.CRM.{S}.WebApi/        ← Controllers (HTTP entry points)
API/CustomsCloud.CRM.{S}.BL/           ← Business logic, Proxies, Validations
API/CustomsCloud.CRM.{S}.DAL/          ← Data access (EF Core + Dapper)
API/CustomsCloud.CRM.{S}.Model/        ← DTOs, DbContext, YAML schema
Postman/                                ← Postman collection
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
- **DAL reads** → `ReadOnlyContext` only; **DAL writes** → `Context` with Fetch & Merge
- **BL** always delegates via `DataLayer.` (not `DAL.` directly)
- **CA1707:** no underscores in type/member names — `PR_FRM1020_Foo` → `Frm1020Foo`
- **Soft-delete filter:** `.Where(e => e.State != 99)` + `.ExcludeInterceptor("T7e0Y38X2y")`

## Controller Types

Controllers are added on demand via `/net10-controller`. Three types exist:
- `External` — exposed to external clients / internet
- `Internal` — inter-service communication
- `Incoming` — inbound form/data submissions from external systems

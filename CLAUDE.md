# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture

This library provides a single extension method `AddNHibernate()` for `IServiceCollection` that wires up NHibernate for ASP.NET Core applications.

### Key Components

- **NHibernateServiceCollectionExtensions** - Entry point. Registers all NHibernate services with appropriate lifetimes:
  - `Configuration` and `ISessionFactory` as singletons
  - `ISession` and `IStatelessSession` as scoped (per-request)

- **SessionManager** (internal) - Manages session lifecycle within a DI scope. Lazily creates sessions on first access and disposes them at scope end.

- **Logging/** - Bridges NHibernate logging to `Microsoft.Extensions.Logging` by implementing `INHibernateLoggerFactory` and `INHibernateLogger`.

## Publishing

Releases are published to NuGet via GitHub Actions when a release is created with a `v*` tag.

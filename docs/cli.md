# CLI Reference (`dotnet email-preview`)

EmailStudio provides a command-line tool for scaffolding, previewing, and headless rendering.

## Commands

### `dotnet email-preview init`
Scaffolds a new template workspace containing standard layouts, starter templates, sample data, and locale files.

```bash
dotnet email-preview init [path]
```

### `dotnet email-preview serve`
Starts the local hot-reloading development preview server.

```bash
dotnet email-preview serve [path] [--port 5173] [--smtp host:port]
```

### `dotnet email-preview render`
Performs headless batch compilation and rendering of templates, enforcing the 102 KB Gmail size guard (ideal for CI pipelines).

```bash
dotnet email-preview render [path] [--culture <code-code>] [--out <dist-directory>]
```


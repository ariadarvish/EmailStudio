# EmailStudio Diagnostics Reference

This document catalogs all diagnostic error and warning codes emitted by EmailStudio during template validation and compilation.

| Code | Severity | Description | Fix |
| --- | --- | --- | --- |
| **TPL001** | Error | Template file cannot be parsed | Check Razor / HTML syntax in the `.cshtml` file. |
| **TPL002** | Error | Unknown property on declared model type | Verify property names match the `@model` class. |
| **TPL003** | Error | Referenced layout or partial not found | Ensure layout or partial exists in the specified relative path. |
| **TPL004** | Warning | Missing or invalid sample JSON | Verify `<Template>.sample.json` exists and contains valid JSON. |
| **TPL005** | Error | Missing `subject` in front matter | Add `subject: Your Subject Line` in template YAML front matter. |
| **TPL006** | Error | Duplicate template name | Rename conflicting template files to ensure unique identifiers. |
| **TPL007** | Warning | Rendered HTML exceeds size threshold | Keep rendered email size below 102 KB to avoid Gmail message clipping. |
| **TPL008** | Error | Unsupported syntax element | Remove unsupported Razor directives (e.g. `@functions`, `@await`). |
| **TPL009** | Warning | Missing locale key (strict mode) | Add missing translation key in `Locales/<culture>.json`. |


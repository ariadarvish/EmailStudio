# Internationalization (i18n)

EmailStudio provides localized email rendering through structured JSON locale dictionaries.

## Locale File Structure

Place locale definitions in `Locales/<culture>.json`:

```json
// Locales/de-DE.json
{
  "footer.disclaimer": "Bitte antworten Sie nicht direkt auf diese E-Mail.",
  "actions.confirm": "E-Mail bestätigen"
}
```

## Template Usage

Reference keys within templates:

```html
<p><T.Try("footer.disclaimer") /></p>
```

## Fallback Chain

When resolving keys:
1. Target culture (e.g. `de-DE`)
2. Base language (e.g. `de`)
3. Default template fallback / base text
4. Under strict mode, missing keys raise `TPL009`.


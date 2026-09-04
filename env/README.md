# Environment configuration

This directory contains non-secret environment templates for hosts that
execute the MAF declarative workflow stored under `.pmcro/workflows/`.

The template is intentionally limited to names of values that the executing
host may provide. Do not place credentials, tokens, connection strings, or
private endpoints in the repository.

For local .NET development, prefer user-secrets or environment variables.
For production, use the host platform's secret/configuration provider.

The PMCR-O repository itself does not load `.env` files; this directory is a
configuration handoff for the executing runtime.

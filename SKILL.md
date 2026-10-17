---
name: recover-account-access
description: "Diagnose and guide legitimate GitHub account recovery without bypassing authentication or exposing credentials."
---
# recover-account-access

Diagnose and guide recovery of a legitimate GitHub account when browser sign-in, password, two-factor authentication, or GitHub CLI access is failing. This capability is recovery-only and must never bypass GitHub authentication or expose credentials.

## When to Use

- The account owner reports being locked out of GitHub.com.
- Browser sign-in fails but GitHub CLI or Git operations may still work.
- Password works but a configured second factor is unavailable.
- The owner needs a safe inventory of remaining recovery options before changing anything.

## When Not to Use

- Do not use to access another person's account.
- Do not bypass 2FA, CAPTCHA, passkeys, security keys, device verification, organization controls, or GitHub support policy.
- Do not read, print, copy, upload, or persist passwords, personal access tokens, SSH private keys, TOTP seeds, passkey material, recovery-code contents, session cookies, or browser credential stores.
- Do not run `gh auth token`, `gh auth logout`, revoke tokens, disable 2FA, unlink email, or delete credentials as part of diagnosis.
- Do not treat an authenticated CLI session as proof that browser recovery is complete.

## Inputs

- `mode`: `preflight`, `inspect`, or `check`.
- `repo`: optional repository path whose remotes should be inspected without mutation.
- `symptom`: optional human-readable lockout symptom used only for the recovery plan.

## Workflow

1. Preserve remaining access. Do not log out, revoke, rotate, or overwrite credentials during diagnosis.
2. Run `scripts/github_account_recovery.ps1 -Mode preflight` to confirm required tools and safe local state.
3. Run `scripts/github_account_recovery.ps1 -Mode inspect` to identify whether GitHub CLI/API authentication still works and whether the issue appears browser-only, 2FA-related, or broader.
4. Read `references/recovery-contract.md` and select only GitHub-supported recovery methods.
5. Present official recovery URLs and the minimum next owner action.
6. Stop before any destructive or authority-changing action. Account changes remain human-controlled.
7. Return non-secret evidence to the Trail for independent checking.

## Validation

- Output contains no credential material.
- GitHub CLI/API checks are read-only.
- Recovery links point only to `github.com` or `docs.github.com`.
- No operation disables or weakens 2FA.
- No operation mutates repository, marketplace, registry, grant, activation, or GitHub account state.
- Creation and discovery do not imply registration, grant, activation, publication, installation, or invocation.

## Progressive Disclosure

- `references/recovery-contract.md` defines supported recovery branches and security boundaries.
- `scripts/github_account_recovery.ps1` performs read-only environment inspection.
- `assets/recovery-request.example.json` provides a non-secret example input.

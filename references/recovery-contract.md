# GitHub account recovery contract

This capability supports legitimate account recovery only. It does not bypass GitHub authentication and does not grant access by itself.

## Recovery branches

1. Browser sign-in fails, but GitHub CLI/API still works.
   - Preserve the working CLI session.
   - Do not run `gh auth logout`, `gh auth token`, credential deletion, token rotation, or revocation during diagnosis.
   - Use the working session only as evidence that one authentication surface remains available; it does not prove browser access is restored.

2. Password works, but two-factor authentication fails.
   - Try configured supported factors such as TOTP, passkey/security key, GitHub Mobile, SMS if previously configured, or a valid single-use recovery code.
   - Recovery-code contents are secrets and must never be read or persisted by this capability.

3. Two-factor credentials are lost.
   - Use GitHub's official account recovery flow and configured recovery methods.
   - GitHub Support does not restore a 2FA-protected account when all recovery methods are unavailable.

4. No recovery method remains.
   - Stop. Do not attempt credential bypass.
   - The owner may use GitHub's documented locked-account email-unlink flow only after confirming recovery is impossible. Unlinking email does not restore the locked account.

## Official references

- https://docs.github.com/en/authentication/securing-your-account-with-two-factor-authentication-2fa
- https://docs.github.com/en/authentication/securing-your-account-with-two-factor-authentication-2fa/troubleshooting-two-factor-authentication-issues
- https://docs.github.com/en/site-policy/other-site-policies/github-account-recovery-policy
- https://docs.github.com/en/account-and-profile/how-tos/account-management/unlinking-your-email-address-from-a-locked-account
- https://github.com/login

## Security boundary

Never inspect or export:

- passwords
- personal access tokens or OAuth tokens
- SSH private keys
- TOTP seeds
- passkey/private key material
- recovery-code contents
- browser cookies or saved credentials

The script may report only whether a local GitHub CLI session appears authenticated, the non-secret account login returned by the GitHub API, CLI version, credential-helper configuration, repository remotes, and official recovery URLs.

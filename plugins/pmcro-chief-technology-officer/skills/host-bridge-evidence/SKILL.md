---
name: host-bridge-evidence
description: Query a linked local machine's live device state and read its real files via a remote MCP proxy (get_device_info / device_list_dir / device_request_folder_access, plus whatever local MCP server -- e.g. Desktop Commander -- the desktop app proxies through), for use as Make/Check evidence. USE FOR -- confirming a host-side file, folder, or local MCP server's actual state before writing it into a MakeStep or CheckFrame, and for recovering cleanly when the bridge or a proxied local server drops mid-session. DO NOT USE FOR -- writing or deleting host files outside an explicit, user-approved PlanFrame step, or as a substitute for the host's own build/test tooling.
metadata:
  version: "0.1.0"
  tier: EVIDENCE
  capability_class: SUPPORT
---

# Host Bridge Evidence

## Command

No PMCR-O CLI command of its own -- this skill is a recipe for calling
the remote-device bridge's own tools, not a new deterministic script. See
`assets/tools.host-bridge-evidence.asset.md` for the call catalog.

## Purpose

Wraps a Claude session's remote-device bridge -- the mechanism that lets
a cloud-hosted session read a linked local machine's real files and query
its state -- so a Maker or Checker step can attach real, live evidence
about the host (its files, its connected folders, the state of any local
MCP server it proxies) instead of relying on what a prior session summary
or a stale registry claims.

## Setup

Nothing to install -- the bridge is a capability of the session itself,
not a package this repo adds. `get_device_info` works as soon as a
session is linked to a device. Reading a specific folder's real files
additionally requires `device_request_folder_access` once per folder per
session -- this throws a real consent prompt on the user's device, so it
cannot be done silently or in advance. See
`assets/setup.host-bridge-evidence.asset.md` for the exact sequence and
what each tool's absence versus failure actually means.

## Inputs / Outputs / Boundaries

See `assets/tools.host-bridge-evidence.asset.md` for the call catalog and
`assets/recipe.host-bridge-evidence.asset.md` for how Maker/Checker fold
results into `Action`/`Result` or `criteria[].evidence`.

## References

- `assets/setup.host-bridge-evidence.asset.md`
- `assets/tools.host-bridge-evidence.asset.md`
- `assets/recipe.host-bridge-evidence.asset.md`
- `references/README.md`

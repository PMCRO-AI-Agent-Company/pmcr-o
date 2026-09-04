# Recipe -- folding Aspire evidence into a cycle

## For Maker (before writing a MakeStep)

If a step claims a resource reached a running/healthy state, or that a
request actually went through, call `list_resources` (state) and/or
`list_console_logs` / `list_traces` (behavior) first, and quote the
relevant field(s) directly in the MakeStep's `Action`/`Result` text --
not just "it worked", but what the tool actually returned.

## For Checker (before writing a CheckFrame)

Checker's whole job is not trusting Maker's self-report. Where a
`success_criteria` entry is about runtime state or behavior (not just "the
code compiles"), call the matching Aspire tool independently and put its
own output in `criteria[].evidence` -- e.g. a criterion like
"OrchestrationApi exposes GET /api/chat backed by a gRPC gateway to
Runtime" is a code-review claim until `list_traces` (or a real HTTP call)
actually shows a cross-service call completing.

## What this does not replace

A PASS here reflects Aspire's own view of its resources, not independent
proof. Prefer an actual `dotnet build`, a real HTTP/gRPC round-trip, or a
test run wherever one is possible; use this skill's tools to add runtime
confidence on top of that, not instead of it.

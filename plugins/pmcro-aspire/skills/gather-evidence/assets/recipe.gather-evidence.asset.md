# Recipe -- folding Aspire evidence into a cycle

## For Maker (before writing a MakeStep)

If a step claims a resource reached a running/healthy state, or that a
request actually went through, run `aspire describe --format Json` (state)
and/or `aspire logs <resource>` / `aspire otel traces` (behavior) first,
and quote the relevant field(s) directly in the MakeStep's
`Action`/`Result` text -- not just "it worked", but what the command
actually returned. Use `aspire start` / `aspire wait` / `aspire stop`
for lifecycle -- never `dotnet run` plus manual process killing (see the
setup asset for why).

## For Checker (before writing a CheckFrame)

Checker's whole job is not trusting Maker's self-report. Where a
`success_criteria` entry is about runtime state or behavior (not just "the
code compiles"), run the matching Aspire CLI command independently and put
its own output in `criteria[].evidence` -- e.g. a criterion like
"OrchestrationApi exposes GET /api/chat backed by a gRPC gateway to
Runtime" is a code-review claim until `aspire otel traces` (or a real HTTP
call) actually shows a cross-service call completing.

## What this does not replace

A PASS here reflects the Aspire CLI's own view of its resources, not
independent proof. Prefer an actual `dotnet build`, a real HTTP/gRPC
round-trip, or a test run wherever one is possible; use these commands to
add runtime confidence on top of that, not instead of it. Trail
`d360b692-5014-4267-9018-9b94758e9170` did both: a real curl-equivalent
`/api/chat` call AND `aspire describe` confirming every resource's state.

# Workflows

This repository carries the canonical Microsoft Agent Framework (MAF)
declarative workflow contract under `declarative/`.

## Runtime boundary

`declarative/pmcro-cycle.yaml` is configuration for an executing .NET MAF
host. It is not a second workflow engine and does not replace the PMCR-O
control-plane scripts.

The executing host is responsible for loading the YAML with the MAF
Declarative Workflow APIs and registering the six PMCR-O role agents.

The workflow uses MAF's native `InvokeMcpTool` action for MCP capability
access. The executing host supplies the MAF MCP tool handler; this repository
does not implement an MCP client manager, JSON-RPC transport, or custom tool
registry.

## Package contract

The executing .NET host should reference:

- `Microsoft.Agents.AI`
- `Microsoft.Agents.AI.Workflows`
- `Microsoft.Agents.AI.Workflows.Declarative`
- `Microsoft.Agents.AI.Workflows.Declarative.Mcp`

Microsoft documents `InvokeMcpTool` as the native declarative MCP action and
requires an MCP tool handler in the C# workflow factory.

See `declarative/README.md` for the governance mapping and validation boundary.

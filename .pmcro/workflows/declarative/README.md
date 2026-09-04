# MAF Declarative Workflow

`pmcro-cycle.yaml` is the canonical declarative orchestration contract for
this repository's six PMCR-O roles.

## Runtime boundary

The YAML is executed by Microsoft Agent Framework. The host runtime must
register the six role agents with the MAF agent provider. This repository does
not implement a second workflow engine.

MAF's C# declarative workflow surface uses `kind: Workflow`, an
`OnConversationStart` trigger, and ordered actions. MCP calls use
`InvokeMcpTool`; the runtime supplies the `McpToolHandler`.

Required packages in the executing .NET host are:

- `Microsoft.Agents.AI`
- `Microsoft.Agents.AI.Workflows`
- `Microsoft.Agents.AI.Workflows.Declarative`
- `Microsoft.Agents.AI.Workflows.Declarative.Mcp`

The MCP package is the integration boundary. Do not add a custom MCP client,
JSON-RPC transport, tool registry, or MCP client manager here.

## Governance mapping

- Orchestrator is first in the graph and remains the sole routing authority.
- Planner produces the bounded PlanFrame.
- Maker executes only the certified plan.
- Checker remains the acceptance gate.
- Reflector owns disposition, sealing, and next-seed behavior.
- MCP is capability/evidence access, not orchestration.
- The example Microsoft Learn MCP call is read-oriented but is marked
  `requireApproval: true` because this repository treats external network
  access as a governed TYPE1 boundary.

## Validation

The YAML should be loaded by `DeclarativeWorkflowBuilder.Build` in the
executing MAF host. Static repository validation checks its required action
kinds and the package contract; it does not pretend to execute an external
MAF runtime that is not part of this repository.

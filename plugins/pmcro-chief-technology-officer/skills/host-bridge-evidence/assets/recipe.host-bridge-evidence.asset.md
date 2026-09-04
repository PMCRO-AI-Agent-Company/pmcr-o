# Recipe -- folding host-bridge evidence into a cycle

## For Maker (before writing a MakeStep)

If a step claims a host-side file, folder, or local MCP server is in a
particular state, call the bridge first and quote what actually came
back -- the real `localMcpServers` state string, the real file content
read, the real listing -- not a paraphrase and never a placeholder for a
call that has not actually been made yet. If a needed folder is not yet
granted, request access as its own explicit step and record whether it
was granted, not assumed.

## For Checker (before writing a CheckFrame)

Checker's job is not trusting Maker's self-report. Re-call the bridge
independently -- `get_device_info` again, `read_file` on the specific
path Maker cited -- rather than accept Maker's quoted text as given.
Where a criterion is about a host-side mutation (a file written, a
directory created), independently list or read the result; a MakeStep
that only says "wrote the file" is a claim, not evidence, until Checker's
own call confirms it.

## What this does not replace

A PASS here reflects the bridge's own live view of the host at the
moment of the call, not a durable guarantee -- the host can change
between the Maker's call and the Checker's. Prefer re-querying close to
the point of verification rather than reusing an earlier call's result,
and never assume a link that was up a few turns ago is still up now.

"""Deterministic validator for the PMCR-O governed output envelope."""
import json
import sys
from pathlib import Path

REQUIRED = [
    "frame_id", "trail_id", "workflow_id", "action",
    "state_transition", "required_evidence", "next_gate"
]

def validate(result):
    errors = []
    if not isinstance(result, dict):
        return ["result must be an object"]
    for key in REQUIRED:
        if key not in result:
            errors.append(f"missing required field: {key}")
    for key in ("frame_id", "trail_id", "action", "state_transition", "next_gate"):
        if key in result and (not isinstance(result[key], str) or not result[key]):
            errors.append(f"{key} must be a non-empty string")
    if "required_evidence" in result and not isinstance(result["required_evidence"], list):
        errors.append("required_evidence must be an array")
    if result.get("action") in {"COMPLETE", "SEAL"}:
        if not result.get("evidence"):
            errors.append("completion requires evidence")
        checker = result.get("checker")
        if not isinstance(checker, dict) or checker.get("status") != "PASS":
            errors.append("completion requires checker.status=PASS")
    return errors

def main():
    if len(sys.argv) != 2:
        print("usage: validate_output_contract.py <result.json>")
        return 2
    try:
        result = json.loads(Path(sys.argv[1]).read_text(encoding="utf-8-sig"))
    except (OSError, json.JSONDecodeError) as exc:
        print(f"CHECKER: FAIL\n- invalid JSON input: {exc}")
        return 1
    errors = validate(result)
    if errors:
        print("CHECKER: FAIL")
        for error in errors:
            print(f"- {error}")
        return 1
    print("CHECKER: PASS — governed output conforms to L-OUTPUT-CONTRACT")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())

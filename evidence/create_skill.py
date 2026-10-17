#!/usr/bin/env python3
"""Cross-platform bootstrap author for candidate Agent Skills.

Creation only: this tool never registers, grants, activates, publishes, installs,
or invokes a generated skill.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import sys
from pathlib import Path, PureWindowsPath

NAME_RE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")


def fail(message: str) -> None:
    raise SystemExit(message)


def repo_root(explicit: str | None) -> Path:
    if explicit:
        return Path(explicit).expanduser().resolve()
    return Path(__file__).resolve().parents[4]


def valid_name(label: str, value: str) -> None:
    if not (1 <= len(value) <= 64) or not NAME_RE.fullmatch(value):
        fail(f"{label} must be 1-64 chars: lowercase letters, digits, single hyphens")


def read_request(path: Path) -> dict:
    data = json.loads(path.read_text(encoding="utf-8-sig"))
    for key in ("plugin", "skill", "agent", "description"):
        if not isinstance(data.get(key), str) or not data[key].strip():
            fail(f"request missing non-empty string: {key}")
    valid_name("plugin", data["plugin"])
    valid_name("skill", data["skill"])
    valid_name("agent", data["agent"])
    if len(data["description"]) > 1024:
        fail("description exceeds 1024 characters")
    return data


def safe_relative(name: str) -> Path:
    p = Path(name)
    if p.is_absolute() or PureWindowsPath(name).drive or "\\" in name or ":" in name or ".." in p.parts:
        fail(f"unsafe generated path: {name}")
    return p


def default_skill_body(data: dict) -> str:
    purpose = data.get("purpose") or data["description"]
    return f"""# {data['skill']}\n\n{purpose}\n\n## When to Use\n\n- Use when the accepted plan routes to this capability.\n\n## When Not to Use\n\n- Do not use merely because the package is discoverable.\n- Do not infer registration, grant, activation, publication, or execution authority.\n\n## Workflow\n\n1. Validate inputs and environment.\n2. Simulate the requested change before mutation.\n3. Execute only under an explicit grant.\n4. Return observable evidence for independent checking.\n\n## Validation\n\n- Report created or changed paths.\n- Report failures truthfully.\n- Leave lifecycle promotion to a separate independent check.\n"""


def render_skill(data: dict) -> str:
    body = data.get("skillBody") or default_skill_body(data)
    if not body.endswith("\n"):
        body += "\n"
    desc = json.dumps(data["description"].replace("\n", " ").strip(), ensure_ascii=False)
    return f"---\nname: {data['skill']}\ndescription: {desc}\n---\n{body}"


def render_agent(data: dict) -> str:
    body = data.get("agentBody") or (
        f"# {data['agent']}\n\n"
        f"Use the `{data['skill']}` skill only when selected by the governing plan.\n\n"
        "Do not register, grant, activate, publish, install, or self-approve this candidate.\n"
    )
    desc = json.dumps(data["description"].replace("\n", " ").strip(), ensure_ascii=False)
    return (
        "---\n"
        f"name: {data['agent']}\n"
        f"description: {desc}\n"
        f"skills: [{data['skill']}]\n"
        "---\n"
        f"{body.rstrip()}\n"
    )


def candidate_files(data: dict) -> dict[Path, str]:
    files: dict[Path, str] = {
        Path("SKILL.md"): render_skill(data),
        Path("agents") / f"{data['agent']}.md": render_agent(data),
    }
    extra = data.get("files") or {}
    if not isinstance(extra, dict):
        fail("files must be an object mapping relative path to text content")
    for raw_path, content in extra.items():
        if not isinstance(content, str):
            fail(f"generated file content must be text: {raw_path}")
        rel = safe_relative(raw_path)
        if rel == Path("assets/creation-request.json") or rel in files or rel == Path("SKILL.md") or (rel.parts and rel.parts[0] == "agents"):
            fail(f"reserved generated path: {raw_path}")
        files[rel] = content if content.endswith("\n") else content + "\n"
    return files


def manifest(files: dict[Path, str]) -> dict:
    return {
        str(path).replace("\\", "/"): hashlib.sha256(text.encode("utf-8")).hexdigest()
        for path, text in sorted(files.items(), key=lambda x: str(x[0]))
    }


def check_candidate(dest: Path, data: dict) -> dict:
    skill_md = dest / "SKILL.md"
    agents_dir = dest / "agents"
    errors: list[str] = []
    if not skill_md.is_file():
        errors.append("missing SKILL.md")
    else:
        text = skill_md.read_text(encoding="utf-8")
        if f"name: {data['skill']}" not in text:
            errors.append("SKILL.md name does not match directory")
        if len(text.splitlines()) > 500:
            errors.append("SKILL.md exceeds 500 lines")
    if not agents_dir.is_dir() or not list(agents_dir.glob("*.md")):
        errors.append("missing required agents/*.md declaration")
    request_file = dest / "assets" / "creation-request.json"
    if request_file.is_file():
        request = json.loads(request_file.read_text(encoding="utf-8"))
        if request.get("registered") is not False:
            errors.append("creation-request.json must record registered=false")
    return {"ok": not errors, "errors": errors, "path": str(dest)}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--request", required=True)
    parser.add_argument("--repo")
    parser.add_argument("--mode", choices=("simulate", "execute", "check"), default="simulate")
    args = parser.parse_args()

    root = repo_root(args.repo)
    request_path = Path(args.request).expanduser().resolve()
    data = read_request(request_path)
    request_dir = request_path.parent
    if data.get("skillBodyFile"):
        data["skillBody"] = (request_dir / safe_relative(data["skillBodyFile"])).read_text(encoding="utf-8")
    if data.get("agentBodyFile"):
        data["agentBody"] = (request_dir / safe_relative(data["agentBodyFile"])).read_text(encoding="utf-8")
    source_files = data.get("fileSources") or {}
    if not isinstance(source_files, dict):
        fail("fileSources must be an object mapping generated path to request-relative source path")
    merged_files = dict(data.get("files") or {})
    for generated_path, source_path in source_files.items():
        source = request_dir / safe_relative(source_path)
        if not source.is_file():
            fail(f"missing source file: {source}")
        merged_files[generated_path] = source.read_text(encoding="utf-8")
    data["files"] = merged_files
    dest = root / "plugins" / data["plugin"] / "skills" / data["skill"]
    files = candidate_files(data)

    result = {
        "apiVersion": "pmcro.skill-create/v1",
        "kind": "SkillCandidate",
        "mode": args.mode,
        "plugin": data["plugin"],
        "skill": data["skill"],
        "agent": data["agent"],
        "destination": str(dest),
        "registered": False,
        "granted": False,
        "activated": False,
        "published": False,
        "invoked": False,
        "files": list(manifest(files).keys()),
    }

    if args.mode == "simulate":
        if dest.exists():
            result["wouldFail"] = "destination already exists"
        print(json.dumps(result, indent=2))
        return 0

    if args.mode == "execute":
        if dest.exists():
            fail(f"candidate already exists: {dest}")
        for rel, text in files.items():
            path = dest / rel
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text(text, encoding="utf-8", newline="\n")
        asset_dir = dest / "assets"
        asset_dir.mkdir(parents=True, exist_ok=True)
        creation = {
            "sourceRequest": str(request_path),
            "plugin": data["plugin"],
            "skill": data["skill"],
            "agent": data["agent"],
            "registered": False,
            "granted": False,
            "activated": False,
            "published": False,
            "invoked": False,
            "contentSha256": manifest(files),
        }
        (asset_dir / "creation-request.json").write_text(
            json.dumps(creation, indent=2) + "\n", encoding="utf-8", newline="\n"
        )
        result["check"] = check_candidate(dest, data)
        print(json.dumps(result, indent=2))
        return 0 if result["check"]["ok"] else 2

    result["check"] = check_candidate(dest, data)
    print(json.dumps(result, indent=2))
    return 0 if result["check"]["ok"] else 2


if __name__ == "__main__":
    sys.exit(main())

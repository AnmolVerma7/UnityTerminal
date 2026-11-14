# Agent instructions (UnityTerminal)

All automated agents (Cursor, CI assistants, etc.) working in this repository **must read and follow this file** before making changes, commits, or pull requests.

## Project context

- Unity **6000.x** terminal UI mini-project under `Assets/Project/`.
- Core code: `TerminalController`, `CommandRegistry`, commands in `Assets/Project/Scripts/Terminal/`, themes/profiles as ScriptableObjects.
- Command reference: `Assets/Project/Docs/Commands.md`.
- **Do not** add or commit `Assets/Command Prompt Shell/` (local web reference; listed in `.gitignore`).

## General rules

1. **Minimize scope** — smallest correct change; match existing naming and patterns in `Assets/Project/Scripts/`.
2. **No commits unless the user asks** — stage and commit only when explicitly requested; never force-push `main` without approval.
3. **Never commit** engine caches or generated noise: `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `*.csproj`, `*.sln`, build outputs, or ad-hoc agent plan/scratch files.
4. **Unity `.meta` files** — keep them paired with assets you add or move; use valid 32-character hex GUIDs on hand-authored `.meta` files.
5. **Secrets** — do not commit API keys, tokens, or personal paths in scenes or config.

---

## Git commit conventions

Follow [Conventional Commits](https://www.conventionalcommits.org/) strictly:

```text
<type>(<scope>): <description>
```

### Allowed types

| Type | Use for |
|------|--------|
| `feat` | New gameplay mechanic, system, or player-facing feature |
| `fix` | Bug fix in code, physics, UI, or level logic |
| `art` | 2D/3D models, textures, materials, shaders, animations, VFX |
| `audio` | SFX, music, spatial audio, mixer/clip changes |
| `level` | Scene layout, lighting, terrain, navmesh, spawn points |
| `balance` | Gameplay tuning, damage, economy, physics curves |
| `ui` | HUD, menus, terminal view/input, localization |
| `perf` | Frame rate, draw calls, memory, load time |
| `refactor` | Restructure without behavior change |
| `build` | Engine/packages, build scripts, platform SDK, CI/CD |
| `chore` | Cleanup, `.meta` maintenance, docs, `.gitignore`, `AGENTS.md` |

### Scopes (optional)

Lowercase noun for the affected area, for example:

`terminal`, `commands`, `theme`, `audio`, `input`, `scene`, `profile`, `docs`, `git`

### Subject line

1. **Imperative mood** — “add”, not “added”.
2. **Lowercase** first letter of the description.
3. **No trailing period.**
4. **Target ≤ 50 characters** (72 hard cap).

Examples:

- `feat(terminal): add printmsg command`
- `fix(theme): restore baseline colors when theme cleared`
- `chore(git): ignore command prompt shell reference`

### Body (when needed)

- Explain **what** changed and **why** (especially for `balance`, `level`, or large binary assets where diffs are unreadable).
- Wrap lines at **72 characters**.
- Reference issues or tasks only if the user provided them.

### Breaking changes

- Append `!` before the colon: `feat(save)!: migrate profile schema`.
- Add a footer paragraph starting with `BREAKING CHANGE:` describing migration or impact.

### Atomic commits

- **Do not** mix binary-heavy work (`art`, `audio`, `level`) with script logic (`feat`, `fix`, `refactor`) in one commit unless the user explicitly wants a single snapshot.
- Prefer one logical change per commit.

### What agents must not commit

- Temporary agent plans, conversation exports, or scratch notes.
- `Assets/Command Prompt Shell/` or anything matched by `.gitignore` unless the user overrides.
- Local IDE-only junk (`.DS_Store`) — add to `.gitignore` instead of committing.

---

## Suggested workflow for agents

1. Read `AGENTS.md` and relevant docs under `Assets/Project/Docs/`.
2. Implement and verify (Play Mode / compile) when applicable.
3. When the user requests a commit: `git status` → stage only intended paths → commit with conventional message → push only if asked.
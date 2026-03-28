# AGENTS.md

Guidelines for AI agents working on this codebase.

## Project Overview

Phonic is a lightweight Windows desktop app that lets users switch their system audio output device from a controller-friendly UI when launched through Steam Big Picture.

The app is built with .NET (WPF) and is designed to behave like a console-native tool:
- launches fullscreen
- is fully navigable via controller using Steam Input (mapped to keyboard)
- allows quick selection of audio output devices (speakers, headphones, HDMI, etc.)
- switches the Windows default output device
- provides immediate feedback and exits quickly

Phonic is intentionally minimal:
- no background services
- no hotkeys
- no advanced settings
- no reliance on mouse input

## Code Style

- Prefer functional components
- Use named exports
- Avoid using else if possible
- Short circuit functions when possible
- Short circuit loops when possible
- Avoid nested ifs
- Entry point functions should come first, followed by helper functions used by them below

## Git

Use [Conventional Commits](https://www.conventionalcommits.org/) for all commit messages.

### Format

```
<type>: <description>
```

Keep commits to a single line (no body or footer). Ignore adding co-authors to commits.

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Formatting, missing semicolons, etc.
- `refactor`: Code change that neither fixes a bug nor adds a feature
- `perf`: Performance improvement
- `test`: Adding or updating tests
- `chore`: Maintenance tasks, dependencies, etc.

### Commit Granularity

When committing changes, break work into logical, atomic commits. Each commit should represent a single coherent unit of change — don't bundle unrelated changes into one commit.

Split commits by feature and by backend vs frontend. For a given feature, commit the backend changes (schema, tRPC router, etc.) first, then the frontend changes (components, pages, etc.) in a separate commit. If a task involves multiple features, repeat this pattern for each. Prefer committing as you go rather than squashing everything at the end.

### Examples

```
feat: add password reset flow
fix: correct date parsing for observations
refactor: simplify router structure
chore: update mantine to v8.4
```

## Testing

## Common Patterns

## Important Notes
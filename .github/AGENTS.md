# CPM Project Custom Agents

This workspace defines two custom agents for the CPM project:

- `CPM Source Code Agent`
  - Use for source code tasks under `CpmServer/**`, including backend APIs, database migrations, models, DTOs, services, and frontend `cpm-web`.
  - Designed to keep context focused and avoid exceeding model limits by narrowing file scope.

- `CPM Plan Agent`
  - Use for planning documents under `CpmPlan/**`, such as architecture notes, development standards, process guides, and workflow design.
  - Designed to focus on one document or topic at a time and summarize large files.

- `UI UX PRO MAX Agent`
  - Use for UI/UX design tasks, frontend visual guidance, layout suggestions, color palettes, typography, and experience improvements.
  - Designed to provide practical UI/UX recommendations for `cpm-web` and product design.

## Usage

In Copilot Chat, select the agent by name when working on tasks for the respective area.

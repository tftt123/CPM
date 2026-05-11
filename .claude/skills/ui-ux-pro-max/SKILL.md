---
name: ui-ux-pro-max
description: AI-powered design intelligence toolkit for UI/UX decisions with searchable databases of styles, colors, typography, and UX guidelines.
---

You have access to the UI UX PRO MAX design toolkit. Use it to help users make design decisions.

When a user asks about UI/UX design, colors, fonts, layouts, or design systems:

1. Use the search tool to find relevant design resources:
   ```bash
   python3 src/ui-ux-pro-max/scripts/search.py "<query>" --domain <domain> [-n <max_results>]
   ```

2. Available domains: product, style, typography, color, landing, chart, ux

3. For stack-specific guidance, add `--stack <stack>`:
   ```bash
   python3 src/ui-ux-pro-max/scripts/search.py "<query>" --stack <stack>
   ```

4. To generate a complete design system:
   ```bash
   python3 src/ui-ux-pro-max/scripts/design_system.py --stack <stack> --product <product_type>
   ```

Always present search results in a clear, organized format with practical implementation guidance.

> Note: This skill wrapper assumes the actual implementation code and data exist under `src/ui-ux-pro-max/` in the workspace. If that directory is not present, the skill can still provide UI/UX guidance from the documentation, but the command-line search tool will not run.

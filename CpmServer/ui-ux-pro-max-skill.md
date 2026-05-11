# UI UX PRO MAX - 设计智能工具包

## 简介

UI UX PRO MAX 是一个 AI 驱动的设计参考工具包，为开发者提供可搜索的 UI 样式、配色方案、字体搭配、图表类型和 UX 指南数据库。

**项目地址:** https://github.com/nextlevelbuilder/ui-ux-pro-max-skill

---

## 核心功能

| 搜索领域 | 说明 |
|---------|------|
| `product` | 产品类型推荐（SaaS、电商、作品集等） |
| `style` | UI 风格（玻璃态、极简主义、粗野主义等）+ AI 提示词和 CSS 关键词 |
| `typography` | 字体搭配（含 Google Fonts 导入代码） |
| `color` | 按产品类型分类的配色方案 |
| `landing` | 页面结构和 CTA 策略 |
| `chart` | 图表类型和库推荐 |
| `ux` | 最佳实践和反模式 |

**支持的技术栈:**
`html-tailwind` (默认)、`react`、`nextjs`、`astro`、`vue`、`nuxtjs`、`nuxt-ui`、`svelte`、`swiftui`、`react-native`、`flutter`、`shadcn`、`jetpack-compose`

---

## 安装方法

### 方法 1：作为 Claude Code Skill（推荐）

在你的项目根目录执行：

```bash
# 克隆仓库
git clone https://github.com/nextlevelbuilder/ui-ux-pro-max-skill.git

# 进入项目目录
cd ui-ux-pro-max-skill

# 创建 skill 软链接（Claude Code 会自动识别）
mkdir -p .claude/skills
ln -s ../../src/ui-ux-pro-max .claude/skills/ui-ux-pro-max
```

### 方法 2：通过 npm CLI 安装

```bash
npm install -g uipro-cli
uipro init
```

### 方法 3：直接使用 Python 脚本

```bash
# 克隆仓库
git clone https://github.com/nextlevelbuilder/ui-ux-pro-max-skill.git

# 无需安装依赖，直接使用
python3 src/ui-ux-pro-max/scripts/search.py "glassmorphism" --domain style
```

---

## 使用示例

### 搜索 UI 风格

```bash
python3 src/ui-ux-pro-max/scripts/search.py "glassmorphism" --domain style
```

### 搜索配色方案

```bash
python3 src/ui-ux-pro-max/scripts/search.py "SaaS" --domain color -n 5
```

### 搜索字体搭配

```bash
python3 src/ui-ux-pro-max/scripts/search.py "modern" --domain typography
```

### 按技术栈搜索

```bash
python3 src/ui-ux-pro-max/scripts/search.py "button" --stack react
```

### 生成设计系统

```bash
python3 src/ui-ux-pro-max/scripts/design_system.py --stack react --product saas
```

---

## 项目结构

```
src/ui-ux-pro-max/                # 数据源
├── data/                         # CSV 数据库
│   ├── products.csv              # 产品类型
│   ├── styles.csv                # UI 风格
│   ├── colors.csv                # 配色方案
│   ├── typography.csv            # 字体搭配
│   └── stacks/                   # 各技术栈指南
├── scripts/
│   ├── search.py                 # 搜索入口（BM25 + 正则混合搜索）
│   ├── core.py                   # 搜索引擎核心
│   └── design_system.py          # 设计系统生成
└── templates/                    # 模板文件
```

---

## 作为 Skill 使用时的提示词示例

当你安装了这个 skill 后，可以在对话中这样使用：

> "帮我搜索适合 SaaS 产品的玻璃态设计风格"

> "给我推荐一个现代风格的字体搭配，用于科技类网站"

> "生成一个 React + Tailwind 的设计系统，针对电商产品"

Claude 会自动调用 `search.py` 或 `design_system.py` 为你获取结果。

---

## 注意事项

- 需要 Python 3.x，**无需额外依赖**
- 数据源修改后，`.claude/` 和 `.factory/` 中的软链接会自动同步
- 所有数据的**唯一可信来源**是 `src/ui-ux-pro-max/` 目录
- 搜索支持自动领域检测（省略 `--domain` 时自动判断）

---

## 作为 Claude Skill 的 SKILL.md 内容

```markdown
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
```

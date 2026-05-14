import { readonly } from 'vue'

/**
 * CPM Design Token System
 * Read-only token accessor for components.
 * All tokens are CSS custom properties (--cpm-*) defined in salesforce-theme.css.
 * Use this composable to access token values programmatically when needed.
 */

export interface CpmColorTokens {
  brand: {
    primary: string
    accent: string
    accentHover: string
    primaryDark: string
    primaryLight: string
  }
  background: {
    page: string
    card: string
    header: string
    sidebar: string
    hover: string
    selected: string
    dark: string
    elevated: string
  }
  text: {
    primary: string
    secondary: string
    placeholder: string
    inverse: string
    muted: string
  }
  semantic: {
    success: string
    successBg: string
    successBorder: string
    warning: string
    warningBg: string
    warningBorder: string
    error: string
    errorBg: string
    errorBorder: string
    info: string
    infoBg: string
    infoBorder: string
  }
  border: {
    DEFAULT: string
    light: string
    focus: string
  }
}

export interface CpmRadiusTokens {
  sm: string
  md: string
  lg: string
  full: string
}

export interface CpmShadowTokens {
  sm: string
  md: string
  lg: string
  focus: string
}

export interface CpmSpaceTokens {
  '1': string
  '2': string
  '3': string
  '4': string
  '5': string
  '6': string
  '8': string
  '10': string
  '12': string
}

export interface CpmFontTokens {
  family: string
  size: {
    display: string
    h1: string
    h2: string
    h3: string
    body: string
    small: string
    label: string
  }
}

export interface CpmAnimationTokens {
  duration: {
    fast: string
    normal: string
    slow: string
  }
  easing: {
    default: string
    bounce: string
  }
}

export interface CpmTokens {
  color: CpmColorTokens
  radius: CpmRadiusTokens
  shadow: CpmShadowTokens
  space: CpmSpaceTokens
  font: CpmFontTokens
  animation: CpmAnimationTokens
}

const tokens: CpmTokens = {
  color: {
    brand: {
      primary: '#0F172A',
      accent: '#EA580C',
      accentHover: '#C2410C',
      primaryDark: '#020617',
      primaryLight: '#1E293B',
    },
    background: {
      page: '#F8FAFC',
      card: '#FFFFFF',
      header: '#FFFFFF',
      sidebar: '#0F172A',
      hover: '#F1F5F9',
      selected: '#EFF6FF',
      dark: '#020617',
      elevated: '#1E293B',
    },
    text: {
      primary: '#0F172A',
      secondary: '#475569',
      placeholder: '#94A3B8',
      inverse: '#FFFFFF',
      muted: '#64748B',
    },
    semantic: {
      success: '#059669',
      successBg: '#ECFDF5',
      successBorder: '#A7F3D0',
      warning: '#D97706',
      warningBg: '#FFFBEB',
      warningBorder: '#FDE68A',
      error: '#DC2626',
      errorBg: '#FEF2F2',
      errorBorder: '#FECACA',
      info: '#0369A1',
      infoBg: '#F0F9FF',
      infoBorder: '#BAE6FD',
    },
    border: {
      DEFAULT: '#E2E8F0',
      light: '#F1F5F9',
      focus: '#EA580C',
    },
  },
  radius: {
    sm: '4px',
    md: '6px',
    lg: '8px',
    full: '9999px',
  },
  shadow: {
    sm: '0 1px 2px rgba(0, 0, 0, 0.05)',
    md: '0 4px 6px -1px rgba(0, 0, 0, 0.08)',
    lg: '0 10px 15px -3px rgba(0, 0, 0, 0.08)',
    focus: '0 0 0 3px rgba(234, 88, 12, 0.15)',
  },
  space: {
    '1': '4px',
    '2': '8px',
    '3': '12px',
    '4': '16px',
    '5': '20px',
    '6': '24px',
    '8': '32px',
    '10': '40px',
    '12': '48px',
  },
  font: {
    family: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif",
    size: {
      display: '28px',
      h1: '22px',
      h2: '18px',
      h3: '16px',
      body: '14px',
      small: '12px',
      label: '11px',
    },
  },
  animation: {
    duration: {
      fast: '100ms',
      normal: '150ms',
      slow: '200ms',
    },
    easing: {
      default: 'cubic-bezier(0.4, 0, 0.2, 1)',
      bounce: 'cubic-bezier(0.16, 1, 0.3, 1)',
    },
  },
}

/**
 * Status badge style mapping for programmatic use.
 */
export const statusBadgeStyles: Record<string, { bg: string; color: string; border: string }> = {
  draft: { bg: '#FEF3C8', color: '#92400E', border: '#FDE68A' },
  pending: { bg: '#FFFBEB', color: '#92400E', border: '#FDE68A' },
  inProgress: { bg: '#F0F9FF', color: '#0369A1', border: '#BAE6FD' },
  approved: { bg: '#ECFDF5', color: '#059669', border: '#A7F3D0' },
  success: { bg: '#ECFDF5', color: '#059669', border: '#A7F3D0' },
  rejected: { bg: '#FEF2F2', color: '#DC2626', border: '#FECACA' },
  error: { bg: '#FEF2F2', color: '#DC2626', border: '#FECACA' },
  expired: { bg: '#F3F4F6', color: '#4B5563', border: '#E5E7EB' },
  invalidated: { bg: '#FEF2F2', color: '#DC2626', border: '#FECACA' },
  active: { bg: '#ECFDF5', color: '#059669', border: '#A7F3D0' },
}

/**
 * Get badge style by status string.
 */
export function getStatusBadge(status: string) {
  return statusBadgeStyles[status] || statusBadgeStyles.draft
}

/**
 * CSS variable accessor for inline styles.
 * Usage: cssVar('cpm-brand-accent') → 'var(--cpm-brand-accent)'
 */
export function cssVar(name: string): string {
  return `var(--${name})`
}

/**
 * Main composable export.
 * Returns read-only token object plus helpers.
 */
export function useTheme() {
  return {
    tokens: readonly(tokens) as Readonly<CpmTokens>,
    cssVar,
    getStatusBadge,
    statusBadgeStyles: readonly(statusBadgeStyles) as Readonly<typeof statusBadgeStyles>,
  }
}

export default useTheme

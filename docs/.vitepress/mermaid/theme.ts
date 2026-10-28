import type { MermaidConfig } from 'mermaid';

/**
 * Builds the Mermaid configuration from the VitePress theme tokens currently applied to the page,
 * so diagrams follow the brand colors in both light and dark mode.
 *
 * Mermaid derives colors with khroma, which cannot resolve `var(...)`, so the computed values are
 * read from the document. Only opaque tokens are used; translucent ones derive poorly.
 */
export function createMermaidConfig(isDark: boolean): MermaidConfig {
  const style = getComputedStyle(document.documentElement);
  const token = (name: string) => style.getPropertyValue(name).trim();

  return {
    startOnLoad: false,
    securityLevel: 'strict',
    // The component shows its own error fallback; don't let Mermaid append a syntax-error graphic to <body>.
    suppressErrorRendering: true,
    // Mermaid 12 defaults to ELK, which adds ~340 kB. Diagrams can opt in via `config: { layout: elk }`.
    layout: 'dagre',
    // Mermaid's sequence defaults (16px text, 150px actors, mirrored actors) overflow the content width
    sequence: {
      actorFontSize: 14,
      messageFontSize: 14,
      noteFontSize: 14,
      width: 120,
      actorMargin: 30,
      mirrorActors: false,
    },
    theme: 'base',
    themeVariables: {
      darkMode: isDark,
      fontFamily: token('--vp-font-family-base'),
      fontSize: '14px',
      background: token('--vp-c-bg'),
      primaryColor: token('--vp-c-bg-soft'),
      primaryTextColor: token('--vp-c-text-1'),
      primaryBorderColor: token('--vp-c-brand-1'),
      secondaryColor: token('--vp-c-bg-alt'),
      tertiaryColor: token('--vp-c-bg'),
      lineColor: token('--vp-c-text-2'),
      textColor: token('--vp-c-text-1'),
      noteBkgColor: token('--vp-c-bg-alt'),
      noteBorderColor: token('--vp-c-divider'),
      noteTextColor: token('--vp-c-text-1'),
    },
  };
}

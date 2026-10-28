// Based on vitepress-plugin-mermaid (MIT, Copyright (c) 2022 Emerson Bottero)
// https://github.com/emersonbottero/vitepress-plugin-mermaid

import type { MarkdownRenderer } from 'vitepress';

/**
 * Renders ```mermaid fences as `<MermaidDiagram>` components, which draw the diagram on the client.
 * All other fences keep their default rendering.
 */
export function mermaidMarkdown(md: MarkdownRenderer): void {
  const fence = md.renderer.rules.fence!;

  md.renderer.rules.fence = (tokens, index, options, env, self) => {
    const token = tokens[index];

    if (token.info.trim() !== 'mermaid')
      return fence(tokens, index, options, env, self);

    return `<MermaidDiagram code="${encodeURIComponent(token.content)}" />`;
  };
}

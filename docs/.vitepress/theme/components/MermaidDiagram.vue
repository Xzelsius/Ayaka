<!--
  Based on vitepress-plugin-mermaid (MIT, Copyright (c) 2022 Emerson Bottero)
  https://github.com/emersonbottero/vitepress-plugin-mermaid
-->
<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, useId } from 'vue';
import { createMermaidConfig } from '../../mermaid/theme';

const props = defineProps<{
  /** URI-encoded diagram source, as emitted by the markdown fence rule. */
  code: string
}>();

/** Wide diagrams shrink to fit the content, but not below this width; narrow screens scroll instead. */
const MIN_READABLE_WIDTH = 560;

const source = decodeURIComponent(props.code);
const id = `mermaid-${useId()}`;

const svg = ref<string>();
const error = ref<string>();

const minWidth = computed(() => {
  const width = Number(svg.value?.match(/viewBox="[\d.-]+ [\d.-]+ ([\d.]+)/)?.[1]);
  return width
    ? `${Math.min(width, MIN_READABLE_WIDTH)}px`
    : undefined;
});

let observer: MutationObserver | undefined;
let renderedDark: boolean | undefined;
let version = 0;

async function render(): Promise<void> {
  const isDark = document.documentElement.classList.contains('dark');
  if (isDark === renderedDark)
    return;
  renderedDark = isDark;

  // A theme switch can start a new render before the previous one finished; only the latest wins.
  const current = ++version;

  try {
    // Loaded on demand, so pages without diagrams never download Mermaid.
    const { default: mermaid } = await import('mermaid');
    mermaid.initialize(createMermaidConfig(isDark));
    const result = await mermaid.render(id, source);

    if (current === version) {
      svg.value = result.svg;
      error.value = undefined;
    }
  }
  catch (e) {
    if (current === version) {
      error.value = e instanceof Error
        ? e.message
        : String(e);
    }
    console.error(`[mermaid] Failed to render diagram ${id}`, e);
  }
}

onMounted(() => {
  // VitePress toggles the `dark` class on <html> when the color scheme changes.
  observer = new MutationObserver(render);
  observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ['class'],
  });
  render();
});

onBeforeUnmount(() => observer?.disconnect());
</script>

<template>
  <div v-if="error" class="mermaid-diagram mermaid-diagram--error">
    <p>Diagram could not be rendered: {{ error }}</p>
    <pre>{{ source }}</pre>
  </div>
  <div v-else-if="svg" class="mermaid-diagram" :style="{ '--mermaid-min-width': minWidth }" v-html="svg" />
  <div v-else class="mermaid-diagram mermaid-diagram--loading" aria-busy="true" />
</template>

<style scoped>
.mermaid-diagram {
  margin: 16px 0;
  overflow-x: auto;
  text-align: center;
}

.mermaid-diagram :deep(svg) {
  max-width: 100%;
  min-width: var(--mermaid-min-width, 0);
  height: auto;
}

/* VitePress' `.vp-doc p` line-height leaks into labels that Mermaid sized with its own line-height */
.mermaid-diagram :deep(foreignObject p) {
  line-height: inherit;
}

.mermaid-diagram--loading {
  min-height: 160px;
  border-radius: 8px;
  background-color: var(--vp-c-bg-soft);
}

.mermaid-diagram--error {
  padding: 16px;
  border: 1px solid var(--vp-c-danger-1);
  border-radius: 8px;
  text-align: left;
  color: var(--vp-c-danger-1);
}

.mermaid-diagram--error pre {
  margin: 8px 0 0;
  white-space: pre-wrap;
  color: var(--vp-c-text-2);
}
</style>

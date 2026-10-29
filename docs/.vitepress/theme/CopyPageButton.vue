<script setup>
import { onMounted, onUnmounted, ref } from 'vue'
import { useCopyOrDownloadAsMarkdownButtons } from 'vitepress-plugin-llms/vitepress-components'

// Theme-styled front end for the copy page logic of vitepress-plugin-llms
const { aiProviders, copied, copyAsMarkdown, downloadMarkdown, downloaded, openInAI, viewAsMarkdown } =
  useCopyOrDownloadAsMarkdownButtons()

const open = ref(false)
const root = ref()

function run(action) {
  open.value = false
  action()
}

function onDocumentClick(event) {
  if (root.value && !root.value.contains(event.target)) open.value = false
}

function onKeydown(event) {
  if (event.key === 'Escape') open.value = false
}

onMounted(() => {
  document.addEventListener('click', onDocumentClick)
  document.addEventListener('keydown', onKeydown)
})

onUnmounted(() => {
  document.removeEventListener('click', onDocumentClick)
  document.removeEventListener('keydown', onKeydown)
})
</script>

<template>
  <div ref="root" class="copy-page">
    <div class="group">
      <button class="part main" type="button" @click="run(copyAsMarkdown)">
        <svg v-if="copied" class="icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M20 6 9 17l-5-5" /></svg>
        <svg v-else class="icon" viewBox="0 0 24 24" aria-hidden="true"><rect x="8" y="8" width="13" height="13" rx="2" /><path d="M4 16V5a1 1 0 0 1 1-1h11" /></svg>
        {{ copied ? 'Copied' : 'Copy page' }}
      </button>
      <button
        class="part toggle"
        type="button"
        aria-label="More page options"
        aria-haspopup="menu"
        :aria-expanded="open"
        @click="open = !open"
      >
        <svg class="icon chevron" :class="{ open }" viewBox="0 0 24 24" aria-hidden="true"><path d="m6 9 6 6 6-6" /></svg>
      </button>
    </div>

    <div v-show="open" class="menu" role="menu">
      <button class="item" type="button" role="menuitem" @click="run(viewAsMarkdown)">
        View as Markdown
        <svg class="icon external" viewBox="0 0 24 24" aria-hidden="true"><path d="M7 17 17 7M8 7h9v9" /></svg>
      </button>
      <button class="item" type="button" role="menuitem" @click="run(downloadMarkdown)">
        {{ downloaded ? 'Downloaded' : 'Download as Markdown' }}
      </button>
      <div class="divider" />
      <button
        v-for="provider in aiProviders"
        :key="provider.name"
        class="item"
        type="button"
        role="menuitem"
        @click="run(() => openInAI(provider))"
      >
        Open in {{ provider.name }}
        <svg class="icon external" viewBox="0 0 24 24" aria-hidden="true"><path d="M7 17 17 7M8 7h9v9" /></svg>
      </button>
    </div>
  </div>
</template>

<style scoped>
.copy-page {
  position: relative;
  display: inline-block;
  margin-top: 16px;
}

.group {
  display: flex;
  border: 2px solid var(--alba-outline);
  border-radius: 10px;
  background-color: var(--vp-button-alt-bg);
  box-shadow: 0 3px 0 var(--alba-btn-edge);
  overflow: hidden;
  transition: transform 0.12s, box-shadow 0.12s;
}

.group:active {
  transform: translateY(3px);
  box-shadow: 0 0 0 var(--alba-btn-edge);
}

.part {
  display: flex;
  align-items: center;
  gap: 6px;
  height: 28px;
  font-size: 13px;
  font-weight: 600;
  color: var(--vp-button-alt-text);
  transition: color 0.25s, background-color 0.25s;
}

.part:hover {
  color: var(--vp-button-alt-hover-text);
  background-color: var(--vp-button-alt-hover-bg);
}

.main {
  padding: 0 10px 0 14px;
}

.toggle {
  padding: 0 10px 0 8px;
  border-left: 2px solid var(--alba-outline);
}

.icon {
  width: 15px;
  height: 15px;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.chevron {
  transition: transform 0.25s;
}

.chevron.open {
  transform: rotate(180deg);
}

.menu {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  z-index: 20;
  min-width: 220px;
  padding: 12px;
  border: 2px solid var(--alba-outline);
  border-radius: 14px;
  background-color: var(--vp-c-bg-elv);
  box-shadow: 6px 6px 0 var(--alba-mint-strong);
}

.item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 0 12px;
  border-radius: 6px;
  line-height: 32px;
  font-size: 14px;
  font-weight: 500;
  text-align: left;
  color: var(--vp-c-text-1);
  white-space: nowrap;
  transition: background-color 0.25s, color 0.25s;
}

.item:hover {
  color: var(--vp-c-text-1);
  background-color: var(--alba-mint);
}

.external {
  width: 12px;
  height: 12px;
  margin-left: 12px;
  color: var(--vp-c-text-3);
}

.divider {
  margin: 8px 12px;
  border-top: 1px solid var(--vp-c-divider);
}
</style>

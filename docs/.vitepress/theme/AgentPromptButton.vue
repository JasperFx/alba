<script setup>
import { onMounted, ref } from 'vue'
import { withBase } from 'vitepress'
import { VPButton } from 'vitepress/theme'

const promptUrl = withBase('/agent-prompt.md')
const label = ref('Copy agent prompt')
let prompt = null

async function loadPrompt() {
  prompt ??= await fetch(promptUrl).then(r => r.text())
  return prompt
}

// Loaded up front so the click can write to the clipboard straight away, which Safari requires
onMounted(() => loadPrompt().catch(() => {}))

// VPButton rewrites internal hrefs as page links, so the raw file is opened from script
function view() {
  window.open(promptUrl, '_blank')
}

async function copy() {
  try {
    await navigator.clipboard.writeText(await loadPrompt())
    label.value = 'Copied!'
  } catch {
    view()
    return
  }
  setTimeout(() => (label.value = 'Copy agent prompt'), 2000)
}
</script>

<template>
  <div class="agent-prompt-actions">
    <VPButton theme="brand" :text="label" @click="copy" />
    <VPButton theme="alt" text="View the prompt" @click="view" />
  </div>
</template>

<style scoped>
.agent-prompt-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin: 16px 0;
}
</style>

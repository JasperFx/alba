import { h } from 'vue'
import DefaultTheme from 'vitepress/theme-without-fonts'
import '@fontsource-variable/bitter'
import '@fontsource-variable/figtree'
import '@fontsource-variable/jetbrains-mono'
import AgentPromptButton from './AgentPromptButton.vue'
import CopyPageButton from './CopyPageButton.vue'
import HeroCode from './HeroCode.md'
import './custom.css'

export default {
  ...DefaultTheme,
  // Code sample in place of the home page hero image
  Layout: () => h(DefaultTheme.Layout, null, {
    'home-hero-image': () => h('div', { class: 'hero-code vp-doc' }, [h(HeroCode)])
  }),
  enhanceApp({ app }) {
    // vitepress-plugin-llms inserts this component under each page title
    app.component('CopyOrDownloadAsMarkdownButtons', CopyPageButton)
    app.component('AgentPromptButton', AgentPromptButton)
  }
}

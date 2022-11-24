import { defineConfig } from 'vitepress'
import { BUNDLED_LANGUAGES } from 'shiki'

// Include `cs` as alias for csharp
BUNDLED_LANGUAGES
  .find(lang => lang.id === 'csharp').aliases.push('cs');


export default defineConfig({
    title: 'Alba',
    description: 'Supercharged integration testing for ASP.Net Core web services',
    base: '/alba/',
    head: [],
    themeConfig: {
        logo: null,
        socialLinks: [
            { icon: 'github', link: 'https://github.com/JasperFx/alba' }
        ],

        editLink: {
            pattern: 'https://github.com/JasperFx/alba/edit/master/docs/:path',
            text: 'Suggest changes to this page'
        },

        nav: [
            { text: 'Guide', link: '/guide/hosting' },
            { text: 'Gitter | Join Chat', link: 'https://gitter.im/JasperFx/alba?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge' }
        ],

        algolia: {
            appId: '2V5OM390DF',
            apiKey: '674cd4f3e6b6ebe232a980c7cc5a0270',
            indexName: 'alba_index'
        },

        sidebar: [
            {
                text: 'Getting Started',
                items: getGuideSidebar()
            },
            {
                text: 'Scenario Testing',
                items: getScenarioSidebar()
            }
        ]
    },
    markdown: {
        linkify: false
    }
})


function getGuideSidebar() {
    return [
        { text: 'Alba Setup', link: '/guide/gettingstarted' },
        { text: 'Integrating with xUnit.Net', link: '/guide/xunit' },
        { text: 'Integrating with NUnit', link: '/guide/nunit' },
        { text: 'Extension Model', link: '/guide/extensions' },
        { text: 'Security Extensions', link: '/guide/security' },
        { text: 'Alternative Bootstrapping', link:'/guide/bootstrapping'},
        { text: 'History & Architecture', link: '/guide/history' },
    ]
}

function getScenarioSidebar() {
    return [
        { text: 'Writing Scenarios', link: '/scenarios/writingscenarios' },      
        { text: 'Specifying Urls', link: '/scenarios/urls' },
        { text: 'HTTP Status Codes', link: '/scenarios/statuscode' },
        { text: 'HTTP Headers', link: '/scenarios/headers' },
        { text: 'JSON Web Services', link: '/scenarios/json' },
        { text: 'Xml Web Services', link: '/scenarios/xml' },
        { text: 'Plain Text Web Services', link: '/scenarios/text' },
        { text: 'Before and After Actions', link: '/scenarios/setup' },
        { text: 'Custom Assertions', link: '/scenarios/assertions' },
        { text: 'Redirects', link: '/scenarios/redirects' },
    ]
}

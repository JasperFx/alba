import { defineConfig } from 'vitepress'

export default defineConfig({
    title: 'Alba',
    description: 'Supercharged integration testing for ASP.Net Core web services',
    base: '/alba/',
    head: [],
    themeConfig: {
        socialLinks: [
            { icon: 'github', link: 'https://github.com/JasperFx/alba' }
        ],

        editLink: {
            pattern: 'https://github.com/JasperFx/alba/edit/master/docs/:path',
            text: 'Suggest changes to this page'
        },

        nav: [
            { text: 'Docs', link: '/guide/gettingstarted' },
            { text: 'Discord | Join Chat', link: 'https://discord.gg/WMxrvegf8H' }
        ],

        algolia: {
            appId: '2V5OM390DF',
            apiKey: '674cd4f3e6b6ebe232a980c7cc5a0270',
            indexName: 'alba_index'
        },

        sidebar: [
            {
                text: 'Getting Started',
                collapsed: false,
                items: getGuideSidebar()
            },
            {
                text: 'Scenario Testing',
                collapsed: false,
                items: getScenarioSidebar()
            }
        ]
    },
    markdown: {
        linkify: false,
    }
})


function getGuideSidebar() {
    return [
        { text: 'Alba Setup', link: '/guide/gettingstarted' },
        { text: 'Integrating with xUnit.Net', link: '/guide/xunit' },
        { text: 'Integrating with NUnit', link: '/guide/nunit' },
        { text: 'Extension Model', link: '/guide/extensions' },
        { text: 'Security Extensions', link: '/guide/security' },
        { text: 'Tracing & Open Telemetry', link: '/guide/opentelemetry' },
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
        { text: 'Plain Text Web Services', link: '/scenarios/text' },
        { text: 'Xml Web Services', link: '/scenarios/xml' },
        { text: 'Sending Form Data', link: '/scenarios/formdata'},
        { text: 'Before and After Actions', link: '/scenarios/setup' },
        { text: 'Custom Assertions', link: '/scenarios/assertions' },
        { text: 'Redirects', link: '/scenarios/redirects' },
    ]
}

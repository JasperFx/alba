module.exports = {
    title: 'Alba',
    description: 'Supercharged integration testing for ASP.Net Core web services',
    base: '/alba/',
    head: [],
    themeConfig: {
        logo: null,
        repo: 'JasperFx/alba',
        docsDir: 'docs',
        docsBranch: 'master',
        editLinks: true,
        editLinkText: 'Suggest changes to this page',

        nav: [
            { text: 'Guide', link: '/guide/' },
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
                link: '/guide/',
                children: getGuideSidebar()
            },
            {
                text: 'Scenario Testing',
                link: '/scenarios/',
                children: getScenarioSidebar()
            }
        ]
    },
    markdown: {
        linkify: false
    }
}

function getGuideSidebar() {
    return [
        {text: 'Alba Setup', link: '/guide/hosting'},
        {text: 'Integrating with xUnit.Net', link: '/guide/xunit'},
        {text: 'Integrating with NUnit', link: '/guide/nunit'},
        {text: 'Extension Model', link: '/guide/extensions'},
        {text: 'Security Extensions', link: '/guide/security'}
    ]
}

function getScenarioSidebar(){
    return [
        {text: 'Set up and tear down actions', link: '/scenarios/setup'},
        {text: 'Specifying Urls', link: '/scenarios/urls'},
        {text: 'HTTP Status Codes', link: '/scenarios/statuscode'},
        {text: 'HTTP Headers', link: '/scenarios/headers'},
        {text: 'Json Web Services', link: '/scenarios/json'},
        {text: 'Xml Web Services', link: '/scenarios/xml'},
        {text: 'Plain Text Web Services', link: '/scenarios/text'},
        {text: 'Custom Assertions', link: '/scenarios/assertions'},
        {text: 'Redirects', link: '/scenarios/redirects'},
    ]
}

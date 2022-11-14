import{_ as n,c as s,o as a,a as t}from"./app.bb080ae4.js";const _='{"title":"Integrating with NUnit","description":"","frontmatter":{},"relativePath":"guide/nunit.md","lastUpdated":1668446412987}',p={},o=t(`<h1 id="integrating-with-nunit" tabindex="-1">Integrating with NUnit <a class="header-anchor" href="#integrating-with-nunit" aria-hidden="true">#</a></h1><p>When using Alba within <a href="./.html">NUnit testing projects</a>, you probably want to reuse the <code>IAlbaHost</code> across tests and test fixtures because <code>AlbaHost</code> is relatively expensive to create (it&#39;s bootstrapping your whole application more than Alba itself is slow). To do that with NUnit, you could track a single <code>AlbaHost</code> on a static class like this one:</p><p><a id="snippet-sample_nunit_application"></a></p><div class="language-cs"><pre><code><span class="token punctuation">[</span><span class="token attribute"><span class="token class-name">SetUpFixture</span></span><span class="token punctuation">]</span>
<span class="token keyword">public</span> <span class="token keyword">class</span> <span class="token class-name">Application</span>
<span class="token punctuation">{</span>
    <span class="token comment">// Make this lazy so you don&#39;t build it out</span>
    <span class="token comment">// when you don&#39;t need it.</span>
    <span class="token keyword">private</span> <span class="token keyword">static</span> <span class="token keyword">readonly</span> <span class="token class-name">Lazy<span class="token punctuation">&lt;</span>IAlbaHost<span class="token punctuation">&gt;</span></span> _host<span class="token punctuation">;</span>

    <span class="token keyword">static</span> <span class="token function">Application</span><span class="token punctuation">(</span><span class="token punctuation">)</span>
    <span class="token punctuation">{</span>
        _host <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">Lazy<span class="token punctuation">&lt;</span>IAlbaHost<span class="token punctuation">&gt;</span></span><span class="token punctuation">(</span><span class="token punctuation">(</span><span class="token punctuation">)</span> <span class="token operator">=&gt;</span> Program
            <span class="token punctuation">.</span><span class="token function">CreateHostBuilder</span><span class="token punctuation">(</span>Array<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">Empty</span><span class="token generic class-name"><span class="token punctuation">&lt;</span><span class="token keyword">string</span><span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span>
            <span class="token punctuation">.</span><span class="token function">StartAlba</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
    <span class="token punctuation">}</span>

    <span class="token keyword">public</span> <span class="token keyword">static</span> <span class="token return-type class-name">IAlbaHost</span> AlbaHost <span class="token operator">=&gt;</span> _host<span class="token punctuation">.</span>Value<span class="token punctuation">;</span>

    <span class="token comment">// Make sure that NUnit will shut down the AlbaHost when</span>
    <span class="token comment">// all the projects are finished</span>
    <span class="token punctuation">[</span>OneTimeTearDown<span class="token punctuation">]</span>
    <span class="token keyword">public</span> <span class="token return-type class-name"><span class="token keyword">void</span></span> <span class="token function">Teardown</span><span class="token punctuation">(</span><span class="token punctuation">)</span>
    <span class="token punctuation">{</span>
        <span class="token keyword">if</span> <span class="token punctuation">(</span>_host<span class="token punctuation">.</span>IsValueCreated<span class="token punctuation">)</span>
        <span class="token punctuation">{</span>
            _host<span class="token punctuation">.</span>Value<span class="token punctuation">.</span><span class="token function">Dispose</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        <span class="token punctuation">}</span>
    <span class="token punctuation">}</span>
<span class="token punctuation">}</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/NUnitSamples/UnitTest1.cs#L11-L41" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_nunit_application" title="Start of snippet">anchor</a></sup></p><p>Then reference the <code>AlbaHost</code> in tests like this sample:</p><p><a id="snippet-sample_nunit_scenario_test"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token keyword">class</span> <span class="token class-name">sample_integration_fixture</span>
<span class="token punctuation">{</span>
    <span class="token punctuation">[</span><span class="token attribute"><span class="token class-name">Test</span></span><span class="token punctuation">]</span>
    <span class="token keyword">public</span> <span class="token return-type class-name">Task</span> <span class="token function">happy_path</span><span class="token punctuation">(</span><span class="token punctuation">)</span>
    <span class="token punctuation">{</span>
        <span class="token keyword">return</span> Application<span class="token punctuation">.</span>AlbaHost<span class="token punctuation">.</span><span class="token function">Scenario</span><span class="token punctuation">(</span>_ <span class="token operator">=&gt;</span>
        <span class="token punctuation">{</span>
            _<span class="token punctuation">.</span>Get<span class="token punctuation">.</span><span class="token function">Url</span><span class="token punctuation">(</span><span class="token string">&quot;/fake/okay&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
            _<span class="token punctuation">.</span><span class="token function">StatusCodeShouldBeOk</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
        <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
    <span class="token punctuation">}</span>
<span class="token punctuation">}</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/NUnitSamples/UnitTest1.cs#L43-L56" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_nunit_scenario_test" title="Start of snippet">anchor</a></sup></p>`,9),e=[o];function c(i,l,u,k,r,d){return a(),s("div",null,e)}var m=n(p,[["render",c]]);export{_ as __pageData,m as default};

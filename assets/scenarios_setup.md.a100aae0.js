import{_ as n,c as s,o as a,a as t}from"./app.bb080ae4.js";const f='{"title":"Before and after actions","description":"","frontmatter":{},"relativePath":"scenarios/setup.md","lastUpdated":1644151252706}',e={},o=t(`<h1 id="before-and-after-actions" tabindex="-1">Before and after actions <a class="header-anchor" href="#before-and-after-actions" aria-hidden="true">#</a></h1><div class="warning custom-block"><p class="custom-block-title">WARNING</p><p>The Before/After actions are <strong>not</strong> additive. The last one specified is the only one executed.</p></div><p>As of Alba 2.0, you can specify actions that run immediately before or after an HTTP request is executed for common setup or teardown work like setting up authentication credentials or tracing or whatever.</p><p>Here&#39;s a sample:</p><p><a id="snippet-sample_before_and_after"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token return-type class-name"><span class="token keyword">void</span></span> <span class="token function">sample_usage</span><span class="token punctuation">(</span><span class="token class-name">AlbaHost</span> system<span class="token punctuation">)</span>
<span class="token punctuation">{</span>
    <span class="token comment">// Synchronously</span>
    system<span class="token punctuation">.</span><span class="token function">BeforeEach</span><span class="token punctuation">(</span>context <span class="token operator">=&gt;</span>
    <span class="token punctuation">{</span>
        <span class="token comment">// Modify the HttpContext immediately before each</span>
        <span class="token comment">// Scenario()/HTTP request is executed</span>
        context<span class="token punctuation">.</span>Request<span class="token punctuation">.</span>Headers<span class="token punctuation">.</span><span class="token function">Add</span><span class="token punctuation">(</span><span class="token string">&quot;trace&quot;</span><span class="token punctuation">,</span> <span class="token string">&quot;something&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

    system<span class="token punctuation">.</span><span class="token function">AfterEach</span><span class="token punctuation">(</span>context <span class="token operator">=&gt;</span>
    <span class="token punctuation">{</span>
        <span class="token comment">// perform an action immediately after the scenario/HTTP request</span>
        <span class="token comment">// is executed</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

    <span class="token comment">// Asynchronously</span>
    system<span class="token punctuation">.</span><span class="token function">BeforeEachAsync</span><span class="token punctuation">(</span>context <span class="token operator">=&gt;</span>
    <span class="token punctuation">{</span>
        <span class="token comment">// do something asynchronous here</span>
        <span class="token keyword">return</span> Task<span class="token punctuation">.</span>CompletedTask<span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

    system<span class="token punctuation">.</span><span class="token function">AfterEachAsync</span><span class="token punctuation">(</span>context <span class="token operator">=&gt;</span>
    <span class="token punctuation">{</span>
        <span class="token comment">// do something asynchronous here</span>
        <span class="token keyword">return</span> Task<span class="token punctuation">.</span>CompletedTask<span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

<span class="token punctuation">}</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/before_and_after_actions.cs#L33-L66" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_before_and_after" title="Start of snippet">anchor</a></sup></p>`,7),p=[o];function c(u,i,r,l,k,d){return a(),s("div",null,p)}var h=n(e,[["render",c]]);export{f as __pageData,h as default};

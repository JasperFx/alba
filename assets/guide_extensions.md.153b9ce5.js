import{_ as n,c as s,o as a,a as t}from"./app.bb080ae4.js";const b='{"title":"Extension Model","description":"","frontmatter":{},"relativePath":"guide/extensions.md","lastUpdated":1668446440151}',e={},o=t(`<h1 id="extension-model" tabindex="-1">Extension Model <a class="header-anchor" href="#extension-model" aria-hidden="true">#</a></h1><p>Alba V5 added a new extension model based on this interface:</p><p><a id="snippet-sample_ialbaextension"></a></p><div class="language-cs"><pre><code><span class="token comment">/// &lt;summary&gt;</span>
<span class="token comment">/// Models an extension to an AlbaHost</span>
<span class="token comment">/// &lt;/summary&gt;</span>
<span class="token keyword">public</span> <span class="token keyword">interface</span> <span class="token class-name">IAlbaExtension</span> <span class="token punctuation">:</span> <span class="token type-list"><span class="token class-name">IDisposable</span><span class="token punctuation">,</span> <span class="token class-name">IAsyncDisposable</span></span>
<span class="token punctuation">{</span>
    <span class="token comment">/// &lt;summary&gt;</span>
    <span class="token comment">/// Called during the initialization of an AlbaHost after the application is started,</span>
    <span class="token comment">/// so the application DI container is available. Useful for registering setup or teardown</span>
    <span class="token comment">/// actions on an AlbaHOst</span>
    <span class="token comment">/// &lt;/summary&gt;</span>
    <span class="token comment">/// &lt;param name=&quot;host&quot;&gt;&lt;/param&gt;</span>
    <span class="token comment">/// &lt;returns&gt;&lt;/returns&gt;</span>
    <span class="token return-type class-name">Task</span> <span class="token function">Start</span><span class="token punctuation">(</span><span class="token class-name">IAlbaHost</span> host<span class="token punctuation">)</span><span class="token punctuation">;</span>
    
    <span class="token comment">/// &lt;summary&gt;</span>
    <span class="token comment">/// Allow an extension to alter the application&#39;s</span>
    <span class="token comment">/// IHostBuilder prior to starting the application</span>
    <span class="token comment">/// &lt;/summary&gt;</span>
    <span class="token comment">/// &lt;param name=&quot;builder&quot;&gt;&lt;/param&gt;</span>
    <span class="token comment">/// &lt;returns&gt;&lt;/returns&gt;</span>
    <span class="token return-type class-name">IHostBuilder</span> <span class="token function">Configure</span><span class="token punctuation">(</span><span class="token class-name">IHostBuilder</span> builder<span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token punctuation">}</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/Alba/IAlbaExtension.cs#L7-L32" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_ialbaextension" title="Start of snippet">anchor</a></sup></p><p>When you are initializing an <code>AlbaHost</code>, you can pass in an optional array of extensions like this sample from the security stub testing:</p><p><a id="snippet-sample_bootstrapping_with_stub_extension"></a></p><div class="language-cs"><pre><code><span class="token comment">// This is calling your real web service&#39;s configuration</span>
<span class="token class-name"><span class="token keyword">var</span></span> hostBuilder <span class="token operator">=</span> WebAppSecuredWithJwt<span class="token punctuation">.</span>Program
    <span class="token punctuation">.</span><span class="token function">CreateHostBuilder</span><span class="token punctuation">(</span>Array<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">Empty</span><span class="token generic class-name"><span class="token punctuation">&lt;</span><span class="token keyword">string</span><span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

<span class="token comment">// This is a new Alba v5 extension that can &quot;stub&quot; out</span>
<span class="token comment">// JWT token authentication</span>
<span class="token class-name"><span class="token keyword">var</span></span> securityStub <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">AuthenticationStub</span><span class="token punctuation">(</span><span class="token punctuation">)</span>
    <span class="token punctuation">.</span><span class="token function">With</span><span class="token punctuation">(</span><span class="token string">&quot;foo&quot;</span><span class="token punctuation">,</span> <span class="token string">&quot;bar&quot;</span><span class="token punctuation">)</span>
    <span class="token punctuation">.</span><span class="token function">With</span><span class="token punctuation">(</span>JwtRegisteredClaimNames<span class="token punctuation">.</span>Email<span class="token punctuation">,</span> <span class="token string">&quot;guy@company.com&quot;</span><span class="token punctuation">)</span>
    <span class="token punctuation">.</span><span class="token function">WithName</span><span class="token punctuation">(</span><span class="token string">&quot;jeremy&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

<span class="token comment">// AlbaHost was &quot;SystemUnderTest&quot; in previous versions of</span>
<span class="token comment">// Alba</span>
theHost <span class="token operator">=</span> <span class="token keyword">new</span> <span class="token constructor-invocation class-name">AlbaHost</span><span class="token punctuation">(</span>hostBuilder<span class="token punctuation">,</span> securityStub<span class="token punctuation">)</span><span class="token punctuation">;</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Security/web_api_authentication_with_stub.cs#L21-L38" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_bootstrapping_with_stub_extension" title="Start of snippet">anchor</a></sup></p>`,9),p=[o];function c(l,i,u,r,k,m){return a(),s("div",null,p)}var h=n(e,[["render",c]]);export{b as __pageData,h as default};

import{_ as s,c as a,o as n,a as t}from"./app.bb080ae4.js";const _='{"title":"Http Status Codes","description":"","frontmatter":{},"relativePath":"scenarios/statuscode.md","lastUpdated":1668446412983}',e={},o=t(`<h1 id="http-status-codes" tabindex="-1">Http Status Codes <a class="header-anchor" href="#http-status-codes" aria-hidden="true">#</a></h1><p>You can declaratively check the status code with this syntax:</p><p><a id="snippet-sample_check_the_status_code"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token return-type class-name">Task</span> <span class="token function">check_the_status</span><span class="token punctuation">(</span><span class="token class-name">IAlbaHost</span> system<span class="token punctuation">)</span>
<span class="token punctuation">{</span>
    <span class="token keyword">return</span> system<span class="token punctuation">.</span><span class="token function">Scenario</span><span class="token punctuation">(</span>_ <span class="token operator">=&gt;</span>
    <span class="token punctuation">{</span>
        <span class="token comment">// Shorthand for saying that the StatusCode should be 200</span>
        _<span class="token punctuation">.</span><span class="token function">StatusCodeShouldBeOk</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

        <span class="token comment">// Or a specific status code</span>
        _<span class="token punctuation">.</span><span class="token function">StatusCodeShouldBe</span><span class="token punctuation">(</span><span class="token number">403</span><span class="token punctuation">)</span><span class="token punctuation">;</span>

        <span class="token comment">// Ignore the status code altogether</span>
        _<span class="token punctuation">.</span><span class="token function">IgnoreStatusCode</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>
<span class="token punctuation">}</span>
</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/StatusCodes.cs#L7-L22" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_check_the_status_code" title="Start of snippet">anchor</a></sup></p><p>Do note that by default, if you do not specify the expected status code, Alba assumes that the request should return 200 (OK) and will fail the scenario if a different status code is found. You can ignore that check with the <code>Scenario.IgnoreStatusCode()</code> method.</p>`,6),p=[o];function c(u,i,l,r,d,h){return n(),a("div",null,p)}var f=s(e,[["render",c]]);export{_ as __pageData,f as default};

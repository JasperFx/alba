import{o as s,c as n,a,b as t,d as p}from"./app.144e7b1a.js";const e='{"title":"Class Fixtures","description":"","frontmatter":"title:Integrating Alba with xUnit.Net editLink:true","headers":[{"level":2,"title":"Class Fixtures","slug":"class-fixtures"},{"level":2,"title":"Collection Fixtures","slug":"collection-fixtures"}],"relativePath":"guide/xunit.md","lastUpdated":1629228268014}',o={},c=t("p",null,"If you are writing only a few Alba specifications in your testing project and your application spins up very quickly, you can just happily write tests like this:",-1),l=p('<p><a id="snippet-sample_should_say_hello_world"></a></p><div class="language-cs"><pre><code><span class="token punctuation">[</span><span class="token attribute"><span class="token class-name">Fact</span></span><span class="token punctuation">]</span>\n<span class="token keyword">public</span> <span class="token keyword">async</span> <span class="token return-type class-name">Task</span> <span class="token function">should_say_hello_world</span><span class="token punctuation">(</span><span class="token punctuation">)</span>\n<span class="token punctuation">{</span>\n    <span class="token keyword">await</span> <span class="token keyword">using</span> <span class="token class-name"><span class="token keyword">var</span></span> host <span class="token operator">=</span> <span class="token keyword">await</span> Program\n        <span class="token punctuation">.</span><span class="token function">CreateHostBuilder</span><span class="token punctuation">(</span>Array<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">Empty</span><span class="token generic class-name"><span class="token punctuation">&lt;</span><span class="token keyword">string</span><span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span>\n        \n        <span class="token comment">// This extension method is just a shorter version</span>\n        <span class="token comment">// of new AlbaHost(builder)</span>\n        <span class="token punctuation">.</span><span class="token function">StartAlbaAsync</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    \n    <span class="token comment">// This runs an HTTP request and makes an assertion</span>\n    <span class="token comment">// about the expected content of the response</span>\n    <span class="token keyword">await</span> host<span class="token punctuation">.</span><span class="token function">Scenario</span><span class="token punctuation">(</span>_ <span class="token operator">=&gt;</span>\n    <span class="token punctuation">{</span>\n        _<span class="token punctuation">.</span>Get<span class="token punctuation">.</span><span class="token function">Url</span><span class="token punctuation">(</span><span class="token string">&quot;/&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n        _<span class="token punctuation">.</span><span class="token function">ContentShouldBe</span><span class="token punctuation">(</span><span class="token string">&quot;Hello, World!&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n        _<span class="token punctuation">.</span><span class="token function">StatusCodeShouldBeOk</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Quickstart.cs#L30-L50" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_should_say_hello_world" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><p>Do note that your <code>[Fact]</code> method needs to be declared as <code>async Task</code> to ensure that xUnit finishes the specification before disposing the system or you&#39;ll get <em>unusual</em> behavior. Also note that you really need to dispose the <code>AlbaHost</code> to shut down your application and dispose any internal services that might be holding on to computer resources.</p><h2 id="class-fixtures"><a class="header-anchor" href="#class-fixtures" aria-hidden="true">#</a> Class Fixtures</h2><p>If your application startup time becomes a performance problem, and especially in larger test suites, you probably want to share the <code>AlbaHost</code> object between tests. xUnit helpfully provides the <a href="https://xunit.github.io/docs/shared-context" target="_blank" rel="noopener noreferrer">class fixture feature</a> for just this use case.</p><p>In this case, build out your <code>AlbaHost</code> in a class like this:</p>',7),i=p('<p><a id="snippet-sample_xunit_fixture"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token keyword">class</span> <span class="token class-name">WebAppFixture</span> <span class="token punctuation">:</span> <span class="token type-list"><span class="token class-name">IDisposable</span></span>\n<span class="token punctuation">{</span>\n    <span class="token keyword">public</span> <span class="token keyword">readonly</span> <span class="token class-name">IAlbaHost</span> AlbaHost <span class="token operator">=</span> WebApp<span class="token punctuation">.</span>Program\n        <span class="token punctuation">.</span><span class="token function">CreateHostBuilder</span><span class="token punctuation">(</span>Array<span class="token punctuation">.</span><span class="token generic-method"><span class="token function">Empty</span><span class="token generic class-name"><span class="token punctuation">&lt;</span><span class="token keyword">string</span><span class="token punctuation">&gt;</span></span></span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">)</span>\n        <span class="token punctuation">.</span><span class="token function">StartAlba</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n\n    <span class="token keyword">public</span> <span class="token return-type class-name"><span class="token keyword">void</span></span> <span class="token function">Dispose</span><span class="token punctuation">(</span><span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n        AlbaHost<span class="token punctuation">?.</span><span class="token function">Dispose</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    <span class="token punctuation">}</span>\n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ContractTestWithAlba.cs#L9-L21" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_xunit_fixture" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><p>Then in your actual xUnit fixture classes, implement the <code>IClassFixture&lt;T&gt;</code> class like this:</p>',4),u=p('<p><a id="snippet-sample_using_xunit_fixture"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token keyword">class</span> <span class="token class-name">ContractTestWithAlba</span> <span class="token punctuation">:</span> <span class="token type-list"><span class="token class-name">IClassFixture<span class="token punctuation">&lt;</span>WebAppFixture<span class="token punctuation">&gt;</span></span></span>\n<span class="token punctuation">{</span>\n    <span class="token keyword">public</span> <span class="token function">ContractTestWithAlba</span><span class="token punctuation">(</span><span class="token class-name">WebAppFixture</span> app<span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n        _host <span class="token operator">=</span> app<span class="token punctuation">.</span>AlbaHost<span class="token punctuation">;</span>\n    <span class="token punctuation">}</span>\n\n    <span class="token keyword">private</span> <span class="token keyword">readonly</span> <span class="token class-name">IAlbaHost</span> _host<span class="token punctuation">;</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ContractTestWithAlba.cs#L23-L32" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_using_xunit_fixture" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><h2 id="collection-fixtures"><a class="header-anchor" href="#collection-fixtures" aria-hidden="true">#</a> Collection Fixtures</h2><p>In the previous section, the <code>WebAppFixture</code> instance will only be shared between all the tests in the one <code>ContractTestWithAlba</code> class. To reuse the <code>IAlbaHost</code> across multiple test fixture classes, you&#39;ll need to use <a href="http://xUnit.Net" target="_blank" rel="noopener noreferrer">xUnit.Net</a>&#39;s <a href="https://xunit.net/docs/shared-context" target="_blank" rel="noopener noreferrer">Collection Fixture</a> concept.</p><p>Still using <code>WebAppFixture</code>, we&#39;ll now need to have a marker collection class like this:</p>',6),r=p('<p><a id="snippet-sample_scenariocollection"></a></p><div class="language-cs"><pre><code><span class="token punctuation">[</span><span class="token attribute"><span class="token class-name">CollectionDefinition</span><span class="token attribute-arguments"><span class="token punctuation">(</span><span class="token string">&quot;scenarios&quot;</span><span class="token punctuation">)</span></span></span><span class="token punctuation">]</span>\n<span class="token keyword">public</span> <span class="token keyword">class</span> <span class="token class-name">ScenarioCollection</span> <span class="token punctuation">:</span> <span class="token type-list"><span class="token class-name">ICollectionFixture<span class="token punctuation">&lt;</span>WebAppFixture<span class="token punctuation">&gt;</span></span></span>\n<span class="token punctuation">{</span>\n    \n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ContractTestWithAlba.cs#L69-L77" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_scenariocollection" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><p>As a convenience, I like to have a base class for all test fixture classes that will be using scenarios like this:</p>',4),k=p('<p><a id="snippet-sample_scenariocontext"></a></p><div class="language-cs"><pre><code><span class="token punctuation">[</span><span class="token attribute"><span class="token class-name">Collection</span><span class="token attribute-arguments"><span class="token punctuation">(</span><span class="token string">&quot;scenarios&quot;</span><span class="token punctuation">)</span></span></span><span class="token punctuation">]</span>\n<span class="token keyword">public</span> <span class="token keyword">abstract</span> <span class="token keyword">class</span> <span class="token class-name">ScenarioContext</span>\n<span class="token punctuation">{</span>\n    <span class="token keyword">protected</span> <span class="token function">ScenarioContext</span><span class="token punctuation">(</span><span class="token class-name">WebAppFixture</span> fixture<span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n        Host <span class="token operator">=</span> fixture<span class="token punctuation">.</span>AlbaHost<span class="token punctuation">;</span>\n    <span class="token punctuation">}</span>\n\n    <span class="token keyword">public</span> <span class="token return-type class-name">IAlbaHost</span> Host <span class="token punctuation">{</span> <span class="token keyword">get</span><span class="token punctuation">;</span> <span class="token punctuation">}</span>\n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ContractTestWithAlba.cs#L79-L92" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_scenariocontext" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p><p>And then inherit from that <code>ScenarioContext</code> base class in actual test fixture classes:</p>',4),d=p('<p><a id="snippet-sample_integration_fixture"></a></p><div class="language-cs"><pre><code><span class="token keyword">public</span> <span class="token keyword">class</span> <span class="token class-name">sample_integration_fixture</span> <span class="token punctuation">:</span> <span class="token type-list"><span class="token class-name">ScenarioContext</span></span>\n<span class="token punctuation">{</span>\n    <span class="token keyword">public</span> <span class="token function">sample_integration_fixture</span><span class="token punctuation">(</span><span class="token class-name">WebAppFixture</span> fixture<span class="token punctuation">)</span> <span class="token punctuation">:</span> <span class="token keyword">base</span><span class="token punctuation">(</span>fixture<span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n    <span class="token punctuation">}</span>\n    \n    <span class="token punctuation">[</span><span class="token attribute"><span class="token class-name">Fact</span></span><span class="token punctuation">]</span>\n    <span class="token keyword">public</span> <span class="token return-type class-name">Task</span> <span class="token function">happy_path</span><span class="token punctuation">(</span><span class="token punctuation">)</span>\n    <span class="token punctuation">{</span>\n        <span class="token keyword">return</span> Host<span class="token punctuation">.</span><span class="token function">Scenario</span><span class="token punctuation">(</span>_ <span class="token operator">=&gt;</span>\n        <span class="token punctuation">{</span>\n            _<span class="token punctuation">.</span>Get<span class="token punctuation">.</span><span class="token function">Url</span><span class="token punctuation">(</span><span class="token string">&quot;/fake/okay&quot;</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n            _<span class="token punctuation">.</span><span class="token function">StatusCodeShouldBeOk</span><span class="token punctuation">(</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n        <span class="token punctuation">}</span><span class="token punctuation">)</span><span class="token punctuation">;</span>\n    <span class="token punctuation">}</span>\n<span class="token punctuation">}</span>\n</code></pre></div><p><sup><a href="https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/ContractTestWithAlba.cs#L94-L113" title="Snippet source file">snippet source</a> | <a href="#snippet-sample_integration_fixture" title="Start of snippet">anchor</a></sup>\x3c!-- endSnippet --\x3e</p>',3);o.render=function(t,p,e,o,_,h){return s(),n("div",null,[c,a(" snippet: sample_should_say_hello_world "),l,a(" snippet: sample_xUnit_Fixture "),i,a(" snippet: sample_using_xUnit_Fixture "),u,a(" snippet: sample_ScenarioCollection "),r,a(" snippet: sample_ScenarioContext "),k,a(" snippet: sample_integration_fixture "),d])};export{e as __pageData,o as default};

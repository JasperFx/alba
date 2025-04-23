using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Alba.Testing
{
    public class using_extensions_with_sync_builder
    {
        private readonly FakeExtension extension1;
        private readonly FakeExtension extension2;
        private readonly FakeExtension extension3;
        private readonly IHostBuilder theBuilder;
        private readonly IAlbaHost theHost;

        public using_extensions_with_sync_builder()
        {
            extension1 = new FakeExtension();
            extension2 = new FakeExtension();
            extension3 = new FakeExtension();

            theBuilder = Host.CreateDefaultBuilder();

            theHost = theBuilder.StartAlba(extension1, extension2, extension3);
        }

        [Fact]
        public void all_extensions_should_be_applied_to_configure()
        {
            extension1.WasConfigured.ShouldBeTrue();
            extension2.WasConfigured.ShouldBeTrue();
            extension3.WasConfigured.ShouldBeTrue();

            theHost.Dispose();
        }

        [Fact]
        public void all_extensions_should_be_started()
        {
            extension1.WasStarted.ShouldBeTrue();
            extension2.WasStarted.ShouldBeTrue();
            extension3.WasStarted.ShouldBeTrue();

            theHost.Dispose();
        }

        [Fact]
        public async Task all_extensions_disposed_async()
        {
            await theHost.DisposeAsync();
            
            extension1.WasAsyncDisposed.ShouldBeTrue();
            extension2.WasAsyncDisposed.ShouldBeTrue();
            extension3.WasAsyncDisposed.ShouldBeTrue();
        }
        
        [Fact]
        public void all_extensions_disposed()
        {
            theHost.Dispose();
            
            extension1.WasDisposed.ShouldBeTrue();
            extension2.WasDisposed.ShouldBeTrue();
            extension3.WasDisposed.ShouldBeTrue();
        }
    }
    
    public class using_extensions_with_async_builder 
    {
        private readonly FakeExtension extension1;
        private readonly FakeExtension extension2;
        private readonly FakeExtension extension3;
        private readonly IHostBuilder theBuilder;
        private readonly IAlbaHost theHost;

        public using_extensions_with_async_builder()
        {
            extension1 = new FakeExtension();
            extension2 = new FakeExtension();
            extension3 = new FakeExtension();

            theBuilder = Host.CreateDefaultBuilder();

            theHost = theBuilder.StartAlbaAsync(extension1, extension2, extension3).GetAwaiter().GetResult();
        }

        [Fact]
        public void all_extensions_should_be_applied_to_configure()
        {
            extension1.WasConfigured.ShouldBeTrue();
            extension2.WasConfigured.ShouldBeTrue();
            extension3.WasConfigured.ShouldBeTrue();

            theHost.Dispose();
        }

        [Fact]
        public void all_extensions_should_be_started()
        {
            extension1.WasStarted.ShouldBeTrue();
            extension2.WasStarted.ShouldBeTrue();
            extension3.WasStarted.ShouldBeTrue();

            theHost.Dispose();
        }

        [Fact]
        public async Task all_extensions_disposed_async()
        {
            await theHost.DisposeAsync();
            
            extension1.WasAsyncDisposed.ShouldBeTrue();
            extension2.WasAsyncDisposed.ShouldBeTrue();
            extension3.WasAsyncDisposed.ShouldBeTrue();
        }
        
        [Fact]
        public void all_extensions_disposed()
        {
            theHost.Dispose();
            
            extension1.WasDisposed.ShouldBeTrue();
            extension2.WasDisposed.ShouldBeTrue();
            extension3.WasDisposed.ShouldBeTrue();
        }
    }


    public class FakeExtension : IAlbaExtension
    {
        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }

        public ValueTask DisposeAsync()
        {
            WasAsyncDisposed = true;
            return ValueTask.CompletedTask;
        }

        public bool WasAsyncDisposed { get; set; }

        public Task Start(IAlbaHost host)
        {
            WasStarted = true;
            return Task.CompletedTask;
        }

        public bool WasStarted { get; set; }

        public IHostBuilder Configure(IHostBuilder builder)
        {
            WasConfigured = true;
            return builder;
        }

        public bool WasConfigured { get; set; }
    }
}
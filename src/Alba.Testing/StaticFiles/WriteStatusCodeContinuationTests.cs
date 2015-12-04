using System.Collections.Generic;
using Alba.StaticFiles;
using Shouldly;
using Xunit;

namespace Alba.Testing.StaticFiles
{
    public class WriteStatusCodeContinuationTests
    {
        [Fact]
        public void just_writes_status_code()
        {
            var env = new Dictionary<string, object>();
            new WriteStatusCodeContinuation(env, 501, "don't like").Write(env);

            env.StatusCode().ShouldBe(501);
            env.StatusDescription().ShouldBe("don't like");
        } 
    }
}
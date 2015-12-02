using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Alba.Testing
{
    public class FakeTests
    {
        [Fact]
        public void good()
        {
            1.ShouldBe(1);
        }
    }
}

using Alba.Assertions;
using System.Collections;

namespace Alba.Testing.Assertions
{
    public class StatusCodeSuccessAssertionTests
    {
        [Theory]
        [ClassData(typeof(SuccessStatusCodes))]
        public void HappyPath(int statusCode)
        {
            var assertion = new StatusCodeSuccessAssertion();

            AssertionRunner.Run(assertion, _ => _.StatusCode(statusCode))
                .AssertAll();
        }

        [Theory]
        [ClassData(typeof(FailureStatusCodes))]
        public void SadPath(int statusCode)
        {
            var assertion = new StatusCodeSuccessAssertion();

            AssertionRunner.Run(assertion, _ => _.StatusCode(statusCode))
                .SingleMessageShouldBe($"Expected a status code between 200 and 299, but was {statusCode}");
        }
    }

   
        
}

public class SuccessStatusCodes : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var code in Enumerable.Range(200, 99))
        {
            yield return new object[] { code };
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
}

public class FailureStatusCodes : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        foreach (var code in Enumerable.Range(100, 99))
        {
            yield return new object[] { code };
        }
        foreach (var code in Enumerable.Range(300, 200))
        {
            yield return new object[] { code };
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}
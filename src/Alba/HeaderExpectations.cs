namespace Alba
{
    public class HeaderExpectations
    {
        private readonly Scenario _parent;
        private readonly string _headerKey;

        public HeaderExpectations(Scenario parent, string headerKey)
        {
            _parent = parent;
            _headerKey = headerKey;
        }

        public HeaderExpectations SingleValueShouldEqual(string expected)
        {
            _parent.AssertThat(new HeaderValueAssertion(_headerKey, expected));
            return this;
        }

        public HeaderExpectations ShouldHaveOneNonNullValue()
        {
            _parent.AssertThat(new HasSingleHeaderValueAssertion(_headerKey));
            return this;
        }

        public void ShouldNotBeWritten()
        {
            _parent.AssertThat(new NoHeaderValueAssertion(_headerKey));
        }
    }
}
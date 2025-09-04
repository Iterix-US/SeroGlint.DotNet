namespace SeroGlint.DotNet.Tests.TestClasses.NamedPipes.TestObjects
{
    public class TestObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "SomeName";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

using FluentAssertions;
using StarviaBackend.Modules.Ai.Application.PostGeneration;

namespace StarviaBackend.Modules.Ai.Tests.Unit.PostGeneration;

public sealed class GeneratedPostParserTests
{
    [Fact]
    public void Parse_ReadsABareJsonPost()
    {
        // Act
        var post = GeneratedPostParser.Parse("""{"title": "Fresh coffee", "body": "Try our new blend."}""");

        // Assert
        post.Title.Should().Be("Fresh coffee");
        post.Body.Should().Be("Try our new blend.");
    }

    [Fact]
    public void Parse_TurnsEscapedLineBreaksIntoRealOnes()
    {
        // Act
        var post = GeneratedPostParser.Parse("""{"title": "Hello", "body": "First line.\n\nSecond line. #coffee"}""");

        // Assert
        post.Body.Should().Be("First line.\n\nSecond line. #coffee");
    }

    [Fact]
    public void Parse_KeepsNonAsciiTextAndEmojis()
    {
        // Act
        var post = GeneratedPostParser.Parse("""{"title": "Świeża kawa ☕", "body": "Zażółć gęślą jaźń 🎉"}""");

        // Assert
        post.Title.Should().Be("Świeża kawa ☕");
        post.Body.Should().Be("Zażółć gęślą jaźń 🎉");
    }

    [Fact]
    public void Parse_TrimsTheTitleAndTheBody()
    {
        // Act
        var post = GeneratedPostParser.Parse("""{"title": "  Hello  ", "body": "\n Body \n"}""");

        // Assert
        post.Title.Should().Be("Hello");
        post.Body.Should().Be("Body");
    }

    [Fact]
    public void Parse_AcceptsPropertyNamesInAnyCase()
    {
        // Act
        var post = GeneratedPostParser.Parse("""{"Title": "Hello", "BODY": "Body"}""");

        // Assert
        post.Title.Should().Be("Hello");
        post.Body.Should().Be("Body");
    }

    [Fact]
    public void Parse_IgnoresUnknownProperties()
    {
        // Act
        var post = GeneratedPostParser.Parse("""{"title": "Hello", "body": "Body", "hashtags": ["a", "b"]}""");

        // Assert
        post.Title.Should().Be("Hello");
        post.Body.Should().Be("Body");
    }

    [Fact]
    public void Parse_AcceptsAPostWrappedInAMarkdownCodeBlock()
    {
        // Arrange
        var content = "```json\n{\"title\": \"Hello\", \"body\": \"Body\"}\n```";

        // Act
        var post = GeneratedPostParser.Parse(content);

        // Assert
        post.Title.Should().Be("Hello");
        post.Body.Should().Be("Body");
    }

    [Fact]
    public void Parse_AcceptsAPostSurroundedByRemarks()
    {
        // Arrange
        var content = "Sure! Here is your post:\n{\"title\": \"Hello\", \"body\": \"Body\"}\nLet me know if you want changes.";

        // Act
        var post = GeneratedPostParser.Parse(content);

        // Assert
        post.Title.Should().Be("Hello");
        post.Body.Should().Be("Body");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("I am sorry, I cannot help with that.")]
    [InlineData("}{")]
    [InlineData("{}")]
    [InlineData("""{"title": "Only a title"}""")]
    [InlineData("""{"body": "Only a body"}""")]
    [InlineData("""{"title": "", "body": "Body"}""")]
    [InlineData("""{"title": "Title", "body": "   "}""")]
    [InlineData("""{"title": null, "body": "Body"}""")]
    [InlineData("""{"title": 42, "body": "Body"}""")]
    [InlineData("""{"title": "Title", "body": ["a", "b"]}""")]
    [InlineData("""{"title": "Cut off""")]
    [InlineData("""{"title": "Title", "body": "Body"} {"title": "Another", "body": "Post"}""")]
    [InlineData("""[{"title": "Title", "body": "Body"}, {"title": "Another", "body": "Post"}]""")]
    public void Parse_WhenThereIsNoUsablePost_Throws(string content)
    {
        // Act
        var act = () => GeneratedPostParser.Parse(content);

        // Assert
        act.Should().Throw<InvalidGeneratedPostException>().Which.Code.Should().Be("generated_post_invalid");
    }
}

using FluentAssertions;
 
namespace RedRover.Puzzle.Console.Tests.Unit;

public class NpcsvParserTests
{
    [Fact]
    public void Parse_ValidInput_ReturnsExpectedTree()
    {
        // Arrange
        const string input = "(id, name, email, type(id, name, customFields(c1, c2, c3)), externalId)";
        var expectedTree = BuildExpectedTree();
        
        // Act
        var nodeTree = NpcsvParser.Parse(input);
        
        // Assert
        nodeTree.Should().BeEquivalentTo(expectedTree);
    }
    
    [Fact]
    public void Parse_PropertiesWithNumbers_ReturnsExpectedTree()
    {
        // Arrange
        const string input = "(id1, name2, email3)";
        
        // Act
        var result = NpcsvParser.Parse(input);
    
        // Assert
        result.Children.Should().HaveCount(3);
        result.Children.Select(c => c.Name).Should().BeEquivalentTo("id1", "name2", "email3");
    }
    
    [Fact]
    public void Parse_SingleProperty_ReturnsSingleChild()
    {
        // Arrange
        const string input = "(id)";
    
        // Act
        var result = NpcsvParser.Parse(input);
    
        // Assert
        result.Children.Should().ContainSingle()
            .Which.Name.Should().Be("id");
    }
    
    [Theory]
    [InlineData("(id, name, email, type(id, name, customFields(c1, c2, c3))")]
    [InlineData("(id, name, email, type(id, name, customFields(c1, c2, c3)")]
    [InlineData("(id, name, email, type(id, name, customFi")]
    [InlineData("(id, name, email, type")]
    [InlineData("(id, name, email, type(id, name,")]
    public void Parse_InvalidEnd_ThrowsSyntaxException(string input)
    {
        // Act
        var act = () => NpcsvParser.Parse(input);
        
        // Assert
        act.Should().Throw<NpcsvSyntaxException>()
            .WithMessage("Unexpected end of input.");
    }
    
    [Theory]
    [InlineData("(id, name, email, type (id, name, customFields(c1, c2, c3))")]
    [InlineData("(id, name, email type(id, name, customFields(c1, c2, c3)")]
    public void Parse_InvalidWhiteSpace_ThrowsSyntaxException(string input)
    {
        // Act
        var act = () => NpcsvParser.Parse(input);
        
        // Assert
        act.Should().Throw<NpcsvSyntaxException>()
            .WithMessage("Unexpected white space at column*");
    }
    
    [Theory]
    [InlineData("(id, name, email, type(id,, name, customFields(c1, c2, c3)), externalId)")]
    [InlineData("(,id, name, email, type(id, name, customFields(c1, c2, c3)), externalId)")]
    [InlineData("(id, name, email, type(id, name, customFields(c1, c2, c3)),)")]
    [InlineData("(id, ,name, email, type(id, name, customFields(c1, c2, c3)), externalId)")]
    [InlineData("(id, name, email,,,, type(id, name, customFields(c1, c2, c3)), externalId)")]
    public void Parse_InvalidCommas_ThrowsSyntaxException(string input)
    {
        // Act
        var act = () => NpcsvParser.Parse(input);
        
        // Assert
        act.Should().Throw<NpcsvSyntaxException>()
            .WithMessage("Unexpected token*");
    }
    
    [Theory]
    [InlineData("(#id, name, email, type(id, name, customFields(c1, c2, c3)), externalId)")]
    [InlineData("(id, n@me, email, type(id, name, customFields(c1, c2, c3)), externalId)")]
    [InlineData("(id, name, ema!l, type(id, name, customFields(c1, c2, c3)), externalId)")]
    [InlineData("(id, name?, email, type(id, name, customFields(c1, c2, c3)), externalId)")]
    [InlineData("(id, name, email, type(id, name, custom!Fields(c1, c2, c3)), externalId)")]
    [InlineData("(id, name, email, ~type(id, name, customFields(c1, c2, c3)), externalId)")]
    public void Parse_InvalidCharacters_ThrowsSyntaxException(string input)
    {
        // Act
        var act = () => NpcsvParser.Parse(input);
        
        // Assert
        act.Should().Throw<NpcsvSyntaxException>()
            .WithMessage("Invalid token*");
    }
    
    [Theory]
    [InlineData("(id, type()())")]
    [InlineData("(id,( type)")]
    public void Parse_InvalidParentheses_ThrowsSyntaxException(string input)
    {
        var act = () => NpcsvParser.Parse(input);
    
        act.Should().Throw<NpcsvSyntaxException>();
    }
    
    [Theory]
    [InlineData("(id)(name)")]
    [InlineData("(id),")]
    public void Parse_CharactersAfterRootNodeEnd_IgnoresAdditionalInput(string input)
    {
        var act = () => NpcsvParser.Parse(input);
    
        // Act
        var result = NpcsvParser.Parse(input);
    
        // Assert
        result.Children.Should().HaveCount(1);
    }
    
    [Fact]
    public void Parse_InvalidStart_ThrowsSyntaxException()
    {
        // Arrange
        const string input = "id, name, email, type(id, name, customFields(c1, c2, c3)), externalId)";
        // Act
        var act = () => NpcsvParser.Parse(input);
        
        // Assert
        act.Should().Throw<NpcsvSyntaxException>()
            .WithMessage("Unexpected start of input.");
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_NullOrEmptyInput_ThrowsArgumentException(string? input)
    {
        // Act
        var act = () => NpcsvParser.Parse(input);
    
        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("input");
    }
    
    [Fact]
    public void Parse_EmptyParentheses_ReturnsEmptyRoot()
    {
        // Arrange
        const string input = "()";
    
        // Act
        var result = NpcsvParser.Parse(input);
    
        // Assert
        result.Should().NotBeNull();
        result.Children.Should().BeEmpty();
    }
    
    private static NpcsvNode BuildExpectedTree()
    {
        var root = NpcsvNode.CreateRoot();
        root.AddChild("id");
        root.AddChild("name");
        root.AddChild("email");
    
        var type = root.AddChild("type");
        type.AddChild("id");
        type.AddChild("name");
    
        var customFields = type.AddChild("customFields");
        customFields.AddChild("c1");
        customFields.AddChild("c2");
        customFields.AddChild("c3");
    
        root.AddChild("externalId");
        
        return root;
    }
}
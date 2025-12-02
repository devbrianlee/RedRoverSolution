using FluentAssertions;

namespace RedRover.Puzzle.Console.Tests.Unit;

public class NpcsvRendererTests
{
    [Fact]
    public void RenderUnsorted_MultiLevelNode_ReturnsExpectedOutput()
    {
        // Arrange
        var tree = BuildMultiLevelTree();
        const string expectedOutput = """
- id
- name
- email
- type
  - id
  - name
  - customFields
    - c1
    - c2
    - c3
- externalId
""";
        // Act
        var unsortedRenderedList = NpcsvRenderer.Render(tree, false);
        
        // Assert
        unsortedRenderedList.Should().NotBeNull().And.Be(expectedOutput);
    }
    
    [Fact]
    public void RenderSorted_MultiLevelNode_ReturnsExpectedOutput()
    {
        // Arrange
        var tree = BuildMultiLevelTree();
        const string expectedOutput = """
  - email
  - externalId
  - id
  - name
  - type
    - customFields
      - c1
      - c2
      - c3
    - id
    - name
  """;
        
        // Act
        var sortedRenderedList = NpcsvRenderer.Render(tree, true);
        
        // Assert
        sortedRenderedList.Should().NotBeNull().And.Be(expectedOutput);
    }
    
    private static NpcsvNode BuildMultiLevelTree()
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
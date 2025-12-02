using System.Text;

namespace RedRover.Puzzle.Console;

// This class could be one of many renderers implementing an interface
// Perhaps one or more renderers could be registered with DI or configured to be strategically injected/used based on context
public static class NpcsvRenderer
{
    private const int IndentSize = 2;
        
    public static string Render(NpcsvNode root, bool sort)
    {
        var stringBuilder = new StringBuilder();
        RenderNode(root, stringBuilder, 0, sort);
        return stringBuilder.ToString().TrimEnd(); // Remove trailing newline
    }
    
    private static string RenderNode(NpcsvNode node, StringBuilder sb, int depth, bool isSorted)
    {
        var children = isSorted ? node.Children.OrderBy(x => x.Name).ToList() : node.Children.ToList();
        
        foreach (var child in children)
        {
            sb.Append(new string(' ', depth * IndentSize));
            
            sb.AppendLine($"- {child.Name}");
            
            if (child.Children.Count > 0)
            {
                RenderNode(child, sb, depth + 1, isSorted);
            }
        }
        
        return string.Empty;
    }
}
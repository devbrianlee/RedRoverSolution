namespace RedRover.Puzzle.Console;

public class NpcsvNode
{
    private readonly List<NpcsvNode> children = [];
    public IReadOnlyList<NpcsvNode> Children => children;
    public bool IsRoot { get; }
    public string Name { get; }  
    
    private NpcsvNode(string name, bool isRoot)
    {
        Name = name;
        IsRoot = isRoot;
    }
    
    public static NpcsvNode CreateRoot()
    {
        return new NpcsvNode("root", true);
    }   
    
    public NpcsvNode AddChild(string name)
    {
        var childNode = new NpcsvNode(name, false);
        
        children.Add(childNode);

        return childNode;
    }
}
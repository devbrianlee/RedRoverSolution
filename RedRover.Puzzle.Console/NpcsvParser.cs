using System.Text.RegularExpressions;

namespace RedRover.Puzzle.Console;

// Hypothetical future-state considerations:
    // Currently allowing multiple properties of the same name in the same node, as well as empty nodes
    // Line breaks are not supported
    // Nested nodes and root nodes can be empty
    // Additional tokens and nodes (after the root is closed) are currently ignored, as once the root node is closed, node creation is stopped
    // Determine current and future usage of both parser and renderer to decide whether to support DI or continue to use static methods like JsonConvert.SerializeObject (Newtonsoft's implementation of Json parsing) 
public partial class NpcsvParser(string input) : IDisposable
{
    private const char NestStartDelimiter = '(';
    private const char NestEndDelimiter = ')';
    private const char PropertyDelimiter = ',';
    private const char WhiteSpace = ' ';
    
    private int column = 0;
    private int depth = 0;
    
    private bool isPropertyToken;
    private bool isPropertyDelimiter;
    private bool isNestStart;
    private bool isNestEnd;
    private bool isRootEnd;
    
    // Null to start
    private char prevToken = '\0';
    
    private readonly CharEnumerator rawInput = input.Trim().GetEnumerator();
    
    private readonly Regex propertyTokenTest = PropertyTokenRegex();
    
    public Task<bool> IsValidAsync(string input)
    {
        return Task.FromResult(true);
    }

    private void ResetTokenState()
    {
        isPropertyToken = false;
        isPropertyDelimiter = false;
        isNestStart = false;
        isNestEnd = false;
    }
    
    private bool NextToken()
    {
        if (rawInput.MoveNext())
        {
            column++;
            ResetTokenState();
            
            if(prevToken == '\0' && rawInput.Current != NestStartDelimiter)
                throw new NpcsvSyntaxException("Unexpected start of input.");
            
            if(isRootEnd)
                throw new NpcsvSyntaxException($"Unexpected token {rawInput.Current} after root node end.");
            
            switch (rawInput.Current)
            {
                case WhiteSpace:
                    // Let's keep track of the column but avoid returning or validating against whitespace
                    ValidateWhiteSpace();
                    column++;
                    NextToken();
                    break;
                case NestStartDelimiter:
                    ValidateNestStartDelimiter();
                    isNestStart = true;
                    depth++;
                    break;
                case NestEndDelimiter:
                    ValidateNestEndDelimiter();
                    isNestEnd = true;
                    depth--; 
                    if(depth == 0) isRootEnd = true;
                    break;
                case PropertyDelimiter:
                    ValidatePropertyDelimiter();
                    isPropertyDelimiter = true;
                    break;
                default:
                    ValidateToken();
                    isPropertyToken = true;
                    break;
            }
            
            prevToken = rawInput.Current;
            
            return true;
        }
        else if(prevToken != NestEndDelimiter || depth > 0)
        {
            throw new NpcsvSyntaxException("Unexpected end of input.");
        }
        
        return false;
    }

    // Valid only after opening parenthesis, commas, or another space
    // We'll be strict about whitespace between properties and opening parenthesis, so we don't have to look ahead to validate
    private void ValidateWhiteSpace()
    {
        if(prevToken != '(' && prevToken != ',' && prevToken != WhiteSpace) 
            throw new NpcsvSyntaxException($"Unexpected white space at column {column} after '{prevToken}'.");
    }
    
    // Valid only at the beginning of input or after a property token or space
    private void ValidateNestStartDelimiter()
    {
        if(!propertyTokenTest.IsMatch(prevToken.ToString()) && prevToken != '\0') 
            throw new NpcsvSyntaxException($"Unexpected token '{rawInput.Current}' at column {column} after '{prevToken}'.");
    }
    
    // Valid only after a property token or another nest-end (or opening parenthesis, if we have an empty child or root node)
    private void ValidateNestEndDelimiter()
    {
        if(!propertyTokenTest.IsMatch(prevToken.ToString()) && prevToken != NestEndDelimiter && prevToken != NestStartDelimiter) 
            throw new NpcsvSyntaxException($"Unexpected token '{rawInput.Current}' at column {column} after '{prevToken}'.");
    }
    
    // Valid only after a property token or nest-end
    private void ValidatePropertyDelimiter()
    {
        if(!propertyTokenTest.IsMatch(prevToken.ToString()) && prevToken != NestEndDelimiter) 
            throw new NpcsvSyntaxException($"Unexpected token '{rawInput.Current}' at column {column} after '{prevToken}'.");
    }
    
    // Valid only after another property token, nest-start, or comma (property delimiter)
    // Valid only if composed of alphanumeric characters
    private void ValidateToken()
    {
        if(!propertyTokenTest.IsMatch(prevToken.ToString())
           && prevToken != NestStartDelimiter
           && prevToken != PropertyDelimiter) 
            throw new NpcsvSyntaxException($"Unexpected token '{rawInput.Current}' at column {column} after '{prevToken}'.");
        
        if(!propertyTokenTest.IsMatch(rawInput.Current.ToString()))
           throw new NpcsvSyntaxException($"Invalid token '{rawInput.Current}' at column {column}.");
    }
    
    public static NpcsvNode Parse(string input)
    {
        if(string.IsNullOrWhiteSpace(input)) throw new ArgumentException("Input cannot be null or whitespace.", nameof(input));
        
        var root = NpcsvNode.CreateRoot();

        using var parser = new NpcsvParser(input);
        
        parser.ParseLevel(root);

        return root;
    }

    private void ParseLevel(NpcsvNode parentNode)
    {
        var propertyName = string.Empty;
    
        while (NextToken())
        {
            if (isPropertyToken)
            {
                propertyName += rawInput.Current;
            }
            else if (isNestStart)
            {
                if (depth == 1) continue;
                
                var childNode = parentNode.AddChild(propertyName);
                propertyName = string.Empty;
                ParseLevel(childNode);
            }
            else if (isNestEnd)
            {
                if (string.IsNullOrEmpty(propertyName)) return;
                
                parentNode.AddChild(propertyName);
                return;
            }
            else if (isPropertyDelimiter)
            {
                if (string.IsNullOrEmpty(propertyName)) continue;
                
                parentNode.AddChild(propertyName);
                propertyName = string.Empty;
            }
        }
    
        // Add last property
        if (!string.IsNullOrEmpty(propertyName))
        {
            parentNode.AddChild(propertyName);
        }
    }
    
    public void Dispose()
    {
        rawInput.Dispose();
        
        // Prevents any inheriting classes from having to implement a finalizer.
        GC.SuppressFinalize(this);
    }

    [GeneratedRegex(@"^[\w\d]+$")]
    private static partial Regex PropertyTokenRegex();
}
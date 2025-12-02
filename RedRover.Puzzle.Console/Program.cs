using RedRover.Puzzle.Console;

Console.WriteLine("NPCSV (Nested Parenthetical Comma Separated Values) Parser - Enter nested CSV input or 'exit' to quit");
Console.WriteLine("Commands: enter 'sorted' to toggle sorted output");
Console.WriteLine("=================================================\n");

var sortedMode = false;

while (true)
{
    Console.Write($"Parse and Render{(sortedMode ? " [sorted]" : "")}> ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (input.Trim().Equals("sorted", StringComparison.OrdinalIgnoreCase))
    {
        sortedMode = !sortedMode;
        Console.WriteLine($"Sorted mode: {(sortedMode ? "ON" : "OFF")}\n");
        continue;
    }

    try
    {
        var nodeTree = NpcsvParser.Parse(input);
        var output = NpcsvRenderer.Render(nodeTree, sortedMode);
        
        Console.WriteLine("\nResult:");
        Console.WriteLine(output);
    }
    catch (NpcsvSyntaxException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nSyntax Error: {ex.Message}");
        Console.ResetColor();
        Console.WriteLine();
    }
    catch (ArgumentException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nInvalid Input: {ex.Message}");
        Console.ResetColor();
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nUnexpected Error: {ex.Message}");
        Console.ResetColor();
        Console.WriteLine();
    }
}
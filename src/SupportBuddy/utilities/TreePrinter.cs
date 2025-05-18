using System;
using System.Collections.Generic;
using System.Linq;

public static class TreePrinter
{
    private record TreeToken(string Text, ConsoleColor? Color);
    private record TreeLine(int Level, List<TreeToken> Tokens);

    private static readonly List<TreeLine> _lines = new();
    private static int _currentLevel;
    private static int _currentLineLength;

    public static void Indent() => _currentLevel++;
    public static void Unindent() => _currentLevel = Math.Max(0, _currentLevel - 1);

    public static void Print(string text, ConsoleColor? color = null)
    {
        Console.WriteLine(); // End prior line

        var tokens = new List<TreeToken> { new(text, color) };
        var line = new TreeLine(_currentLevel, tokens);
        _lines.Add(line);

        var siblingGuides = GetSiblingGuides(_lines.Count - 1);
        int prefixWidth = GetPrefixWidth(siblingGuides);
        _currentLineLength = prefixWidth + text.Length;

        RedrawAll();
    }


    public static void Append(string word, ConsoleColor? color = null)
    {
        if (_lines.Count == 0)
        {
            Print(word, color);
            return;
        }

        var last = _lines[^1];
        var siblingGuides = GetSiblingGuides(_lines.Count - 1);
        var maxWidth = Console.WindowWidth;

        int prefixWidth = GetPrefixWidth(siblingGuides);
        int fullLineWidth = _currentLineLength + word.Length;

        // If it will wrap
        if (fullLineWidth > maxWidth)
        {
            Console.WriteLine();

            bool isLast = IsLastSibling(_lines.Count - 1);

            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < siblingGuides.Length - 1; i++)
                Console.Write(siblingGuides[i] ? "│   " : "    ");
            if (siblingGuides.Length > 0)
                Console.Write(isLast ? "    " : "│   ");
            Console.ResetColor();

            word = word.TrimStart();
            _currentLineLength = prefixWidth + word.Length;
        }
        else
        {
            _currentLineLength = fullLineWidth;
        }

        last.Tokens.Add(new TreeToken(word, color));

        if (color.HasValue)
            Console.ForegroundColor = color.Value;

        Console.Write(word);
        Console.ResetColor();
    }



    private static void RedrawAll()
    {
        Console.Clear();

        for (int i = 0; i < _lines.Count; i++)
        {
            var line = _lines[i];
            var siblingGuides = GetSiblingGuides(i);
            var isLast = IsLastSibling(i);
            var wrapWidth = Console.WindowWidth - GetPrefixWidth(siblingGuides);
            var wrappedLines = WrapTokens(line.Tokens, wrapWidth);

            foreach (var (tokens, j) in wrappedLines.Select((t, j) => (t, j)))
            {
                Console.ForegroundColor = ConsoleColor.White;

                for (int level = 0; level < siblingGuides.Length - 1; level++)
                    Console.Write(siblingGuides[level] ? "│   " : "    ");
                if (siblingGuides.Length > 0)
                    Console.Write(j == 0 ? (isLast ? "└── " : "├── ") : (isLast ? "    " : "│   "));

                Console.ResetColor();

                foreach (var (Text, Color) in tokens)
                {
                    var isFirstToken = tokens[0].Text == Text && tokens[0].Color == Color;
                    var content = isFirstToken ? Text.TrimStart() : Text;

                    if (Color.HasValue) Console.ForegroundColor = Color.Value;
                    else Console.ResetColor();

                    Console.Write(content);
                }

                Console.ResetColor();

                if (i < _lines.Count - 1 || j < wrappedLines.Count - 1)
                    Console.WriteLine();
            }
        }
    }

    private static List<List<TreeToken>> WrapTokens(List<TreeToken> tokens, int maxWidth)
    {
        var result = new List<List<TreeToken>>();
        var current = new List<TreeToken>();
        var lineLength = 0;

        foreach (var token in tokens)
        {
            var len = token.Text.Length;
            if (lineLength + len > maxWidth && current.Count > 0)
            {
                result.Add(current);
                current = new List<TreeToken>();
                lineLength = 0;
            }

            current.Add(token);
            lineLength += len;
        }

        if (current.Count > 0)
            result.Add(current);

        return result;
    }

    private static int GetPrefixWidth(bool[] guides) =>
        4 * (guides.Length == 0 ? 0 : guides.Length);

    private static bool[] GetSiblingGuides(int index)
    {
        int level = _lines[index].Level;
        var guides = new bool[level];

        for (int l = 0; l < level; l++)
        {
            for (int i = index + 1; i < _lines.Count; i++)
            {
                var next = _lines[i];
                if (next.Level < l + 1) break;
                if (next.Level == l + 1)
                {
                    guides[l] = true;
                    break;
                }
            }
        }

        return guides;
    }

    private static bool IsLastSibling(int index)
    {
        int level = _lines[index].Level;
        for (int i = index + 1; i < _lines.Count; i++)
        {
            if (_lines[i].Level == level) return false;
            if (_lines[i].Level < level) break;
        }
        return true;
    }

    public static void Reset()
    {
        _lines.Clear();
        _currentLevel = 0;
        Console.Clear();
    }
}

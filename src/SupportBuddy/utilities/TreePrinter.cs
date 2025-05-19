using System;
using System.Collections.Generic;

#nullable enable

public static class TreePrinter
{
    public record TreeToken(string Text, ConsoleColor? Color);

    public sealed class TreeNode
    {
        public int Level { get; }
        public List<TreeToken> Tokens { get; } = new();
        public List<TreeNode> Children { get; } = new();

        public TreeNode(int level, string text, ConsoleColor? color)
        {
            Level = level;
            Tokens.Add(new TreeToken(text, color));
        }
    }

    private static TreeNode? _root;
    private static TreeNode? _lastAppendedNode;
    private static int _lastLineLength;
    private static List<bool> _lastGuides = new();
    private static TreeNode? _finalLeafNode;

    private static int _streamingVersion = 0;
    private static int _lastStreamVersion = -1;

    public static void Reset()
    {
        _root = null;
        _lastAppendedNode = null;
        _lastLineLength = 0;
        _lastGuides.Clear();
        _streamingVersion++;
        Console.Clear();
    }

    public static TreeHandle CreateRoot(string label, ConsoleColor? color = null)
    {
        if (_root is not null)
            Reset();

        _root = new TreeNode(0, label, color);
        RedrawAll();
        return new TreeHandle(_root);
    }

    public static TreeHandle CreateSubtree(string label, ConsoleColor? color = null)
    {
        if (_root is null)
            throw new InvalidOperationException("Must call TreePrinter.CreateRoot(...) first.");

        var child = new TreeNode(_root.Level + 1, label, color);
        _root.Children.Add(child);
        RedrawAll();
        return new TreeHandle(child);
    }

    private static void RedrawAll()
    {
        _streamingVersion++;
        Console.Clear();
        _lastAppendedNode = null;
        _lastLineLength = 0;
        _lastGuides.Clear();

        if (_root != null)
        {
            _finalLeafNode = GetDeepestRightmost(_root);
            PrintNode(_root, new List<bool>(), isLast: true);
        }
    }

    private static TreeNode GetDeepestRightmost(TreeNode node)
    {
        while (node.Children.Count > 0)
            node = node.Children[^1];
        return node;
    }

    private static void PrintNode(TreeNode node, List<bool> guides, bool isLast)
    {
        int prefixWidth = 4 * (guides.Count == 0 ? 0 : guides.Count);
        int availableWidth = Console.WindowWidth - prefixWidth;

        var tokens = node.Tokens;
        var line = new List<TreeToken>();
        int lineLen = 0;

        void PrintPrefix(bool isFirstLine)
        {
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < guides.Count - 1; i++)
                Console.Write(guides[i] ? "│   " : "    ");
            if (guides.Count > 0)
                Console.Write(isFirstLine ? (isLast ? "└── " : "├── ") : (isLast ? "    " : "│   "));
            Console.ResetColor();
        }

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            var text = token.Text;
            var color = token.Color;
            int tokenLen = text.Length;

            if (lineLen + tokenLen > availableWidth && line.Count > 0)
            {
                PrintPrefix(isFirstLine: line == tokens.GetRange(0, line.Count));
                foreach (var t in line)
                {
                    if (t.Color.HasValue) Console.ForegroundColor = t.Color.Value;
                    Console.Write(t.Text);
                    Console.ResetColor();
                }
                Console.WriteLine();

                line.Clear();
                lineLen = 0;
            }

            line.Add(token);
            lineLen += tokenLen;
        }

        if (line.Count > 0)
        {
            PrintPrefix(isFirstLine: tokens.Count == line.Count);
            foreach (var t in line)
            {
                if (t.Color.HasValue) Console.ForegroundColor = t.Color.Value;
                Console.Write(t.Text);
                Console.ResetColor();
            }
            if (node != _finalLeafNode)
                Console.WriteLine();
        }

        // Update streaming context
        _lastAppendedNode = node;
        _lastLineLength = prefixWidth + GetLineTextLength(tokens);
        _lastGuides = new List<bool>(guides);
        _lastStreamVersion = _streamingVersion;

        // Recurse
        for (int i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var childIsLast = i == node.Children.Count - 1;
            var childGuides = new List<bool>(guides) { !childIsLast };
            PrintNode(child, childGuides, childIsLast);
        }
    }

    private static int GetLineTextLength(List<TreeToken> tokens)
    {
        int len = 0;
        foreach (var token in tokens)
            len += token.Text.Length;
        return len;
    }

    private static void StreamAppend(TreeNode node, string text, ConsoleColor? color)
    {
        if (node != _lastAppendedNode || _lastStreamVersion != _streamingVersion)
        {
            node.Tokens.Add(new TreeToken(text, color));
            RedrawAll();
            return;
        }

        int maxWidth = Console.WindowWidth;
        if (_lastLineLength + text.Length > maxWidth)
        {
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < _lastGuides.Count; i++)
                Console.Write("    ");
            Console.ResetColor();

            text = text.TrimStart();
            _lastLineLength = 4 * _lastGuides.Count + text.Length;
        }
        else
        {
            _lastLineLength += text.Length;
        }

        node.Tokens.Add(new TreeToken(text, color));

        if (color.HasValue) Console.ForegroundColor = color.Value;
        Console.Write(text);
        Console.ResetColor();
    }

    public sealed class TreeHandle
    {
        private readonly TreeNode _node;

        public TreeHandle(TreeNode node)
        {
            _node = node;
        }

        public void Print(string text, ConsoleColor? color = null)
        {
            _node.Tokens.Add(new TreeToken(text, color));
            RedrawAll();
        }

        public void Append(string word, ConsoleColor? color = null)
        {
            StreamAppend(_node, word, color);
        }

        public TreeHandle CreateSubtree(string text, ConsoleColor? color = null)
        {
            var child = new TreeNode(_node.Level + 1, text, color);
            _node.Children.Add(child);
            RedrawAll();
            return new TreeHandle(child);
        }
    }
}

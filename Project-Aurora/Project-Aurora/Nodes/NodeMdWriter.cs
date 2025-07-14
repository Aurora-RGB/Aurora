using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AuroraRgb.Profiles.Desktop;

namespace AuroraRgb.Nodes;

public class NodeMdWriter
{
    public static void Write()
    {
        ClearMdFiles();
        var propertyLookups = NodePropertyLookups.PropertyMap[typeof(DesktopState)];
        var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "NodePropertyLookups");
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        GenerateHierarchicalMarkdown(propertyLookups, outputDir);
    }

    private static void ClearMdFiles()
    {
        var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "NodePropertyLookups");
        if (!Directory.Exists(outputDir)) return;
        foreach (var file in Directory.GetFiles(outputDir, "*.md"))
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // Ignore errors, just skip files that can't be deleted
            }
        }
    }

    private static void GenerateHierarchicalMarkdown(List<PropertyLookup> allProps, string outputDir)
    {
        // Group by top-level path
        var groups = allProps
            .GroupBy(p => p.GsiPath.Split('/')[0])
            .ToDictionary(g => g.Key, g => g);

        var order = 1;
        foreach (var (topLevelPath, propertyLookups) in groups)
        {
            var folderProps = propertyLookups
                .First(p => p.GsiPath == topLevelPath);

            var fileName = Path.Combine(outputDir, $"{topLevelPath.ToLowerInvariant()}.md");
            using var writer = new StreamWriter(fileName, false, new UTF8Encoding(false));
            writer.WriteLine($"""
                              ---
                              title: {topLevelPath}
                              order: {order++}
                              authors:
                                - Aytackydln
                              ---

                              {folderProps.Description}
                              """);
            var tree = BuildTree(propertyLookups);
            WriteTreeMarkdown(tree, writer, 0);
        }
    }

    private sealed class Node
    {
        public required string Name;
        public string? Description;
        public readonly List<Node> Children = [];
        public bool IsFolder;
    }

    private static Node BuildTree(IEnumerable<PropertyLookup> props)
    {
        var root = new Node { Name = "root", IsFolder = true };
        foreach (var prop in props)
        {
            var parts = prop.GsiPath.Split('/');
            var current = root;
            for (var i = 1; i < parts.Length; i++)
            {
                var name = parts[i];
                var child = current.Children.FirstOrDefault(n => n.Name == name);
                if (child == null)
                {
                    child = new Node { Name = name, IsFolder = i < parts.Length - 1, Description = prop.Description };
                    current.Children.Add(child);
                }

                current = child;
            }
        }

        return root;
    }

    private static void WriteTreeMarkdown(Node node, StreamWriter writer, int indentCount)
    {
        var indent = new string(' ', indentCount * 2);
        foreach (var child in node.Children)
        {
            var prefix = indent + "- ";
            writer.WriteLine($"{prefix}**{child.Name}**");
            if (!string.IsNullOrEmpty(child.Description))
            {
                writer.WriteLine($"{indent}: {child.Description}");
            }

            if (child.Children.Count != 0)
                WriteTreeMarkdown(child, writer, indentCount + 1);
        }
    }
}
using System.Text;

namespace Frameworks.StateMachine.StateGraphVisualizer
{
    static class GraphvizFormatter
    {
        public static class ShapeType
        {
            public const string Rect = "rect";
            public const string Ellipse = "ellipse";
        }

        public static class Color
        {
            public const string Lightgrey = "lightgrey";
            public const string White = "white";
            public const string Red = "red";
            public const string Yellow = "yellow";
        }

        public static class Style
        {
            public const string Filled = "filled";
        }

        public static string FormatDigraph(string id, string label, string nodes)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("digraph G");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(FormatLineIndentation($"label=\"{label}\""));
            stringBuilder.AppendLine(FormatLineIndentation(nodes));
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        public static string FormatSubgraph(string id, string label, string nodes, string style = Style.Filled,
            string color = Color.White)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"subgraph cluster_{id}");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(FormatLineIndentation($"style=\"{style}\""));
            stringBuilder.AppendLine(FormatLineIndentation($"color=\"{color}\""));
            stringBuilder.AppendLine(FormatLineIndentation($"label=\"{label}\""));
            stringBuilder.AppendLine(FormatLineIndentation(nodes));
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        public static string FormatNode(string nodeId, string nodeLabel, string shapeType = ShapeType.Ellipse,
            string color = Color.White, string style = Style.Filled)
        {
            return $"\"{nodeId}\" [label=\"{nodeLabel}\" shape=\"{shapeType}\" color=\"{color}\" style=\"{style}\"]";
        }

        public static string JoinNodes(string fromNodeId, string toNodeId)
        {
            return $"\"{fromNodeId}\" -> \"{toNodeId}\"";
        }

        static string FormatLineIndentation(string text)
        {
            return $"\t{text.Replace("\n", "\n\t")}";
        }
    }
}
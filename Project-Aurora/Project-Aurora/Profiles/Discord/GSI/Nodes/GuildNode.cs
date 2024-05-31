namespace AuroraRgb.Profiles.Discord.GSI.Nodes {
    public class GuildNode : AutoJsonNode<GuildNode> {
        public long Id { get; set; }
        public string Name { get; set; }

        internal GuildNode(string json) : base(json) { }
    }
}

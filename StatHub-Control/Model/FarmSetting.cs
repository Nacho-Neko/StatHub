
namespace StatHub.Control.Model {
    internal class FarmSetting
    {
        public bool? node_agent { get; set; }
        public string farm { get; set; }
        public string reward_address { get; set; }
        public string? node_rpc_url { get; set; }
        public List<string> paths { get; set; }
        public List<string>? listen { get; set; }
        public List<string>? reserved_peers { get; set; }
        public ushort? farming_thread { get; set; }
        public ushort? plotting_thread { get; set; }
        public ushort? replotting_thread { get; set; }
        public bool? allow_private_ips { get; set; }
        public bool? disable_bootstrap_on_start { get; set; }
        public bool? farm_during_initial_plotting { get; set; }
    }
}

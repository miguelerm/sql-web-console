namespace SqlWebConsole.ApiModels
{
    public class ConnectionModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }
    }
}
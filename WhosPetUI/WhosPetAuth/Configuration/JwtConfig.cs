namespace WhosPetAuth.Configuration
{
    public class JwtConfig
    {
        public string Secret { get; set; }

        public TimeSpan Expiretime { get; set; }
    }
}

using System.Data.Entity.ModelConfiguration;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Data.Mappings
{
    public class HttpCallMap : EntityTypeConfiguration<HttpCall>
    {
        public HttpCallMap()
        {
            ToTable("Ent_Instrumentation.Http_Call_Log");
            HasKey(t => t.CallId);
        }
    }
}

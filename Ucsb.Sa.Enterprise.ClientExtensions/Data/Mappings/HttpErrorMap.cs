using System.Data.Entity.ModelConfiguration;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Data.Mappings
{
    public class HttpErrorMap : EntityTypeConfiguration<HttpError>
    {
        public HttpErrorMap()
        {
            ToTable("Ent_Instrumentation.Http_Error_Log");
            HasKey(t => t.ErrorId);
        }
    }
}

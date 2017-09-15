using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using Ucsb.Sa.Enterprise.ClientExtensions.Data.Mappings;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Data
{
	/// <summary>
	/// Db Context Class for Instument DB
	/// </summary>
	public class InstrumentationDbContext : DbContext
	{
		#region "constructor"

		static InstrumentationDbContext()
		{
			DbInterception.Add(new IsolationLevelInterceptor(IsolationLevel.ReadUncommitted));
		}

		/// <summary>
		/// Default Construtor and pass ConnectionString
		/// </summary>
		public InstrumentationDbContext()
			: base("Instrumentation")
		{

		}

		#endregion

		#region "DbSets Mappings"

		public DbSet<HttpCall> Calls { get; set; }
		public DbSet<HttpError> Errors { get; set; }

		#endregion

		#region "Model Binder"

		/// <summary>
		/// Model Binders to set mappings
		/// </summary>
		/// <param name="modelBuilder"></param>
		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Configurations.Add(new HttpCallMap());
			modelBuilder.Configurations.Add(new HttpErrorMap());

			base.OnModelCreating(modelBuilder);
		}

		#endregion
	}
}

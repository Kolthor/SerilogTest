using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace SerilogTest
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();

			// Serilog.Sinks.MSSqlServer ColumnOptions
			var columnOptions = new ColumnOptions();
			columnOptions.Store.Remove(StandardColumn.Properties);
			columnOptions.Store.Add(StandardColumn.LogEvent);
			columnOptions.Store.Add(StandardColumn.TraceId);
			columnOptions.Store.Add(StandardColumn.SpanId);

			// Configure Serilog
			builder.Services.AddLogging(builder =>
				builder.AddSerilog(
					logger: new LoggerConfiguration().Enrich.FromLogContext()
					                                 .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
					                                 .AuditTo.MSSqlServer(@"Server=.\SqlExpress;Database=__SerilogTest;Trusted_Connection=True;Encrypt=False;",
					                                                      new MSSqlServerSinkOptions
					                                                      {
						                                                      TableName = "EventLog",
						                                                      AutoCreateSqlDatabase = true,
						                                                      AutoCreateSqlTable = true,
					                                                      },
					                                                      columnOptions: columnOptions
					                                 ).CreateLogger(),
					dispose: true
				).Configure(options => options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId)
			);

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}
	}
}
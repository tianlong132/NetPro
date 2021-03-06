﻿using NetPro.Core.Configuration;
using NetPro.Core.Infrastructure;
using App.Metrics;
using App.Metrics.Extensions.Configuration;
using App.Metrics.Filtering;
using App.Metrics.Formatters.Json;
using App.Metrics.Health;
using App.Metrics.Health.Extensions.Configuration;
using App.Metrics.Health.Formatters.Ascii;
using App.Metrics.Reporting.InfluxDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using App.Metrics.Health.Checks.Sql;
using NetPro.Utility.Helpers;
using NetPro.Core.Consts;
using App.Metrics.Builder;

namespace NetPro.Web.Core.Infrastructure
{
    /// <summary>
    /// 应用性能监控
    /// </summary>
    public class AppMetricsStartup : INetProStartup
    {
        public int Order => 200;

        public void Configure(IApplicationBuilder application)
        {
            var config = EngineContext.Current.Resolve<NetProOption>();
            if (!config.APMEnabled) return;
            application.UseMetricsAllMiddleware();
            application.UseMetricsAllEndpoints();
            //健康检测
           // application.UseHealthAllEndpoints();
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, ITypeFinder typeFinder)
        {
            //TODO APM按需实现
            return;
            var config = services.BuildServiceProvider().GetRequiredService<NetProOption>();
            if (!config.APMEnabled) return;

            var connectionString = config.ConnectionStrings?.DecryptDefaultConnection;
            var influxOptions = new MetricsReportingInfluxDbOptions();
            configuration.GetSection(nameof(MetricsReportingInfluxDbOptions)).Bind(influxOptions);

            //var esOptions = new MetricsReportingElasticsearchOptions();
            //configuration.GetSection(nameof(MetricsReportingElasticsearchOptions)).Bind(esOptions);

            var metricsBuilder = AppMetrics.CreateDefaultBuilder()
                 .Configuration.ReadFrom(configuration);

            if (influxOptions != null) {
                metricsBuilder = metricsBuilder.Report.ToInfluxDb(influxOptions);
            }
            //if (esOptions!= null)
            //{
            //    metricsBuilder = metricsBuilder.Report.ToElasticsearch(esOptions);
            //}

            var  metricsRoot = metricsBuilder.Build();
            services.AddMetrics(metricsRoot);
            services.AddMetricsReportingHostedService();
            services.AddMetricsTrackingMiddleware(configuration);
            services.AddMetricsEndpoints(configuration);

            //健康检测
            //var metricsHealth = AppMetricsHealth.CreateDefaultBuilder()
            // .Configuration.ReadFrom(configuration)
            //.HealthChecks.RegisterFromAssembly(services)
            //.HealthChecks.AddPingCheck("ping 百度", "www.baidu.com", TimeSpan.FromSeconds(5))
            //.HealthChecks.AddHttpGetCheck("官网", new Uri("http://wwww.NetPro.com.cn"), TimeSpan.FromSeconds(30))          
            //.HealthChecks.AddProcessPhysicalMemoryCheck("占用内存是否超过1G", 1024*1024*1024)
            //.HealthChecks.AddSqlCheck("数据库连接检测", connectionString, TimeSpan.FromSeconds(60))
            //.BuildAndAddTo(services);

            //services.AddHealth(metricsHealth);
            //services.AddHealthEndpoints(configuration);

        }
    }
}

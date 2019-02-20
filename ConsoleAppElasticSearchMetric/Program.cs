using Metrics;
using Metrics.ElasticSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppElasticSearchMetric
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("done setting things start.");
            MetricsConfig();

            //MetricsNewConfig();

            Console.ReadLine();
        }

        #region [MetricsNewConfig]
        private static void MetricsNewConfig()
        {
            Metric.Config
                // Web监视仪表板，提供Metrics.NET度量值图表
                .WithHttpEndpoint("http://localhost:1234/metrics/")
                // 配置报表输出
                .WithReporting((rc) =>
                {
                    // 报表输出到控制台
                    rc.WithConsoleReport(TimeSpan.FromSeconds(5));
                });

            GaugeTest();
            CounterTest();
            HistogramTest();
            MeterTest();
            TimerTest();
            HealthCheckTest();
        }
        static Random ran = new Random(DateTime.Now.TimeOfDay.Milliseconds);

        static void GaugeTest()
        {
            Metric.Gauge("test.gauge", () => ran.NextDouble() * 1000, Unit.None);
        }

        static void CounterTest()
        {
            var counter = Metric.Counter("test.counter", Unit.Custom("并发"));

            Action doWork = () => { System.Threading.Thread.Sleep(ran.Next(10, 300)); };
            Action idlesse = () => { System.Threading.Thread.Sleep(ran.Next(0, 500)); };
            for (var i = 0; i < 20; i++)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        counter.Increment();
                        doWork();
                        counter.Decrement();
                        idlesse();
                    }
                });
            }
        }

        static void HistogramTest()
        {
            var histogram = Metric.Histogram("test.histogram", Unit.Custom("岁"), SamplingType.LongTerm);


            Task.Run(() =>
            {
                while (true)
                {
                    histogram.Update(ran.Next(10, 80), ran.Next(0, 2) > 0 ? "男" : "女");
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            });
        }

        static void MeterTest()
        {
            var meter = Metric.Meter("test.meter", Unit.Calls, TimeUnit.Seconds);

            Action idlesse = () => { System.Threading.Thread.Sleep(ran.Next(20, 50)); };
            Task.Run(() => {
                while (true)
                {
                    meter.Mark();
                    idlesse();
                }
            });
        }

        static void TimerTest()
        {
            var timer = Metric.Timer("test.meter", Unit.None, SamplingType.Default, TimeUnit.Seconds, TimeUnit.Microseconds);

            Action doWork = () => { System.Threading.Thread.Sleep(ran.Next(10, 300)); };
            Action idlesse = () => { System.Threading.Thread.Sleep(ran.Next(0, 500)); };
            for (var i = 0; i < 20; i++)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        timer.Time(doWork);
                        idlesse();
                    }
                });
            }
        }

        static void HealthCheckTest()
        {
            HealthChecks.RegisterHealthCheck("test.healthcheck", () =>
            {
                return ran.Next(100) < 5 ? HealthCheckResult.Unhealthy() : HealthCheckResult.Healthy();
            });
        }
        #endregion

        /// <summary>
        /// MetricsConfig
        /// </summary>
        private static void MetricsConfig()
        {
            //需要使用ES版本为5，不能使用6
            var esConfig = new ElasticReportsConfig() { Host = "127.0.0.1", Port = 9200, Index = "metrics" };
            Metric.Config
                // Web监视仪表板，提供Metrics.NET度量值图表，浏览器打开这个地址可以访问一个Metrics.NET内置的页面
                .WithHttpEndpoint("http://localhost:1234/metrics/")
                .WithReporting(config => 
                    config.WithElasticSearch(esConfig, TimeSpan.FromSeconds(5))
                );
            while (true)
            {
                ToElastic();
            }
        }

        /// <summary>
        /// ToElastic
        /// </summary>
        private static void ToElastic()
        {
            Random Ran = new Random();
            Metric.Gauge("Errors", () => Ran.Next(300, 500), Unit.None);
            Metric.Gauge("% Percent/Gauge|test", () => Ran.Next(0, 100), Unit.None);
            Metric.Gauge("& AmpGauge", () => Ran.Next(0, 1), Unit.None);
            Metric.Gauge("()[]{} ParantesisGauge", () => Ran.Next(22, 23), Unit.None);
            Metric.Gauge("Gauge With No Value", () => 0, Unit.None);
            Console.WriteLine("done setting things up");
            //Console.ReadKey();
        }
    }
}

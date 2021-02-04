using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
//using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace BenchHttp
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<HttpClientBenchmarks>();
        }
    }

    [Config(typeof(Config))]
    //[NativeMemoryProfiler]
    [MemoryDiagnoser]
    public class HttpClientBenchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                AddJob(Job.MediumRun.WithGcServer(true).WithGcForce(true).WithId("ServerForce"));
                AddJob(Job.MediumRun.WithGcServer(true).WithGcForce(false).WithId("Server"));
                AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(true).WithId("Workstation"));
                AddJob(Job.MediumRun.WithGcServer(false).WithGcForce(false).WithId("WorkstationForce"));
            }
        }
        private HttpClient _httpClient;

        [GlobalSetup]
        public void Setup()
        {
            _httpClient = new HttpClient();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _httpClient.Dispose();
        }

        [Benchmark]
        public async Task WithoutHttpCompletionOption()
        {
            var response = await _httpClient.GetAsync("http://openlibrary.org/search.json?q=tdd");

            response.EnsureSuccessStatusCode();

            if (response.Content is object)
            {
                var stream = await response.Content.ReadAsStreamAsync();

                var data = await JsonSerializer.DeserializeAsync<Search>(stream);
            }
        }

        public async Task TryFinallyExample()
        {
            var response = await _httpClient.GetAsync("http://openlibrary.org/search.json?q=tdd", HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            Search data = null;

            try
            {
                if (response.Content is object)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    data = await JsonSerializer.DeserializeAsync<Search>(stream);
                }
            }
            finally
            {
                response.Dispose();
            }

            if (data is object)
            {
                // intensive and slow processing of books list. We don't want this to delay releasing the connection.
            }
        }

        [Benchmark]
        public async Task WithHttpCompletionOption()
        {
            using var response = await _httpClient.GetAsync("http://openlibrary.org/search.json?q=tdd", HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            if (response.Content is object)
            {
                var stream = await response.Content.ReadAsStreamAsync();

                var data = await JsonSerializer.DeserializeAsync<Search>(stream);

                // do something with the data or return it
            }
        }

        // Simplified code
        [Benchmark]
        public async Task WithGetStreamAsync()
        {
            using var stream = await _httpClient.GetStreamAsync("http://openlibrary.org/search.json?q=tdd");

            var data = await JsonSerializer.DeserializeAsync<Search>(stream);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OmniSharp.Roslyn.CSharp.Services.Intellisense;
using TestUtility;
using Xunit;
using Xunit.Abstractions;

namespace OmniSharp.Roslyn.CSharp.Tests
{
    public class EmitFacts : AbstractSingleRequestHandlerTestFixture<EmitService>
    {
        public EmitFacts(ITestOutputHelper testOutput) : base(testOutput)
        {
        }

        protected override string EndpointName { get; } = OmniSharpEndpoints.Emit;

        [Fact]
        public async Task Console_app_project_can_be_emitted()
        {
            using (var testProject = await TestAssets.Instance.GetTestProjectAsync("HelloWorld"))
            using (var host = CreateOmniSharpHost(testProject.Directory))
            {
                var requestHandler = host.GetRequestHandler<EmitService>(OmniSharpEndpoints.Emit);

                var request = new EmitRequest();

                var response = await requestHandler.Handle(request);

                Assert.True(File.Exists($"{response.OutputPath}"));
            }
        }

        [Fact]
        public async Task Emitted_console_app_project_can_be_run_and_console_output_captured()
        {
            using (var host = CreateOmniSharpHost(@"c:\temp\netcoreapp2-console"))
            {
                var requestHandler = host.GetRequestHandler<EmitService>(OmniSharpEndpoints.Emit);

                var request = new EmitRequest();

                var response = await requestHandler.Handle(request);

                var process = new Process
                {
                    StartInfo =
                        new ProcessStartInfo
                        {
                            FileName = @"C:\Program Files\dotnet\dotnet.exe",
                            Arguments = $"run {response.OutputPath}",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            WorkingDirectory = Path.GetDirectoryName(response.OutputPath)
                        }
                };

                var output = new List<string>();
                var error = new List<string>();

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        output.Add(args.Data);
                        TestOutput.WriteLine(args.Data);
                    }
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        error.Add(args.Data);
                        TestOutput.WriteLine("ERROR: " + args.Data);
                    }
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                Assert.Contains("Hello World!", output);
            }
        }
    }
}

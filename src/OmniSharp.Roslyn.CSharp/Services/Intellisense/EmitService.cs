using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using OmniSharp.Mef;

namespace OmniSharp.Roslyn.CSharp.Services.Intellisense
{
    [OmniSharpHandler(OmniSharpEndpoints.Emit, LanguageNames.CSharp)]
    public class EmitService : IRequestHandler<EmitRequest, EmitResponse>
    {
        private readonly OmniSharpWorkspace _workspace;

        [ImportingConstructor]
        public EmitService(OmniSharpWorkspace workspace)
        {
            _workspace = workspace;
        }

        public async Task<EmitResponse> Handle(EmitRequest request)
        {
            var project = _workspace.CurrentSolution.Projects.Single();

            var compilation = await project.GetCompilationAsync();

            compilation.Emit(project.OutputFilePath);

            return new EmitResponse
            {
                OutputPath = project.OutputFilePath
            };
        }
    }

    public class EmitResponse
    {
        public string OutputPath { get; set; }
    }

    public class EmitRequest
    {
    }
}

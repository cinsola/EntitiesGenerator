using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.CodeGeneration;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace EntitiesGenerator
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(EntitiesGeneratorCodeRefactoringProvider)), Shared]
    internal class EntitiesGeneratorCodeRefactoringProvider : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a DAO node.
            var typeDecl = node as TypeDeclarationSyntax;
            if (typeDecl == null || !typeDecl.Identifier.Text.EndsWith("Dao"))
            {
                return;
            }

            // For any type declaration node, create a code action to reverse the identifier text.
            var action = CodeAction.Create("Generate an entity class for this DAO", c => CreateEntities(context.Document, typeDecl, c));

            context.RegisterRefactoring(action);
        }

        private async Task<Solution> CreateEntities(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            var daoObjectName = typeDecl.Identifier.ValueText;
            var entityFileName = daoObjectName.Substring(0, daoObjectName.Length - 3);




            var entityFile = EntityClassGenerator.CreateClassFileString(daoObjectName.Substring(0, daoObjectName.Length-3), typeDecl);
            var proj = document.Project;

            return proj.AddDocument(entityFileName, entityFile).Project.Solution;
        }
    }
}

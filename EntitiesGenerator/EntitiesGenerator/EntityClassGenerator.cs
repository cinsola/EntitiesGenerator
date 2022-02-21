using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn.CodeGeneration
{
    public static class EntityClassGenerator
    {
        public static string CreateClassFileString(string newClassName, TypeDeclarationSyntax daoClass)
        {
            var namespaceToImport = (daoClass.Parent as NamespaceDeclarationSyntax).Name as IdentifierNameSyntax;
            // Create a namespace: (namespace CodeGenerationSample)
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceToImport.Identifier.ValueText)).NormalizeWhitespace();

            // Add System using statement: (using System)
            @namespace = @namespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")));

            var classDeclaration = SyntaxFactory.ClassDeclaration(newClassName);
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            // Create a Property: (public int Quantity { get; set; })
            foreach (var prop in daoClass.Members)
            {
                var propSyntax = prop as PropertyDeclarationSyntax;
                if (propSyntax != null)
                {
                    var propertyDeclaration = SyntaxFactory.PropertyDeclaration(propSyntax.Type, propSyntax.Identifier)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                    classDeclaration = classDeclaration.AddMembers(propertyDeclaration);
                }
            }

            classDeclaration = classDeclaration.AddMembers(
                CreateCastMethod(daoClass, newClassName, "ToEntityObject", "entity", daoClass.Identifier.ValueText),
                CreateCastMethod(daoClass, daoClass.Identifier.ValueText, "ToDao", "entity", newClassName));


            @namespace = @namespace.AddMembers(classDeclaration);
            var code = @namespace
                .NormalizeWhitespace()
                .ToFullString();

            return code;
        }

        public static MethodDeclarationSyntax CreateCastMethod(TypeDeclarationSyntax daoClass,
            string returnTypeName, 
            string methodName, 
            string argName,
            string argType)
        {
            var parameterList = new List<ParameterSyntax>
            {
                SyntaxFactory.Parameter(SyntaxFactory.Identifier(argName)).WithType(SyntaxFactory.ParseTypeName(argType))
            };



            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseName(returnTypeName), methodName)
                .WithBody(GetMethodBody(daoClass, returnTypeName, argName))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddParameterListParameters(parameterList.ToArray())
                .NormalizeWhitespace();
        }

        public static BlockSyntax GetMethodBody(TypeDeclarationSyntax daoClass, string newClassName, string argName) 
        {
            var constructor = new List<SyntaxNodeOrToken>();
            foreach (var prop in daoClass.Members)
            {
                var propSyntax = prop as PropertyDeclarationSyntax;
                if (propSyntax != null)
                {
                    var assignmentNode = SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName(propSyntax.Identifier),
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(argName), SyntaxFactory.IdentifierName(propSyntax.Identifier)));
                    constructor.Add(SyntaxFactory.Token(SyntaxKind.CommaToken));
                    constructor.Add(assignmentNode);
                }
            }

            var returnStatement = SyntaxFactory.ReturnStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(newClassName))
                .WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression)
                .WithExpressions(SyntaxFactory.SeparatedList<ExpressionSyntax>(constructor.Skip(1).ToArray()))));

            return SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(returnStatement)).NormalizeWhitespace();
        }
    }
}
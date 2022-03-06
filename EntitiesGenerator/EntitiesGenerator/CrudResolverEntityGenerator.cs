using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EntitiesGenerator
{
    public class CrudResolverEntityGenerator
    {
        public static async Task<FileScope> CreateClassFileString(Document document, TypeDeclarationSyntax daoClass)
        {
            var template = CrudResolverGeneratorTemplate.Template;
            var primaryKey = GetPrimaryKey(daoClass);
            var dbContextName = GetDbContextName(daoClass);
            var tableName = await GetTableName(document, daoClass);
            var usings = GetUsing(daoClass);
            var rootNamespace = usings.Split('.').First();

            var createdFile = template.Replace("{USING}", usings)
                .Replace("{ROOTNAMESPACE}", rootNamespace)
                .Replace("{TABLENAME}", tableName)
                .Replace("{DBCONTEXT}", dbContextName)
                .Replace("{DAONAME}", daoClass.Identifier.ValueText)
                .Replace("{PRIMARYKEY}", primaryKey);

            return new FileScope
            {
                File = createdFile,
                Name = tableName
            };
        }

        public static string GetPrimaryKey(TypeDeclarationSyntax typeDecl)
        {


            foreach (var prop in typeDecl.Members)
            {
                if (prop.AttributeLists.First().Attributes.First().Name.ToString() == "Key")
                {
                    return (prop as PropertyDeclarationSyntax).Identifier.ValueText;
                }

            }

            return "Id";
        }

        public static string GetDbContextName(TypeDeclarationSyntax typeDecl)
        {
            var syntaxRoot = typeDecl.SyntaxTree.GetRoot();
            var classNode = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            return classNode.Identifier.ValueText;
        }

        public static async Task<string> GetTableName(Document document, TypeDeclarationSyntax typeDecl)
        {
            var model = await document.GetSemanticModelAsync();
            var ts = (ITypeSymbol) model.GetDeclaredSymbol(typeDecl);
            var tableAttrbitue = ts.GetAttributes().FirstOrDefault(x => x.AttributeClass.Name == "TableAttribute");
            
            return tableAttrbitue.ConstructorArguments.First().Value.ToString();
        }

        public static string GetUsing(TypeDeclarationSyntax typeDecl)
        {
            var syntaxRoot = typeDecl.SyntaxTree.GetRoot();
            return syntaxRoot.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First().Name.ToString();
        }

    }
}

namespace EntitiesGenerator
{
	public class CrudResolverGeneratorTemplate
	{
		public static string Template = @"
using {USING};
using Fnz.Utils.DataAccess;
using System;

namespace {ROOTNAMESPACE}.Services.{TABLENAME}.DataAccess
{
    public class {TABLENAME}Crud : DataAccessBase<{DBCONTEXT}>, ICrudResolver<{DAONAME}>
    {
        public {TABLENAME}Crud({DBCONTEXT} context) : 
            base(context)
        {
        }

        public int Add({DAONAME} entity)
        {
            Context.{TABLENAME}.Add(entity);
            Context.SaveChanges();

            return entity.{PRIMARYKEY}.Value;
        }

        public bool Delete(int id)
        {
            throw new NotImplementedException();
        }

        public {DAONAME} Get(int id)
        {
            return Context.{TABLENAME}.Find(id);
        }

        public int Update({DAONAME} entity)
        {
            Context.Update(entity);
            Context.SaveChanges();

            return entity.{PRIMARYKEY};
        }
    }
}";
	}
}
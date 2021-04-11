using FluentMigrator.Builders.Create.Table;

namespace Ekklesia.Api.Data
{
    public static class MigrationExtensions
    {
        public static ICreateTableColumnOptionOrWithColumnSyntax WithDefaultColumns(
            this ICreateTableWithColumnOrSchemaSyntax table)
        {
            return table.WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("CreatedOn").AsDateTime()
                .WithColumn("CreatedBy").AsString().Nullable()
                .WithColumn("ModifiedOn").AsDateTime()
                .WithColumn("ModifiedBy").AsString().Nullable();
        }
        
        public static ICreateTableColumnOptionOrWithColumnSyntax WithCompositeData(
            this ICreateTableColumnOptionOrWithColumnSyntax table)
        {
            return table.WithColumn("Data").AsString(int.MaxValue).Nullable();
        }
    }
}
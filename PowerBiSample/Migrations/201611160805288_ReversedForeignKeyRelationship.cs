namespace PowerBiSample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReversedForeignKeyRelationship : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "Profile_ProfileId", "dbo.Profiles");
            DropIndex("dbo.AspNetUsers", new[] { "Profile_ProfileId" });
            RenameColumn(table: "dbo.AspNetUsers", name: "Profile_ProfileId", newName: "ProfileId");
            AlterColumn("dbo.AspNetUsers", "ProfileId", c => c.Int(nullable: false));
            CreateIndex("dbo.AspNetUsers", "ProfileId");
            AddForeignKey("dbo.AspNetUsers", "ProfileId", "dbo.Profiles", "ProfileId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "ProfileId", "dbo.Profiles");
            DropIndex("dbo.AspNetUsers", new[] { "ProfileId" });
            AlterColumn("dbo.AspNetUsers", "ProfileId", c => c.Int());
            RenameColumn(table: "dbo.AspNetUsers", name: "ProfileId", newName: "Profile_ProfileId");
            CreateIndex("dbo.AspNetUsers", "Profile_ProfileId");
            AddForeignKey("dbo.AspNetUsers", "Profile_ProfileId", "dbo.Profiles", "ProfileId");
        }
    }
}

namespace PowerBiSample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedExternalTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUsers", "ProfileId", "dbo.Profiles");
            DropIndex("dbo.AspNetUsers", new[] { "ProfileId" });
            AddColumn("dbo.AspNetUsers", "PowerBiRole", c => c.String(nullable: false));
            DropColumn("dbo.AspNetUsers", "ProfileId");
            DropTable("dbo.Profiles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Profiles",
                c => new
                    {
                        ProfileId = c.Int(nullable: false, identity: true),
                        Role = c.String(),
                    })
                .PrimaryKey(t => t.ProfileId);
            
            AddColumn("dbo.AspNetUsers", "ProfileId", c => c.Int(nullable: false));
            DropColumn("dbo.AspNetUsers", "PowerBiRole");
            CreateIndex("dbo.AspNetUsers", "ProfileId");
            AddForeignKey("dbo.AspNetUsers", "ProfileId", "dbo.Profiles", "ProfileId", cascadeDelete: true);
        }
    }
}

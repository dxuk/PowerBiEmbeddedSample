namespace PowerBiSample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedPowerBiProfilesToProfiles : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PowerBiProfiles", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.PowerBiProfiles", new[] { "User_Id" });
            CreateTable(
                "dbo.Profiles",
                c => new
                    {
                        ProfileId = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128),
                        Role = c.String(),
                    })
                .PrimaryKey(t => t.ProfileId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            DropTable("dbo.PowerBiProfiles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PowerBiProfiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Role = c.String(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.Profiles", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Profiles", new[] { "UserId" });
            DropTable("dbo.Profiles");
            CreateIndex("dbo.PowerBiProfiles", "User_Id");
            AddForeignKey("dbo.PowerBiProfiles", "User_Id", "dbo.AspNetUsers", "Id");
        }
    }
}

namespace PowerBiSample.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedProfileToUser : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Profiles", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Profiles", new[] { "UserId" });
            AddColumn("dbo.AspNetUsers", "Profile_ProfileId", c => c.Int());
            CreateIndex("dbo.AspNetUsers", "Profile_ProfileId");
            AddForeignKey("dbo.AspNetUsers", "Profile_ProfileId", "dbo.Profiles", "ProfileId");
            DropColumn("dbo.Profiles", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Profiles", "UserId", c => c.String(maxLength: 128));
            DropForeignKey("dbo.AspNetUsers", "Profile_ProfileId", "dbo.Profiles");
            DropIndex("dbo.AspNetUsers", new[] { "Profile_ProfileId" });
            DropColumn("dbo.AspNetUsers", "Profile_ProfileId");
            CreateIndex("dbo.Profiles", "UserId");
            AddForeignKey("dbo.Profiles", "UserId", "dbo.AspNetUsers", "Id");
        }
    }
}

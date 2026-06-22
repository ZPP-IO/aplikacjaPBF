using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjectPBF.Data;

#nullable disable

namespace ProjectPBF.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260622210000_AddCampaignLibraryTemplates")]
    partial class AddCampaignLibraryTemplates
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
#pragma warning restore 612, 618
        }
    }
}

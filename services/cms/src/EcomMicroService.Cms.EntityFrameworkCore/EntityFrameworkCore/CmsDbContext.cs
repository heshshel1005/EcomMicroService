using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.CmsKit.Blogs;
using Volo.CmsKit.Comments;
using Volo.CmsKit.EntityFrameworkCore;
using Volo.CmsKit.GlobalResources;
using Volo.CmsKit.MarkedItems;
using Volo.CmsKit.MediaDescriptors;
using Volo.CmsKit.Menus;
using Volo.CmsKit.Pages;
using Volo.CmsKit.Ratings;
using Volo.CmsKit.Reactions;
using Volo.CmsKit.Tags;
using Volo.CmsKit.Users;

namespace EcomMicroService.Cms.EntityFrameworkCore;

[ReplaceDbContext(typeof(ICmsKitDbContext))]
[ConnectionStringName(EcomMicroServiceNames.CmsDb)]
public class CmsDbContext(DbContextOptions<CmsDbContext> options)
    : AbpDbContext<CmsDbContext>(options),
        ICmsKitDbContext,
        ICmsDbContext
{
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CmsUser> User { get; set; }
    public DbSet<UserReaction> Reactions { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<EntityTag> EntityTags { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<BlogFeature> BlogFeatures { get; set; }
    public DbSet<MediaDescriptor> MediaDescriptors { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<GlobalResource> GlobalResources { get; set; }
    public DbSet<UserMarkedItem> UserMarkedItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureCms();
        builder.ConfigureCmsKit();
    }
}

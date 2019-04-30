namespace BrandUp.Extensions.Migrations
{
    public interface IMigration
    {
        void Up();
        void Down();
    }
}
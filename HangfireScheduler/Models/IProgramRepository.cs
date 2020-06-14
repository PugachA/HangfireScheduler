namespace HangfireScheduler.Models
{
    public interface IRepository<T>
    {
        void AddOrUpdate(T value);
        void Delete(string key);
        T Get(string key);
    }
}
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using WeaverNet.Core.Infrastructure.Interfaces;

public abstract class LiteDBRepository<TData> : IRepository<TData, Guid>
{
    protected readonly string _dbPath;
    private readonly LiteDatabase _database;
    protected readonly ILiteCollection<TData> Collection;

    public event Action RepositoryChanged;

    public LiteDBRepository(string dbPath)
    {
        _dbPath = dbPath;

        // Bug fix: Create custom mapper and execute ConfigureMapper BEFORE opening LiteDatabase
        var mapper = new BsonMapper();
        ConfigureMapper(mapper);

        _database = new LiteDatabase(dbPath, mapper);
        Collection = _database.GetCollection<TData>("recordings");
    }

    public abstract void ConfigureMapper(BsonMapper mapper);
    public IReadOnlyCollection<TData> All => Collection.FindAll().ToList();

    protected IReadOnlyCollection<TVar> GetDisctinctValues<TVar>(Func<TData, TVar> selector)
    {
        return Collection.FindAll().Select(selector).Distinct().ToList();
    }
    public void Add(TData data)
    {
        Collection.Insert(data);
        NotifyRepositoryChanged();
    }

    public void AddOrReplace(TData data)
    {
        Collection.Upsert(data);
        NotifyRepositoryChanged();
    }

    public TData GetById(Guid id)
    {
        return Collection.FindById(new BsonValue(id));
    }

    public void Remove(Guid id)
    {
        Collection.Delete(new BsonValue(id));
        NotifyRepositoryChanged();
    }

    private void NotifyRepositoryChanged()
    {
        RepositoryChanged?.Invoke();
    }
}
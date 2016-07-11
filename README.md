# Object Build

Build, clone, and mutate mutable and immutable objects.

## Usage

Suppose we have an immutable type `FileKey`:
```
class FileKey
{
    public FileKey(int? accountId, DateTime? createTime)
    {
        AccountId = accountId;
        
        CreateTime = createTime;
    }
    
    public int? AccountId { get; }
    
    public DateTime? CreateTime { get; }
}
```
And an instance created:
```
var fileKeyA = new FileKey(123, new DateTime(2016, 7, 10));
```
Using the `Builder` class, we can make a clone easily:
```
var fileKeyB = new Builder<FileKey>(fileKeyA).Build();
```
We can make mutations to our clone fluently:
```
var fileKeyC = new Builder<FileKey>(fileKeyA)
    .Set(k => k.AccountId, 123)
    .Build();
```
We can make entirely new objects as well:
```
var fileKeyD = new Builder<FileKey>()
    .Set(k => k.AccountId, 234)
    .Set(k => k.CreateTime, new DateTime(2016, 7, 10))
    .Build();
```

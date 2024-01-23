using Silky.Hero.Common.EntityFrameworkCore.Entities;

namespace WsHostDemo;

public class Student : FullAuditedEntity
{
    public string Name { get; set; }
}
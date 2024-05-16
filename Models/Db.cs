using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class Db : DbContext
{
    public Db(DbContextOptions<Db> options) : base(options)
    {

    }
    public DbSet<Connection> Connections { get; set; }
}
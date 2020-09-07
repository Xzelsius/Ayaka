// Copyright (c) Raphael Strotz and other contributors under the MIT license. All rights reserved.

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xunit;

namespace Ayaka.Data
{
    public class EfRepositoryTests
    {
        [Fact]
        public void Can_find_entity()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: "Can_find_entity")
                .Options;

            int id;
            using (var context = new TestContext(options))
            {
                context.Set<TestEntity>().Add(new TestEntity {Name = "Test"});
                context.SaveChanges();

                id = context.Set<TestEntity>().Single().Id;
            }

            using (var context = new TestContext(options))
            {
                var repository = new EfRepository<TestEntity, int>(context);
                var entity = repository.Find(id);

                Assert.NotNull(entity);
                Assert.Equal("Test", entity.Name);
            }
        }

        [Fact]
        public async void Can_find_entity_async()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: "Can_find_entity_async")
                .Options;

            int id;
            using (var context = new TestContext(options))
            {
                context.Set<TestEntity>().Add(new TestEntity {Name = "Test"});
                await context.SaveChangesAsync();

                id = context.Set<TestEntity>().Single().Id;
            }

            using (var context = new TestContext(options))
            {
                var repository = new EfRepository<TestEntity, int>(context);
                var entity = await repository.FindAsync(id);

                Assert.NotNull(entity);
                Assert.Equal("Test", entity.Name);
            }
        }

        [Fact]
        public void Can_insert_entity()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: "Can_insert_entity")
                .Options;

            using (var context = new TestContext(options))
            {
                var repository = new EfRepository<TestEntity, int>(context);
                repository.Insert(new TestEntity {Name = "Test"});
            }

            using (var context = new TestContext(options))
            {
                Assert.Equal(1, context.Set<TestEntity>().Count());
                Assert.Equal("Test", context.Set<TestEntity>().Single().Name);
            }
        }

        [Fact]
        public async void Can_insert_entity_async()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: "Can_insert_entity_async")
                .Options;

            using (var context = new TestContext(options))
            {
                var repository = new EfRepository<TestEntity, int>(context);
                await repository.InsertAsync(new TestEntity {Name = "Test"});
            }

            using (var context = new TestContext(options))
            {
                Assert.Equal(1, context.Set<TestEntity>().Count());
                Assert.Equal("Test", context.Set<TestEntity>().Single().Name);
            }
        }

        [Fact]
        public void Can_update_entity()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: "Can_update_entity")
                .Options;

            using (var context = new TestContext(options))
            {
                context.Set<TestEntity>().Add(new TestEntity {Name = "Test"});
                context.SaveChanges();
            }

            using (var context = new TestContext(options))
            {
                var entity = context.Set<TestEntity>().Single();

                entity.Name = "Test (Updated)";

                var repository = new EfRepository<TestEntity, int>(context);
                repository.Update(entity);
            }

            using (var context = new TestContext(options))
            {
                Assert.Equal(1, context.Set<TestEntity>().Count());
                Assert.Equal("Test (Updated)", context.Set<TestEntity>().Single().Name);
            }
        }

        [Fact]
        public async void Can_update_entity_async()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: "Can_update_entity_async")
                .Options;

            using (var context = new TestContext(options))
            {
                context.Set<TestEntity>().Add(new TestEntity {Name = "Test"});
                await context.SaveChangesAsync();
            }

            using (var context = new TestContext(options))
            {
                var entity = context.Set<TestEntity>().Single();

                entity.Name = "Test (Updated)";

                var repository = new EfRepository<TestEntity, int>(context);
                await repository.UpdateAsync(entity);
            }

            using (var context = new TestContext(options))
            {
                Assert.Equal(1, context.Set<TestEntity>().Count());
                Assert.Equal("Test (Updated)", context.Set<TestEntity>().Single().Name);
            }
        }

        [Fact]
        public void Can_delete_entity()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: "Can_delete_entity")
                .Options;

            using (var context = new TestContext(options))
            {
                context.Set<TestEntity>().Add(new TestEntity {Name = "Test"});
                context.SaveChanges();
            }

            using (var context = new TestContext(options))
            {
                var entity = context.Set<TestEntity>().Single();

                var repository = new EfRepository<TestEntity, int>(context);
                repository.Delete(entity);
            }

            using (var context = new TestContext(options))
            {
                Assert.Equal(0, context.Set<TestEntity>().Count());
            }
        }

        [Fact]
        public async void Can_delete_entity_async()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: "Can_delete_entity_async")
                .Options;

            using (var context = new TestContext(options))
            {
                context.Set<TestEntity>().Add(new TestEntity {Name = "Test"});
                await context.SaveChangesAsync();
            }

            using (var context = new TestContext(options))
            {
                var entity = context.Set<TestEntity>().Single();

                var repository = new EfRepository<TestEntity, int>(context);
                await repository.DeleteAsync(entity);
            }

            using (var context = new TestContext(options))
            {
                Assert.Equal(0, context.Set<TestEntity>().Count());
            }
        }

        [Fact]
        public void Can_query_dbset()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
                .UseInMemoryDatabase(databaseName: "Can_query_dbset")
                .Options;

            using (var context = new TestContext(options))
            {
                context.Set<TestEntity>().AddRange(
                    new TestEntity {Name = "Test 1"},
                    new TestEntity {Name = "Test 2"}
                );
                context.SaveChanges();
            }

            using (var context = new TestContext(options))
            {
                var repository = new EfRepository<TestEntity, int>(context);
                var entities = repository.Table.ToList();

                Assert.NotNull(entities);
                Assert.Equal(2, entities.Count);
                Assert.All(entities, entity => Assert.StartsWith("Test", entity.Name));
            }
        }

        public class TestContext : DbContext
        {
            public TestContext(DbContextOptions options) : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ApplyConfiguration(new TestEntityMap());
                base.OnModelCreating(modelBuilder);
            }
        }

        public class TestEntity : IIdentifiable<int>
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class TestEntityMap : IEntityTypeConfiguration<TestEntity>
        {
            public void Configure(EntityTypeBuilder<TestEntity> builder)
            {
                builder.HasKey(e => e.Id);
                builder.Property(e => e.Name).IsRequired();
            }
        }
    }
}

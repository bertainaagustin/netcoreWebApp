using System;
using System.Threading.Tasks;
using AspNetCoreTodo.Data;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetCoreTodo.UnitTests
{
    public class TodoItemServiceShould
    {
        [Fact]
        public async Task AddNewItemAsIncompleteWithDueDate()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;

            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);

                 var fakeUser = new ApplicationUser
                 {
                     Id = "fake-000",
                     UserName = "fake@example.com"
                 };

                 await service.AddItemAsync(new TodoItem
                 {
                     Title = "Testing?",
                     DueAt = DateTimeOffset.Now.AddDays(3)
                 }, fakeUser);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var itemsInDatabase = await context.Items.CountAsync();
                Assert.Equal(1, itemsInDatabase);
                var item = await context.Items.FirstAsync();
                Assert.Equal("Testing?", item.Title);
                Assert.False(item.IsDone);
                // Item should be due 3 days from now (give or take a second)
                var difference = DateTimeOffset.Now.AddDays(3) - item.DueAt;
                Assert.True(difference < TimeSpan.FromSeconds(1));
            }
        }

        [Fact]
        public async Task MarkDoneAsyncCorrect()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_MarkDone_Correct").Options;
            var fakeUser = new ApplicationUser
            {
                Id = "fake-000",
                UserName = "fake@example.com"
            };
            var fakeItem =  new TodoItem
            {
                Title = "Testing?",
                DueAt = DateTimeOffset.Now.AddDays(3)
            };

            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);

                await service.AddItemAsync(fakeItem, fakeUser);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                var item = await context.Items.FirstAsync();

                var result = await service.MarkDoneAsync(item.Id, fakeUser);
                Assert.True(result);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var item = await context.Items.FirstAsync();
                Assert.True(item.IsDone);
            }
        }

        [Fact]
        public async Task MarkDoneAsyncIncorrectId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_MarkDone_Error").Options;
            var fakeUser = new ApplicationUser
            {
                Id = "fake-000",
                UserName = "fake@example.com"
            };
            var fakeItem =  new TodoItem
            {
                Title = "Testing?",
                DueAt = DateTimeOffset.Now.AddDays(3)
            };
            var fakeItemId = Guid.NewGuid();

            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);

                await service.AddItemAsync(fakeItem, fakeUser);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                var item = await context.Items.FirstAsync();

                var result = await service.MarkDoneAsync(fakeItemId, fakeUser);
                Assert.False(result);
            }
        }

        [Fact]
        public async Task GetIncompleteItemsAsyncUserCheck() 
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_GetIncompleteItems").Options;
            var fakeUser1 = new ApplicationUser
            {
                Id = "fake-000",
                UserName = "fake@example.com"
            };
            
            var fakeUser2 = new ApplicationUser 
            {
                Id = "fake-999",
                UserName = "fake2@example.com"
            };

            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);
                await service.AddItemAsync(new TodoItem
                {
                    Title = "Testing?",
                    DueAt = DateTimeOffset.Now.AddDays(3)
                }, fakeUser1);

                await service.AddItemAsync(new TodoItem
                {
                    Title = "Testing?",
                    DueAt = DateTimeOffset.Now.AddDays(3)
                }, fakeUser2);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);

                var items = await service.GetIncompleteItemsAsync(fakeUser1);

                foreach(var item in items)
                {
                    Assert.Equal(item.UserId, fakeUser1.Id);
                }
            }
        }
    }
}
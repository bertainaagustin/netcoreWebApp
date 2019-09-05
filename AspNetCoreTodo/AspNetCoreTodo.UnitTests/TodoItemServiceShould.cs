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

            ClearDataBase(options);

            
        }

        [Fact]
        public async Task MarkDoneAsyncCorrect()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_MarkDone").Options;
            var doneResult = false;
            var fakeUser = new ApplicationUser
            {
                Id = "fake-000",
                UserName = "fake@example.com"
            };
            var fakeItem =  NewTodoItem("Testing", fakeUser.Id, DateTimeOffset.Now.AddDays(3));

            using (var context = new ApplicationDbContext(options))
            {
                context.Items.Add(fakeItem);
                context.SaveChanges();

                var service = new TodoItemService(context);        
                doneResult = await service.MarkDoneAsync(fakeItem.Id, fakeUser);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var item = await context.Items.FirstAsync();
                Assert.True(doneResult);
                Assert.True(item.IsDone);
            }

            ClearDataBase(options);
        }

        [Fact]
        public async Task MarkDoneAsyncIncorrectId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: "Test_MarkDone").Options;
            var doneResult = false;
            var fakeUser = new ApplicationUser
            {
                Id = "fake-000",
                UserName = "fake@example.com"
            };
            var fakeItem =  NewTodoItem("Testing", fakeUser.Id, DateTimeOffset.Now.AddDays(3));
            var randomId = Guid.NewGuid();

            using (var context = new ApplicationDbContext(options))
            {
                context.Items.Add(fakeItem);
                context.SaveChanges();
                var service = new TodoItemService(context);
                var item = await context.Items.FirstAsync();

                doneResult = await service.MarkDoneAsync(randomId, fakeUser);
            }

            using (var context = new ApplicationDbContext(options))
            {
                var item = await context.Items.FirstAsync();
                Assert.False(doneResult);
                Assert.False(item.IsDone);
            }

            ClearDataBase(options);
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
            
            var itemComplete = NewTodoItem("Testing", fakeUser1.Id, DateTimeOffset.Now.AddDays(3), true);
            var itemIncomplete = NewTodoItem("Testing", fakeUser1.Id, DateTimeOffset.Now.AddDays(3));
            var itemOtherUser = NewTodoItem("Testing", fakeUser2.Id, DateTimeOffset.Now.AddDays(3));
            using (var context = new ApplicationDbContext(options))
            {
                context.Items.Add(itemComplete);
                context.Items.Add(itemIncomplete);
                context.Items.Add(itemOtherUser);

                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(context);

                var items = await service.GetIncompleteItemsAsync(fakeUser1);

                Assert.Single(items);
                Assert.Equal(itemIncomplete.Id, items[0].Id);
                Assert.False(items[0].IsDone);
            }
            ClearDataBase(options);
        }

         private async void ClearDataBase(DbContextOptions<ApplicationDbContext> options)
         {
             using (var context = new ApplicationDbContext(options))
             {
                 await context.Database.EnsureDeletedAsync();
             }
         }

         private TodoItem NewTodoItem (string title, string userId, DateTimeOffset dueAt, bool isDone = false)
         {
             return new TodoItem {
                Id = Guid.NewGuid(),
                Title = title,
                DueAt = dueAt,
                IsDone = isDone,
                UserId = userId
             };
         }
    }
}
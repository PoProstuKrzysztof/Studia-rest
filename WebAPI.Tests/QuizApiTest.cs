using ApplicationCore.Data;
using Infrastructure.EF.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using WebAPI.DTO;
using Xunit;

namespace WebAPI.Tests;


public class QuizApiTest : IClassFixture<QuizAppTestFactory<Program>>
{

    private readonly HttpClient _client;
    private readonly QuizAppTestFactory<Program> _app;
    private readonly QuizDbContext _context;
    public QuizApiTest(QuizAppTestFactory<Program> app)
    {
        _app = app;
        _client = app.CreateClient();
        using (var scope = app.Services.CreateScope())
        {
            _context = scope.ServiceProvider.GetService<QuizDbContext>();
            var items = new HashSet<QuizItemEntity>
            {
                new()
                {
                    Id = 1, CorrectAnswer = "7", Question = "2 + 5", IncorrectAnswers =
                        new HashSet<QuizItemAnswerEntity>
                        {
                            new() {Id = 11, Answer = "5"},
                            new() {Id = 12, Answer = "4"},
                            new() {Id = 13, Answer = "3"},
                        }
                },
                 new()
                {
                    Id = 2, CorrectAnswer = "4", Question = "2 + 5", IncorrectAnswers =
                        new HashSet<QuizItemAnswerEntity>
                        {
                            new() {Id = 14, Answer = "5"},
                            new() {Id = 15, Answer = "1"},
                            new() {Id = 16, Answer = "4"},
                        }
                },
                  new()
                {
                    Id = 3, CorrectAnswer = "3", Question = "2 + 5", IncorrectAnswers =
                        new HashSet<QuizItemAnswerEntity>
                        {
                            new() {Id = 17, Answer = "5"},
                            new() {Id = 18, Answer = "4"},
                            new() {Id = 19, Answer = "4"},
                        }
                }

            };
            if (_context.Quizzes.Count() == 0)
            {
                _context.Quizzes.Add(
                    new QuizEntity
                    {
                        Id = 1,
                        Items = items,
                        Title = "Matematyka"
                    }
                );
                _context.SaveChanges();
            }
        }
    }




    [Fact]
    public async void GetShouldReturnOneQuizz()
    {
        //Arrange
        //await using var application = new WebApplicationFactory<Program>();
        //using var client = application.CreateClient();

        //Act
        var result = await _client.GetFromJsonAsync<List<QuizDto>>( "/Quizes" );

        //Assert
        if (result != null)
        {
            Assert.Single( result );
            Assert.Equal( "Matematyka", result[0].Title );
        }
    }

    [Fact]
    public async void GetShouldReturnOkStatus()
    {
        //Arrange
        //await using var application = new WebApplicationFactory<Program>();
        //using var client = application.CreateClient();

        //Act
        var result = await _client.GetAsync( "/Quizes" );

        //Assert
        Assert.Equal( HttpStatusCode.OK, result.StatusCode );
        Assert.Contains( "application/json", result.Content.Headers.GetValues( "Content-Type" ).First() );
    }
}